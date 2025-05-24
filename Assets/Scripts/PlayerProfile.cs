// -----------------------------------------------------------------------------
// Immutable identity fields at birth, plus turn counter.
// BirthWealthClass is kept mutable so events can move the player up or down.
// -----------------------------------------------------------------------------

using UnequalOdds.GameData;

namespace UnequalOdds.Gameplay
{
    [System.Serializable]
    public class PlayerProfile
    {
        // -------------- identity (initially rolled, but BirthWealthClass can change) --------------
        public BirthWealthClass birthWealth;
        public CountryContext countryContext;
        public Locale locale;
        public SkinColour skin;
        public GenderIdentity genderIdentity;
        public SexualOrientation sexualOrientation;
        public DisabilityStatus disabilityStatus;
        public ParentsEducation parentsEducation;
        public FirstLanguageAlignment firstLang;
        public MigrationCitizenshipStatus migrationStatus;

        // -------------- runtime bookkeeping --------------
        public int turnIndex = 0;  // incremented each completed turn

        // Constructors ----------------------------------------------------------
        public PlayerProfile(
            BirthWealthClass wealth,
            CountryContext ctx,
            Locale loc,
            SkinColour skinColour,
            GenderIdentity gender,
            SexualOrientation orient,
            DisabilityStatus disab,
            ParentsEducation parentEd,
            FirstLanguageAlignment langAlign,
            MigrationCitizenshipStatus migStatus)
        {
            birthWealth = wealth;
            countryContext = ctx;
            locale = loc;
            skin = skinColour;
            genderIdentity = gender;
            sexualOrientation = orient;
            disabilityStatus = disab;
            parentsEducation = parentEd;
            firstLang = langAlign;
            migrationStatus = migStatus;
        }

        // Empty ctor for manual assignment in Inspector / tests
        public PlayerProfile() { }
    }
}