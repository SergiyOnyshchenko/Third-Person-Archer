using UnityEngine;

namespace Meta.Weapons.UI.Selection
{
    /// <summary>
    /// Returns a square sprite used for weapon tiles/carousel.
    /// Implementations may fetch from a ScriptableObject map, a SpriteAtlas, Addressables, etc.
    /// </summary>
    public interface IWeaponIconProvider
    {
        Sprite GetIcon(string weaponId);
    }
}