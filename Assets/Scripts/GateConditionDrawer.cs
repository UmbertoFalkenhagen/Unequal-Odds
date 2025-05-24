// GateConditionDrawer.cs  (Editor assembly)
// -----------------------------------------------------------------------------
// Custom PropertyDrawer so designers can nest AND/OR groups and pick enum
// values from a context-sensitive dropdown.
// -----------------------------------------------------------------------------

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnequalOdds.GameData;

[CustomPropertyDrawer(typeof(GateCondition))]
public class GateConditionDrawer : PropertyDrawer
{
    private const float INDENT = 12f;

    public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
    {
        bool isGroup = prop.FindPropertyRelative("isGroup").boolValue;
        if (!isGroup)
            return EditorGUIUtility.singleLineHeight * 3.5f;

        // group: include children heights
        var childrenProp = prop.FindPropertyRelative("children");
        float h = EditorGUIUtility.singleLineHeight * 3; // toggles + header
        for (int i = 0; i < childrenProp.arraySize; i++)
            h += GetPropertyHeight(childrenProp.GetArrayElementAtIndex(i), label) + 2;
        return h + 4;
    }

    public override void OnGUI(Rect rect, SerializedProperty prop, GUIContent label)
    {
        EditorGUI.BeginProperty(rect, label, prop);
        var isGroupProp = prop.FindPropertyRelative("isGroup");

        // ------ toggle row ------
        Rect line = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
        isGroupProp.boolValue = EditorGUI.ToggleLeft(line, "Group (AND/OR)", isGroupProp.boolValue);

        if (isGroupProp.boolValue)
            DrawGroup(rect, prop, line);
        else
            DrawLeaf(rect, prop, line);

        EditorGUI.EndProperty();
    }

    // ---------------------------------------------------------------------
    private void DrawGroup(Rect rect, SerializedProperty prop, Rect headerLine)
    {
        var opProp = prop.FindPropertyRelative("groupOp");
        var childrenProp = prop.FindPropertyRelative("children");

        // operator dropdown
        headerLine.y += EditorGUIUtility.singleLineHeight + 2;
        EditorGUI.PropertyField(headerLine, opProp);

        // children list
        headerLine.y += EditorGUIUtility.singleLineHeight + 4;
        for (int i = 0; i < childrenProp.arraySize; i++)
        {
            var childProp = childrenProp.GetArrayElementAtIndex(i);
            float h = GetPropertyHeight(childProp, GUIContent.none);
            Rect childRect = new Rect(headerLine.x + INDENT, headerLine.y, headerLine.width - INDENT, h);
            EditorGUI.PropertyField(childRect, childProp, GUIContent.none, true);
            headerLine.y += h + 2;
        }

        // + / – buttons
        Rect plusBtn = new Rect(headerLine.x, headerLine.y, 24, EditorGUIUtility.singleLineHeight);
        if (GUI.Button(plusBtn, "+"))
            childrenProp.arraySize += 1;

        Rect minusBtn = new Rect(headerLine.x + 28, headerLine.y, 24, EditorGUIUtility.singleLineHeight);
        if (GUI.Button(minusBtn, "–") && childrenProp.arraySize > 0)
            childrenProp.arraySize -= 1;
    }

    // ---------------------------------------------------------------------
    private void DrawLeaf(Rect rect, SerializedProperty prop, Rect headerLine)
    {
        var attrProp = prop.FindPropertyRelative("attribute");
        var maskProp = prop.FindPropertyRelative("allowedMask");

        // attribute dropdown
        headerLine.y += EditorGUIUtility.singleLineHeight + 2;
        EditorGUI.PropertyField(headerLine, attrProp);

        // enum mask — show if an attribute is chosen
        if ((AttributeKey)attrProp.enumValueIndex == AttributeKey.None) return;

        headerLine.y += EditorGUIUtility.singleLineHeight + 2;
        var key = (AttributeKey)attrProp.enumValueIndex;

        switch (key)
        {
            case AttributeKey.BirthWealth:
                maskProp.intValue = (int)(BirthWealthClass)
                    EditorGUI.EnumFlagsField(headerLine,
                    (BirthWealthClass)maskProp.intValue);
                break;
            case AttributeKey.CountryContext:
                maskProp.intValue = (int)(CountryContext)
                    EditorGUI.EnumFlagsField(headerLine,
                    (CountryContext)maskProp.intValue);
                break;
            case AttributeKey.Locale:
                maskProp.intValue = (int)(Locale)
                    EditorGUI.EnumFlagsField(headerLine,
                    (Locale)maskProp.intValue);
                break;
            case AttributeKey.SkinColourEthnicPos:
                maskProp.intValue = (int)(SkinColour)
                    EditorGUI.EnumFlagsField(headerLine,
                    (SkinColour)maskProp.intValue);
                break;
            case AttributeKey.GenderIdentity:
                maskProp.intValue = (int)(GenderIdentity)
                    EditorGUI.EnumFlagsField(headerLine,
                    (GenderIdentity)maskProp.intValue);
                break;
            case AttributeKey.SexualOrientation:
                maskProp.intValue = (int)(SexualOrientation)
                    EditorGUI.EnumFlagsField(headerLine,
                    (SexualOrientation)maskProp.intValue);
                break;
            case AttributeKey.DisabilityStatus:
                maskProp.intValue = (int)(DisabilityStatus)
                    EditorGUI.EnumFlagsField(headerLine,
                    (DisabilityStatus)maskProp.intValue);
                break;
            case AttributeKey.ParentsEducation:
                maskProp.intValue = (int)(ParentsEducation)
                    EditorGUI.EnumFlagsField(headerLine,
                    (ParentsEducation)maskProp.intValue);
                break;
            case AttributeKey.FirstLanguageAlign:
                maskProp.intValue = (int)(FirstLanguageAlignment)
                    EditorGUI.EnumFlagsField(headerLine,
                    (FirstLanguageAlignment)maskProp.intValue);
                break;
            case AttributeKey.MigrationStatus:
                maskProp.intValue = (int)(MigrationCitizenshipStatus)
                    EditorGUI.EnumFlagsField(headerLine,
                    (MigrationCitizenshipStatus)maskProp.intValue);
                break;
        }
    }
}
#endif
