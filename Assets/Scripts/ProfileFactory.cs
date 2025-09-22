using System;
using System.Linq;
using UnityEngine;
using UnequalOdds.GameData;
using UnequalOdds.Gameplay;

namespace UnequalOdds.Runtime
{
    public static class ProfileFactory
    {
        /// <summary>Create a fully valid random profile (no zero flags).</summary>
        public static PlayerProfile CreateRandom()
        {
            var p = new PlayerProfile
            {
                birthWealth = PickSingleFlag<BirthWealthClass>(),
                countryContext = PickSingleFlag<CountryContext>(),
                locale = PickSingleFlag<Locale>(),
                skin = PickSingleFlag<SkinColour>(),
                genderIdentity = PickSingleFlag<GenderIdentity>(),
                sexualOrientation = PickSingleFlag<SexualOrientation>(),
                disabilityStatus = PickSingleFlag<DisabilityStatus>(),
                parentsEducation = PickSingleFlag<ParentsEducation>(),
                firstLang = PickSingleFlag<FirstLanguageAlignment>(),
                migrationStatus = PickSingleFlag<MigrationCitizenshipStatus>()
            };
            return p;
        }

        /// <summary>Ensure each field is a single valid flag. If not, assign one.</summary>
        public static void EnsureValid(ref PlayerProfile p)
        {
            if (!IsSingleFlag(p.birthWealth)) p.birthWealth = PickSingleFlag<BirthWealthClass>();
            if (!IsSingleFlag(p.countryContext)) p.countryContext = PickSingleFlag<CountryContext>();
            if (!IsSingleFlag(p.locale)) p.locale = PickSingleFlag<Locale>();
            if (!IsSingleFlag(p.skin)) p.skin = PickSingleFlag<SkinColour>();
            if (!IsSingleFlag(p.genderIdentity)) p.genderIdentity = PickSingleFlag<GenderIdentity>();
            if (!IsSingleFlag(p.sexualOrientation)) p.sexualOrientation = PickSingleFlag<SexualOrientation>();
            if (!IsSingleFlag(p.disabilityStatus)) p.disabilityStatus = PickSingleFlag<DisabilityStatus>();
            if (!IsSingleFlag(p.parentsEducation)) p.parentsEducation = PickSingleFlag<ParentsEducation>();
            if (!IsSingleFlag(p.firstLang)) p.firstLang = PickSingleFlag<FirstLanguageAlignment>();
            if (!IsSingleFlag(p.migrationStatus)) p.migrationStatus = PickSingleFlag<MigrationCitizenshipStatus>();
        }

        // ---- helpers ----
        private static TEnum PickSingleFlag<TEnum>() where TEnum : struct, Enum
        {
            var vals = Enum.GetValues(typeof(TEnum)).Cast<TEnum>()
                .Where(v => IsSingleFlag(v))  // excludes 0 and multi-bit
                .ToArray();

            if (vals.Length == 0)
                throw new Exception($"No single-flag values defined for {typeof(TEnum).Name}");

            return vals[UnityEngine.Random.Range(0, vals.Length)];
        }

        private static bool IsSingleFlag<TEnum>(TEnum value) where TEnum : struct, Enum
        {
            ulong u = Convert.ToUInt64(value);
            return u != 0 && (u & (u - 1)) == 0; // exactly one bit set
        }
    }
}
