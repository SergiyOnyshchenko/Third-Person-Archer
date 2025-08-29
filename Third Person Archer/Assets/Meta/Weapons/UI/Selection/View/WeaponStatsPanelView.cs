using UnityEngine;

namespace Meta.Weapons.UI.Selection
{
    public class WeaponStatsPanelView : MonoBehaviour
    {
        [SerializeField] private WeaponStatSliderView damageSlider;
        [SerializeField] private WeaponStatSliderView balanceSlider;
        [SerializeField] private WeaponStatSliderView distanceSlider;
        [SerializeField] private WeaponStatSliderView ammoCountSlider;
        [SerializeField] private WeaponStatSliderView reloadTimeSlider;
        [SerializeField] private WeaponStatSliderView zoomSlider;

        public void Set(UIStatNormalizationConfig norm, UIStyleConfig style, WeaponStats selected, bool showComparison, WeaponStats equipped)
        {
            // Normalized values
            float sD = norm.Normalize(WeaponStat.Damage,     selected.Damage);
            float sB = norm.Normalize(WeaponStat.Balance,    selected.Balance);
            float sR = norm.Normalize(WeaponStat.Distance,   selected.Distance);
            float sA = norm.Normalize(WeaponStat.AmmoCount,  selected.AmmoCount);
            float sT = norm.Normalize(WeaponStat.ReloadTime, selected.ReloadTime);
            float sZ = norm.Normalize(WeaponStat.Zoom,       selected.Zoom);

            float eD = norm.Normalize(WeaponStat.Damage,     equipped.Damage);
            float eB = norm.Normalize(WeaponStat.Balance,    equipped.Balance);
            float eR = norm.Normalize(WeaponStat.Distance,   equipped.Distance);
            float eA = norm.Normalize(WeaponStat.AmmoCount,  equipped.AmmoCount);
            float eT = norm.Normalize(WeaponStat.ReloadTime, equipped.ReloadTime);
            float eZ = norm.Normalize(WeaponStat.Zoom,       equipped.Zoom);

            // Base labels
            damageSlider.SetLabel("DAMAGE");        balanceSlider.SetLabel("BALANCE");
            distanceSlider.SetLabel("DISTANCE");    ammoCountSlider.SetLabel("MAG");
            reloadTimeSlider.SetLabel("RELOAD TIME"); zoomSlider.SetLabel("ZOOM");

            // Delta (real units, not normalized)
            float dD = selected.Damage     - equipped.Damage;
            float dB = selected.Balance    - equipped.Balance;
            float dR = selected.Distance   - equipped.Distance;
            float dA = selected.AmmoCount  - equipped.AmmoCount;
            float dT = selected.ReloadTime - equipped.ReloadTime; // NOTE: negative is better
            float dZ = selected.Zoom       - equipped.Zoom;

            // Formatting helpers
            Color green = style.ComparisonHigherColor;
            Color red   = style.ComparisonLowerColor;
            Color neutral = new Color(1f,1f,1f,0.7f);

            void SetSlider(WeaponStatSliderView s, string baseText, float delta, string unit, bool lessIsBetter,
                     float selectedNorm, float equippedNorm)
            {
                bool improved = lessIsBetter ? (delta < -1e-4f) : (delta > 1e-4f);
                bool worsened = lessIsBetter ? (delta >  1e-4f) : (delta < -1e-4f);

                string deltaText = !showComparison ? string.Empty
                    : (Mathf.Abs(delta) < 1e-4f ? string.Empty
                    : (delta > 0 ? $" +{AbsFmt(delta)}{unit}" : $" {AbsFmt(delta)}{unit}"));

                s.SetNumeric(baseText, deltaText, improved ? green : (worsened ? red : neutral));
                
                if (showComparison)
                {
                    s.SetBars(equippedNorm, selectedNorm, style.PrimaryFillColor, green, red, allowNegativeDelta: false);
                }
                else
                {
                    s.SetBars(0f, selectedNorm, style.PrimaryFillColor, style.PrimaryFillColor, style.PrimaryFillColor, allowNegativeDelta: false);
                }
            }

            string AbsFmt(float v) => Mathf.Abs(v) >= 10f ? Mathf.RoundToInt(v).ToString("0")
                                   : Mathf.Abs(v) >= 1f  ? v.ToString("0.##")
                                                         : v.ToString("0.###");

            SetSlider(damageSlider,     $"{selected.Damage:0}",      dD, "",    false, sD, eD);
            SetSlider(balanceSlider,    $"{selected.Balance:0}%",    dB, "%",   false, sB, eB);
            SetSlider(distanceSlider,   $"{selected.Distance:0}m",   dR, "m",   false, sR, eR);
            SetSlider(ammoCountSlider,  $"{selected.AmmoCount:0}",   dA, "",    false, sA, eA);
            SetSlider(reloadTimeSlider, $"{selected.ReloadTime:0.0}s", dT, "s", true,  sT, eT); // less is better -> green on negative
            SetSlider(zoomSlider,       $"{selected.Zoom:0.#}x",     dZ, "x",   false, sZ, eZ);
        }

        public void SetUpgradePreview(UIStatNormalizationConfig norm, UIStyleConfig style,
                                      WeaponStats current, WeaponStats after)
        {
            // Normalized (ReloadTime is inverted in Normalize)
            float cD = norm.Normalize(WeaponStat.Damage,     current.Damage);
            float cB = norm.Normalize(WeaponStat.Balance,    current.Balance);
            float cR = norm.Normalize(WeaponStat.Distance,   current.Distance);
            float cA = norm.Normalize(WeaponStat.AmmoCount,  current.AmmoCount);
            float cT = norm.Normalize(WeaponStat.ReloadTime, current.ReloadTime);
            float cZ = norm.Normalize(WeaponStat.Zoom,       current.Zoom);

            float aD = norm.Normalize(WeaponStat.Damage,     after.Damage);
            float aB = norm.Normalize(WeaponStat.Balance,    after.Balance);
            float aR = norm.Normalize(WeaponStat.Distance,   after.Distance);
            float aA = norm.Normalize(WeaponStat.AmmoCount,  after.AmmoCount);
            float aT = norm.Normalize(WeaponStat.ReloadTime, after.ReloadTime);
            float aZ = norm.Normalize(WeaponStat.Zoom,       after.Zoom);

            // Base labels
            damageSlider.SetLabel("DAMAGE");        balanceSlider.SetLabel("BALANCE");
            distanceSlider.SetLabel("DISTANCE");    ammoCountSlider.SetLabel("MAG");
            reloadTimeSlider.SetLabel("RELOAD TIME"); zoomSlider.SetLabel("ZOOM");

            // Delta (real units, not normalized)
            float dD = after.Damage     - current.Damage;
            float dB = after.Balance    - current.Balance;
            float dR = after.Distance   - current.Distance;
            float dA = after.AmmoCount  - current.AmmoCount;
            float dT = after.ReloadTime - current.ReloadTime; // NOTE: negative is better
            float dZ = after.Zoom       - current.Zoom;

            // Formatting helpers
            Color green = style.ComparisonHigherColor;
            Color red   = style.ComparisonLowerColor;
            Color neutral = new Color(1f,1f,1f,0.7f);

            void Set(WeaponStatSliderView s, string baseText, float delta, string unit, bool lessIsBetter,
                     float nFrom, float nTo)
            {
                bool improved = lessIsBetter ? (delta < -1e-4f) : (delta > 1e-4f);
                bool worsened = lessIsBetter ? (delta >  1e-4f) : (delta < -1e-4f);

                string deltaText = Mathf.Abs(delta) < 1e-4f ? string.Empty
                    : (delta > 0 ? $" +{AbsFmt(delta)}{unit}" : $" {AbsFmt(delta)}{unit}"); // delta already has sign

                s.SetNumeric(baseText, deltaText, improved ? green : (worsened ? red : neutral));
                s.SetBars(nFrom, nTo, style.PrimaryFillColor, green, red, allowNegativeDelta:false); // overlay only for improvements
            }

            string AbsFmt(float v) => Mathf.Abs(v) >= 10f ? Mathf.RoundToInt(v).ToString("0")
                                   : Mathf.Abs(v) >= 1f  ? v.ToString("0.##")
                                                         : v.ToString("0.###");

            Set(damageSlider,     $"{current.Damage:0}",      dD, "",    false, cD, aD);
            Set(balanceSlider,    $"{current.Balance:0}%",    dB, "%",   false, cB, aB);
            Set(distanceSlider,   $"{current.Distance:0}m",   dR, "m",   false, cR, aR);
            Set(ammoCountSlider,  $"{current.AmmoCount:0}",   dA, "",    false, cA, aA);
            Set(reloadTimeSlider, $"{current.ReloadTime:0.0}s", dT, "s", true,  cT, aT); // less is better -> green on negative
            Set(zoomSlider,       $"{current.Zoom:0.#}x",     dZ, "x",   false, cZ, aZ);
        }
    }
}