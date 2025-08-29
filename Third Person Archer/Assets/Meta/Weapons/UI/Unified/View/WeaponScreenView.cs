using System;
using UnityEngine;
using UnityEngine.UI;
using Meta.Weapons.UI.Selection;

namespace Meta.Weapons.UI
{
    /// <summary>
    /// One shell for both Selection and Upgrade. Holds the 3D display, the Info Panel,
    /// and a bottom container where we plug either the Selection or Upgrade bottom bar.
    /// </summary>
    public class WeaponScreenView : MonoBehaviour
    {
        [Header("Shared Areas")]
        [SerializeField] private WeaponDisplayController displayController;
        [SerializeField] private WeaponInfoPanelView infoPanelView;

        [Header("Bottom Slot (swapped by presenter)")]
        [SerializeField] private Transform bottomContainer;

        [Header("Navigation")]
        [SerializeField] private Button backButton; // visible only in Upgrade mode

        public WeaponDisplayController Display => displayController;
        public WeaponInfoPanelView Info => infoPanelView;

        public event Action OnBack;

        public void SetBackVisible(bool visible)
        {
            if (backButton) backButton.gameObject.SetActive(visible);
        }

        public T SpawnBottomPanel<T>(T prefab) where T : Component
        {
            ClearBottomPanel();
            return Instantiate(prefab, bottomContainer);
        }

        public void ClearBottomPanel()
        {
            for (int i = bottomContainer.childCount - 1; i >= 0; i--)
                Destroy(bottomContainer.GetChild(i).gameObject);
        }

        private void Awake()
        {
            if (backButton) backButton.onClick.AddListener(() => OnBack?.Invoke());
        }
    }
}
