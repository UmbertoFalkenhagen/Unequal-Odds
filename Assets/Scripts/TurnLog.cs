using System;
using System.Collections.Generic;
using UnequalOdds.GameData;

namespace UnequalOdds.Runtime
{
    [Serializable]
    public class LoggedOption
    {
        public string text;
        public OptionKind kind;        // Privilege / Disadvantage
        public bool hadBackground;     // gate was true for player
        public bool lockedIn;          // contributed to dice math
        public int targetShift;
        public int rollBonus;
    }

    [Serializable]
    public class TurnLogEntry
    {
        public int turnIndex;
        public LifeGoal goal;
        public string cardTitle;

        public int baseTarget;
        public int adjustedTarget;
        public int die;
        public int rollMod;
        public bool success;

        public List<LoggedOption> options = new();
    }
}
