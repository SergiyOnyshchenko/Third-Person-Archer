using TMPro;
using UnityEngine;

/// <summary>Mission panel roster row: enemy type, body shots and head shots with the player's weapon.</summary>
public sealed class EnemyShotsRowView : MonoBehaviour
{
    [SerializeField] private TMP_Text _name;
    [SerializeField] private TMP_Text _body;
    [SerializeField] private TMP_Text _head;

    public void Bind(EnemyShotsPreview preview)
    {
        if (_name != null) _name.text = MissionPreviewUtility.GetEnemyName(preview.Type);

        if (_body != null)
        {
            _body.text = MissionPreviewUtility.FormatShots(preview.BodyShots);
            _body.color = MissionPreviewUtility.GetShotsColor(preview.BodyShots);
        }

        if (_head != null)
        {
            _head.text = MissionPreviewUtility.FormatShots(preview.HeadShots);
            _head.color = MissionPreviewUtility.GetShotsColor(preview.HeadShots);
        }
    }
}
