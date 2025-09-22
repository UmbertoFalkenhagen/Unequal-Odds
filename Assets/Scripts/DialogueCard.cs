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
        public LogicalOp groupOp = LogicalOp.And;

        [SerializeReference] public List<GateCondition> children = new List<GateCondition>();

        // ----- leaf comparison fields -----
        public AttributeKey attribute = AttributeKey.None;

        // Bit-mask of acceptable enum indices (0-based). Allows multiple values.
        // Example for BirthWealth: 001 = Low, 010 = Middle, 100 = High.
        public int allowedMask = 0;

        // ===============================================================
        public bool Evaluate(PlayerProfile p)
        {
            if (isGroup)
            {
                if (children.Count == 0) return true;

                bool result = groupOp == LogicalOp.And;
                foreach (var child in children)
                {
                    bool ok = child.Evaluate(p);
                    result = groupOp == LogicalOp.And ? result && ok : result || ok;
                }
                return result;
            }

            // ----- leaf comparison -----
            int playerIndex = attribute switch
            {
                AttributeKey.BirthWealth => (int)p.birthWealth,
                AttributeKey.CountryContext => (int)p.countryContext,
                AttributeKey.Locale => (int)p.locale,
                AttributeKey.SkinColourEthnicPos => (int)p.skin,
                AttributeKey.GenderIdentity => (int)p.genderIdentity,
                AttributeKey.SexualOrientation => (int)p.sexualOrientation,
                AttributeKey.DisabilityStatus => (int)p.disabilityStatus,
                AttributeKey.ParentsEducation => (int)p.parentsEducation,
                AttributeKey.FirstLanguageAlign => (int)p.firstLang,
                AttributeKey.MigrationStatus => (int)p.migrationStatus,
                _ => 0
            };

            int playerBit = 1 << playerIndex;
            return (allowedMask & playerBit) != 0;      // true if mask covers player value
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
        SkinColourEthnicPos,
        GenderIdentity,
        SexualOrientation,
        DisabilityStatus,
        ParentsEducation,
        FirstLanguageAlign,
        MigrationStatus
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
