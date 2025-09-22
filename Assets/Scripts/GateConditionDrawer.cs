#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnequalOdds.GameData;

[CustomPropertyDrawer(typeof(GateCondition))]
public class GateConditionDrawer : PropertyDrawer
{
    private const float INDENT = 12f;

    public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
    {
        if (prop == null) return EditorGUIUtility.singleLineHeight;

        var isGroupProp = prop.FindPropertyRelative("isGroup");
        if (isGroupProp == null)
            return EditorGUIUtility.singleLineHeight * 2f; // minimal help

        bool isGroup = isGroupProp.boolValue;

        if (!isGroup)
        {
            // leaf: Group toggle + attribute + (maybe) mask
            var attrProp = prop.FindPropertyRelative("attribute");
            float lines = 2f; // toggle + attribute
            if (attrProp != null && (AttributeKey)attrProp.enumValueIndex != AttributeKey.None)
                lines += 1f; // mask line
            return EditorGUIUtility.singleLineHeight * (lines + 0.5f);
        }

        // group: Group toggle + operator + each child + +/- buttons
        var childrenProp = prop.FindPropertyRelative("children");
        float h = EditorGUIUtility.singleLineHeight * 2.5f; // toggle + op
        if (childrenProp != null)
        {
            for (int i = 0; i < childrenProp.arraySize; i++)
            {
                var childProp = childrenProp.GetArrayElementAtIndex(i);
                // if child is null managed ref, reserve one line for the "null child" UI
                if (childProp == null || string.IsNullOrEmpty(childProp.managedReferenceFullTypename))
                {
                    h += EditorGUIUtility.singleLineHeight + 4f;
                }
                else
                {
                    // FIX: use EditorGUI.GetPropertyHeight(..., includeChildren)
                    h += EditorGUI.GetPropertyHeight(childProp, true) + 4f;
                }
            }
        }
        h += EditorGUIUtility.singleLineHeight + 4f; // + / ? row
        return h;
    }

    public override void OnGUI(Rect rect, SerializedProperty prop, GUIContent label)
    {
        if (prop == null)
        {
            EditorGUI.HelpBox(rect, "Gate is null (no property).", MessageType.Warning);
            return;
        }

        EditorGUI.BeginProperty(rect, label, prop);

        // line 1: group toggle
        var isGroupProp = prop.FindPropertyRelative("isGroup");
        if (isGroupProp == null)
        {
            EditorGUI.HelpBox(rect, "Gate data missing (isGroup). Try re-creating the card option.", MessageType.Warning);
            EditorGUI.EndProperty();
            return;
        }

        Rect line = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
        isGroupProp.boolValue = EditorGUI.ToggleLeft(line, "Group (AND/OR)", isGroupProp.boolValue);

        if (isGroupProp.boolValue)
            DrawGroup(rect, prop, line);
        else
            DrawLeaf(rect, prop, line);

        EditorGUI.EndProperty();
    }

    // ------------------------ group node ------------------------
    private void DrawGroup(Rect rect, SerializedProperty prop, Rect line)
    {
        var opProp = prop.FindPropertyRelative("groupOp");
        var childrenProp = prop.FindPropertyRelative("children");

        // operator dropdown
        line.y += EditorGUIUtility.singleLineHeight + 2f;
        if (opProp != null)
            EditorGUI.PropertyField(line, opProp);
        else
            EditorGUI.LabelField(line, "groupOp (missing)");

        // children
        line.y += EditorGUIUtility.singleLineHeight + 4f;
        if (childrenProp != null)
        {
            for (int i = 0; i < childrenProp.arraySize; i++)
            {
                var childProp = childrenProp.GetArrayElementAtIndex(i);
                float childHeight = childProp != null && !string.IsNullOrEmpty(childProp.managedReferenceFullTypename)
                    ? EditorGUI.GetPropertyHeight(childProp, true)   // FIX here too
                    : EditorGUIUtility.singleLineHeight;

                Rect childRect = new Rect(line.x + INDENT, line.y, line.width - INDENT, childHeight);

                if (childProp == null || string.IsNullOrEmpty(childProp.managedReferenceFullTypename))
                {
                    // Null managed reference entry: offer to create default
                    EditorGUI.HelpBox(childRect, "Null child. Click 'Fix' to add a default gate.", MessageType.Info);
                    Rect fixBtn = new Rect(childRect.xMax - 60f, childRect.y + 2f, 56f, EditorGUIUtility.singleLineHeight);
                    if (GUI.Button(fixBtn, "Fix"))
                    {
                        EnsureChildExists(childrenProp, i);
                    }
                }
                else
                {
                    EditorGUI.PropertyField(childRect, childProp, GUIContent.none, true);
                }

                line.y += childHeight + 4f;
            }
        }

        // +/- buttons
        Rect plusBtn = new Rect(line.x, line.y, 24f, EditorGUIUtility.singleLineHeight);
        Rect minusBtn = new Rect(line.x + 28f, line.y, 24f, EditorGUIUtility.singleLineHeight);

        if (GUI.Button(plusBtn, "+"))
        {
            if (childrenProp != null)
            {
                int idx = childrenProp.arraySize;
                childrenProp.arraySize = idx + 1;
                EnsureChildExists(childrenProp, idx); // instantiate default GateCondition
            }
        }

        if (GUI.Button(minusBtn, "–"))
        {
            if (childrenProp != null && childrenProp.arraySize > 0)
            {
                childrenProp.arraySize -= 1;
            }
        }
    }

    private static void EnsureChildExists(SerializedProperty childrenProp, int index)
    {
        var child = childrenProp.GetArrayElementAtIndex(index);
        if (child != null && string.IsNullOrEmpty(child.managedReferenceFullTypename))
        {
            // Assign a default instance to the managed reference slot
            child.managedReferenceValue = new GateCondition();
        }
    }

    // ------------------------ leaf node ------------------------
    private void DrawLeaf(Rect rect, SerializedProperty prop, Rect line)
    {
        var attrProp = prop.FindPropertyRelative("attribute");
        var maskProp = prop.FindPropertyRelative("allowedMask");

        // attribute dropdown
        line.y += EditorGUIUtility.singleLineHeight + 2f;
        if (attrProp != null)
            EditorGUI.PropertyField(line, attrProp);
        else
            EditorGUI.LabelField(line, "attribute (missing)");

        if (attrProp == null) return;

        var key = (AttributeKey)attrProp.enumValueIndex;
        if (key == AttributeKey.None) return;

        // enum mask
        line.y += EditorGUIUtility.singleLineHeight + 2f;
        if (maskProp == null)
        {
            EditorGUI.LabelField(line, "allowedMask (missing)");
            return;
        }

        switch (key)
        {
            case AttributeKey.BirthWealth:
                maskProp.intValue = (int)(BirthWealthClass)EditorGUI.EnumFlagsField(line, (BirthWealthClass)maskProp.intValue);
                break;
            case AttributeKey.CountryContext:
                maskProp.intValue = (int)(CountryContext)EditorGUI.EnumFlagsField(line, (CountryContext)maskProp.intValue);
                break;
            case AttributeKey.Locale:
                maskProp.intValue = (int)(Locale)EditorGUI.EnumFlagsField(line, (Locale)maskProp.intValue);
                break;
            case AttributeKey.SkinColourEthnicPos:
                maskProp.intValue = (int)(SkinColour)EditorGUI.EnumFlagsField(line, (SkinColour)maskProp.intValue);
                break;
            case AttributeKey.GenderIdentity:
                maskProp.intValue = (int)(GenderIdentity)EditorGUI.EnumFlagsField(line, (GenderIdentity)maskProp.intValue);
                break;
            case AttributeKey.SexualOrientation:
                maskProp.intValue = (int)(SexualOrientation)EditorGUI.EnumFlagsField(line, (SexualOrientation)maskProp.intValue);
                break;
            case AttributeKey.DisabilityStatus:
                maskProp.intValue = (int)(DisabilityStatus)EditorGUI.EnumFlagsField(line, (DisabilityStatus)maskProp.intValue);
                break;
            case AttributeKey.ParentsEducation:
                maskProp.intValue = (int)(ParentsEducation)EditorGUI.EnumFlagsField(line, (ParentsEducation)maskProp.intValue);
                break;
            case AttributeKey.FirstLanguageAlign:
                maskProp.intValue = (int)(FirstLanguageAlignment)EditorGUI.EnumFlagsField(line, (FirstLanguageAlignment)maskProp.intValue);
                break;
            case AttributeKey.MigrationStatus:
                maskProp.intValue = (int)(MigrationCitizenshipStatus)EditorGUI.EnumFlagsField(line, (MigrationCitizenshipStatus)maskProp.intValue);
                break;
            default:
                EditorGUI.LabelField(line, key.ToString());
                break;
        }
    }
}
#endif
