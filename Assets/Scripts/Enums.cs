using System;

namespace UnequalOdds.GameData
{
    /// <summary>Family wealth at birth.</summary>
    [Flags]
    public enum BirthWealthClass
    {
        Low = 1 << 0,
        Middle = 1 << 1,
        High = 1 << 2
    }

    /// <summary>Overall political stability of the country the player is in.</summary>
    [Flags]
    public enum CountryContext
    {
        StableDemocracy = 1 << 0,
        FragileState = 1 << 1,
        ActiveConflictZone = 1 << 2
    }

    /// <summary>Kind of settlement where the player lives.</summary>
    [Flags]
    public enum Locale
    {
        MajorCity = 1 << 0,
        SmallTownSuburb = 1 << 1,
        RuralRemote = 1 << 2
    }

    /// <summary>Player’s position in the region’s colour / ethnic hierarchy.</summary>
    [Flags]
    public enum SkinColour
    {
        White = 1 << 0,
        PersonOfColor = 1 << 1
    }

    /// <summary>Player’s gender identity.</summary>
    [Flags]
    public enum GenderIdentity
    {
        CisMan = 1 << 0,
        CisWoman = 1 << 1,
        NonBinary = 1 << 2,
        TransMan = 1 << 3,
        TransWoman = 1 << 4
    }

    /// <summary>Player’s sexual orientation.</summary>
    [Flags]
    public enum SexualOrientation
    {
        Heterosexual = 1 << 0,
        NonHeterosexual = 1 << 1
    }

    /// <summary>Whether the player has a disability or chronic condition.</summary>
    [Flags]
    public enum DisabilityStatus
    {
        AbleBodied = 1 << 0,
        InvisibleDisability = 1 << 1,
        VisibleDisability = 1 << 2   // mobility, sensory, etc.
    }

    /// <summary>Highest education level attained by at least one parent.</summary>
    [Flags]
    public enum ParentsEducation
    {
        Primary = 1 << 0,
        Secondary = 1 << 1,
        Tertiary = 1 << 2
    }

    /// <summary>Alignment with the dominant language in the country.</summary>
    [Flags]
    public enum FirstLanguageAlignment
    {
        Fluent = 1 << 0,
        LimitedAccented = 1 << 1
    }

    /// <summary>Legal / migration status.</summary>
    [Flags]
    public enum MigrationCitizenshipStatus
    {
        Citizen = 1 << 0,
        MigrantWorker = 1 << 1,
        RefugeeAsylumSeeker = 1 << 2
    }
}
