using UnityEngine;
using UnequalOdds.GameData;

namespace UnequalOdds.Gameplay
{
    public static class ProfileFactory
    {
        public static PlayerProfile CreateRandom()
        {
            return new PlayerProfile
            {
                birthWealth = (BirthWealthClass)Random.Range(0, 3),
                countryContext = (CountryContext)Random.Range(0, 3),
                locale = (Locale)Random.Range(0, 3),
                skin = (SkinColour)Random.Range(0, 2),
                genderIdentity = (GenderIdentity)Random.Range(0, 5),
                sexualOrientation = (SexualOrientation)Random.Range(0, 2),
                disabilityStatus = (DisabilityStatus)Random.Range(0, 3),
                parentsEducation = (ParentsEducation)Random.Range(0, 3),
                firstLang = (FirstLanguageAlignment)Random.Range(0, 2),
                migrationStatus = (MigrationCitizenshipStatus)Random.Range(0, 3),
                turnIndex = 0
            };
        }
    }
}