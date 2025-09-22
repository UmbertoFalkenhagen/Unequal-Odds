using System;
using System.Text;
using UnequalOdds.GameData;

namespace UnequalOdds.Runtime
{
    public static class EnumDisplayNames
    {
        public static string ToDisplay<TEnum>(TEnum value) where TEnum : Enum
        {
            // Fall back if somehow 0 sneaks in
            ulong u = Convert.ToUInt64(value);
            if (u == 0) return "Unassigned";

            string s = value.ToString();

            // Exact replacements for your names
            switch (s)
            {
                case "SmallTownSuburb": return "Small town / Suburb";
                case "RuralRemote": return "Rural / Remote";
                case "PersonOfColor": return "Person of Color";
                case "AbleBodied": return "Able-bodied";
                case "InvisibleDisability": return "Invisible disability";
                case "VisibleDisability": return "Visible disability";
                case "NonHeterosexual": return "Non-heterosexual";
                case "StableDemocracy": return "Stable democracy";
                case "FragileState": return "Fragile state";
                case "ActiveConflictZone": return "Active conflict zone";
                case "LimitedAccented": return "Limited / Accented";
                case "RefugeeAsylumSeeker": return "Refugee / Asylum seeker";
            }

            // Generic PascalCase ? spaced
            return InsertSpaces(s);
        }

        private static string InsertSpaces(string s)
        {
            var sb = new StringBuilder(s.Length + 8);
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (i > 0 && char.IsUpper(c) && (char.IsLower(s[i - 1]) || (i + 1 < s.Length && char.IsLower(s[i + 1]))))
                    sb.Append(' ');
                sb.Append(c);
            }
            return sb.ToString();
        }
    }
}
