// DialogueCard.cs  (runtime assembly)
// -----------------------------------------------------------------------------
// Data model for life-goal dialogue cards with nested gate logic.
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;
using UnequalOdds.Gameplay;   // PlayerProfile & enums

namespace UnequalOdds.GameData
{

    [CreateAssetMenu(fileName = "New Dialogue Card",
                     menuName = "Cards/Dialogue Card")]
    public class DialogueCard : ScriptableObject
    {
        // ----- meta -----
        public LifeGoal goal;
        public string cardTitle;
        [TextArea(4, 8)] public string cardBody;

        // ----- dice -----
        [Range(1, 6)] public int baseTarget = 4;

        // ----- options -----
        public List<CardOption> options = new List<CardOption>();
    }

    // =======================================================================
    [Serializable]
    public class CardOption
    {
        [TextArea(2, 3)] public string text;

        public OptionKind kind = OptionKind.Privilege;   // <— NEW

        public GateCondition gate = new GateCondition();

        public int rollBonus = 0;
        public int targetShift = 0;

        // You can keep forcedDrawback if you still need it for special cases,
        // but it is no longer required for the “background-based disadvantage”.
        public bool forcedDrawback = false;
    }

    // =======================================================================
    /// <summary>
    /// Recursive expression tree:
    /// • If isGroup == true ? evaluate children with op (AND / OR).
    /// • Else ? compare one attribute to one enum value.
    /// </summary>
    [Serializable]
    public class GateCondition
    {
        public bool isGroup = false;
        public LogicalOp groupOp = LogicalOp.And;   // or whatever your enum is
        public List<GateCondition> children = new List<GateCondition>();

        public AttributeKey attribute = AttributeKey.None;
        public int allowedMask = 0;                     // bitmask of allowed enum flags

        public bool Evaluate(UnequalOdds.Gameplay.PlayerProfile p)
        {
            if (isGroup)
            {
                if (children == null || children.Count == 0) return true;

                bool andGroup = groupOp.ToString().Equals("And", StringComparison.OrdinalIgnoreCase);
                bool acc = andGroup ? true : false;

                foreach (var c in children)
                {
                    bool v = c != null && c.Evaluate(p);
                    acc = andGroup ? (acc && v) : (acc || v);
                }
                return acc;
            }
            else
            {
                // Treat empty leaf (no attribute selected or mask not set) as pass-through
                if (attribute == AttributeKey.None || allowedMask == 0) return true;

                switch (attribute)
                {
                    case AttributeKey.BirthWealth:
                        return MaskMatch(allowedMask, p.birthWealth);
                    case AttributeKey.CountryContext:
                        return MaskMatch(allowedMask, p.countryContext);
                    case AttributeKey.Locale:
                        return MaskMatch(allowedMask, p.locale);
                    case AttributeKey.SkinColour:
                        return MaskMatch(allowedMask, p.skin);                 // <-- your field
                    case AttributeKey.GenderIdentity:
                        return MaskMatch(allowedMask, p.genderIdentity);
                    case AttributeKey.SexualOrientation:
                        return MaskMatch(allowedMask, p.sexualOrientation);
                    case AttributeKey.DisabilityStatus:
                        return MaskMatch(allowedMask, p.disabilityStatus);
                    case AttributeKey.ParentsEducation:
                        return MaskMatch(allowedMask, p.parentsEducation);
                    case AttributeKey.FirstLanguageAlignment:
                        return MaskMatch(allowedMask, p.firstLang);            // <-- your field
                    case AttributeKey.MigrationCitizenshipStatus:
                        return MaskMatch(allowedMask, p.migrationStatus);
                    default:
                        return true;
                }
            }
        }

        private static bool MaskMatch<TEnum>(int mask, TEnum value) where TEnum : Enum
        {
            int v = Convert.ToInt32(value);
            return (mask & v) != 0;
        }
    }

    // =======================================================================
    public enum LogicalOp { And, Or }
    public enum OptionKind { Privilege, Disadvantage }
    public enum AttributeKey
    {
        None,
        BirthWealth,
        CountryContext,
        Locale,
        SkinColour,
        GenderIdentity,
        SexualOrientation,
        DisabilityStatus,
        ParentsEducation,
        FirstLanguageAlignment,
        MigrationCitizenshipStatus
    }

    public enum LifeGoal
    {
        LivableJob,
        SafeHousing,
        MaintainHealth,
        FinancialCushion,
        CivicVoice
    }
}
