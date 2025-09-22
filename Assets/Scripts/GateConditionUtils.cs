using System;
using System.Collections.Generic;

namespace UnequalOdds.GameData
{
    public static class GateConditionUtils
    {
        public static bool IsEmpty(GateCondition g)
        {
            if (g == null) return true;
            if (!g.isGroup) return g.attribute == AttributeKey.None;
            if (g.children == null || g.children.Count == 0) return true;
            foreach (var c in g.children) if (!IsEmpty(c)) return false;
            return true;
        }

        public static string ToReadable(GateCondition g)
        {
            if (g == null || IsEmpty(g)) return "No specific background";
            return g.isGroup ? GroupToReadable(g) : LeafToReadable(g.attribute, g.allowedMask);
        }

        private static string GroupToReadable(GateCondition g)
        {
            var parts = new List<string>();
            if (g.children != null)
            {
                foreach (var c in g.children)
                {
                    if (IsEmpty(c)) continue;
                    parts.Add(c.isGroup ? $"({ToReadable(c)})" : ToReadable(c));
                }
            }

            if (parts.Count == 0) return "No specific background";

            // No null check needed; enums are non-nullable
            string opName = g.groupOp.ToString();
            bool isAnd = opName.Equals("And", StringComparison.OrdinalIgnoreCase);

            var opStr = isAnd ? " AND " : " OR ";
            return string.Join(opStr, parts);
        }

        // GateConditionUtils.cs (only the LeafToReadable switch)
        private static string LeafToReadable(AttributeKey key, int mask)
        {
            switch (key)
            {
                case AttributeKey.BirthWealth:
                    return NamesFromMask((BirthWealthClass)mask, "Birth wealth");
                case AttributeKey.CountryContext:
                    return NamesFromMask((CountryContext)mask, "Country context");
                case AttributeKey.Locale:
                    return NamesFromMask((Locale)mask, "Locale");
                case AttributeKey.SkinColour:
                    return NamesFromMask((SkinColour)mask, "Ethnic position");
                case AttributeKey.GenderIdentity:
                    return NamesFromMask((GenderIdentity)mask, "Gender identity");
                case AttributeKey.SexualOrientation:
                    return NamesFromMask((SexualOrientation)mask, "Sexual orientation");
                case AttributeKey.DisabilityStatus:
                    return NamesFromMask((DisabilityStatus)mask, "Disability");
                case AttributeKey.ParentsEducation:
                    return NamesFromMask((ParentsEducation)mask, "Parents’ education");
                case AttributeKey.FirstLanguageAlignment:
                    return NamesFromMask((FirstLanguageAlignment)mask, "First language");
                case AttributeKey.MigrationCitizenshipStatus:
                    return NamesFromMask((MigrationCitizenshipStatus)mask, "Migration/citizenship");
                default:
                    return "Background condition";
            }
        }


        private static string NamesFromMask<TEnum>(TEnum mask, string label) where TEnum : Enum
        {
            ulong bits = Convert.ToUInt64(mask);
            if (bits == 0) return label;

            var names = new List<string>();
            foreach (TEnum val in Enum.GetValues(typeof(TEnum)))
            {
                ulong flag = Convert.ToUInt64(val);
                if (flag != 0 && (bits & flag) == flag)
                    names.Add(val.ToString().Replace('_', ' '));
            }

            return $"{label}: {string.Join(" OR ", names)}";
        }
    }
}