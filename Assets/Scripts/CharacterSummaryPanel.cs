using System.Text;
using TMPro;
using UnityEngine;
using UnequalOdds.Gameplay;
using UnequalOdds.Runtime; // EnumDisplayNames, PlayerProfile

namespace UnequalOdds.UI
{
    public class CharacterSummaryPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text bodyText;   // assign BodyText in Inspector

        // Call whenever you want to refresh the view
        public void Bind(PlayerProfile p)
        {
            if (!bodyText || p == null) return;
            bodyText.text = BuildSummary(p);
        }

        public void Show(bool show) => gameObject.SetActive(show);

        private static string D<T>(T v) where T : System.Enum
            => EnumDisplayNames.ToDisplay(v);

        private static string BuildSummary(PlayerProfile p)
        {
            var sb = new StringBuilder(256);
            sb.AppendLine($"Birth wealth: {D(p.birthWealth)}");
            sb.AppendLine($"Country context: {D(p.countryContext)}");
            sb.AppendLine($"Locale: {D(p.locale)}");
            sb.AppendLine($"Skin / ethnic position: {D(p.skin)}");
            sb.AppendLine($"Gender identity: {D(p.genderIdentity)}");
            sb.AppendLine($"Sexual orientation: {D(p.sexualOrientation)}");
            sb.AppendLine($"Disability / chronic health: {D(p.disabilityStatus)}");
            sb.AppendLine($"Parents’ education: {D(p.parentsEducation)}");
            sb.AppendLine($"First language: {D(p.firstLang)}");
            sb.AppendLine($"Migration / citizenship: {D(p.migrationStatus)}");
            return sb.ToString();
        }
    }
}
