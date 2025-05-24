using System;


namespace UnequalOdds.GameData {
    /// <summary>Family wealth at birth.</summary>
    public enum BirthWealthClass
    {
        Low,
        Middle,
        High
    }

    /// <summary>Overall political stability of the country the player is in.</summary>
    public enum CountryContext
    {
        StableDemocracy,
        FragileState,
        ActiveConflictZone
    }

    /// <summary>Kind of settlement where the player lives.</summary>
    public enum Locale
    {
        MajorCity,
        SmallTownSuburb,
        RuralRemote
    }

    /// <summary>Player’s position in the region’s colour / ethnic hierarchy.</summary>
    public enum SkinColour
    {
        White,
        PersonOfColor
    }

    /// <summary>Player’s gender identity.</summary>
    public enum GenderIdentity
    {
        CisMan,
        CisWoman,
        NonBinary,
        TransMan,
        TransWoman
    }

    /// <summary>Player’s sexual orientation.</summary>
    public enum SexualOrientation
    {
        Heterosexual,
        NonHeterosexual
    }

    /// <summary>Whether the player has a disability or chronic condition.</summary>
    public enum DisabilityStatus
    {
        AbleBodied,
        InvisibleDisability,
        VisibleDisability   // mobility, sensory, etc.
    }

    /// <summary>Highest education level attained by at least one parent.</summary>
    public enum ParentsEducation
    {
        Primary,
        Secondary,
        Tertiary
    }

    /// <summary>Alignment with the dominant language in the country.</summary>
    public enum FirstLanguageAlignment
    {
        Fluent,
        LimitedAccented
    }

    /// <summary>Legal / migration status.</summary>
    public enum MigrationCitizenshipStatus
    {
        Citizen,
        MigrantWorker,
        RefugeeAsylumSeeker
    }
}
