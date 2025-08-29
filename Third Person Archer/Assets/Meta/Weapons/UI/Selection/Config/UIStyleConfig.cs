using UnityEngine;

namespace Meta.Weapons.UI.Selection
{
    [CreateAssetMenu(menuName = "Meta/Weapons/UI/Style", fileName = "UIStyle")]
    public class UIStyleConfig : ScriptableObject
    {
        [Header("Comparison Colors")]
        public Color ComparisonHigherColor = new(0.25f, 0.85f, 0.25f); // green
        public Color ComparisonLowerColor  = new(0.90f, 0.25f, 0.25f); // red

        [Header("Slider Fill Colors")]
        public Color PrimaryFillColor = Color.white;
        public Color BackgroundFillColor = new(1f, 1f, 1f, 0.15f);
    }
}