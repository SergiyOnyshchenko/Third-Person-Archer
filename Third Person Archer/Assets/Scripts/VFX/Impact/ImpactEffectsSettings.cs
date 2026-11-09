using System;
using UnityEngine;

public enum ImpactSurface
{
    Default,
    Dirt,
    Sand,
    Snow,
    Wood,
    Stone,
    Metal,
    Body,
    Head,
    /// <summary>Not a surface: the puff of air where a shot leaves the bow.</summary>
    Release
}

/// <summary>Looks and sounds of projectile impacts per surface. Loaded from Resources by <see cref="ImpactEffects"/>.</summary>
[CreateAssetMenu(fileName = "Impact Effects Settings", menuName = "Combat/Impact Effects Settings")]
public sealed class ImpactEffectsSettings : ScriptableObject
{
    public const string ResourcePath = "Impact Effects Settings";

    [Serializable]
    public sealed class Preset
    {
        public ImpactSurface Surface;

        [Header("Dust puff (along the surface normal)")]
        public Color Dust = new Color(0.55f, 0.47f, 0.38f, 0.7f);
        public int DustCount = 4;
        public float DustSize = 0.35f;

        [Header("Chips / splinters (thrown off the surface)")]
        public Color Chips = new Color(0.35f, 0.27f, 0.2f, 1f);
        public int ChipCount = 8;
        public float ChipSize = 0.05f;
        public float ChipSpeed = 3.5f;
        [Tooltip("Splinters: stretched along their flight.")]
        public bool Splinters;

        [Header("Sparks")]
        public Color Sparks = new Color(1f, 0.8f, 0.45f, 1f);
        public int SparkCount;

        [Header("Flash")]
        public Color Flash = new Color(1f, 0.95f, 0.85f, 0.6f);
        public float FlashSize = 0.35f;

        [Header("Ring")]
        public Color Ring = new Color(1f, 1f, 1f, 0f);
        public float RingSize = 1.2f;

        [Header("Sound")]
        public AudioClip[] Sounds;
        [Range(0f, 1f)] public float Volume = 0.6f;
        public Vector2 Pitch = new Vector2(0.9f, 1.1f);
    }

    [Header("Materials")]
    public Material Dust;
    public Material Chip;
    public Material Glow;
    public Material Ring;

    [Header("Surfaces")]
    public Preset[] Presets = Array.Empty<Preset>();

    [Header("Recognising surfaces (lower-case parts of collider, material or terrain layer names)")]
    public string[] Wood = { "wood", "plank", "fence", "barrel", "crate", "branch", "bark", "tree", "log", "oak", "house", "storage", "build", "roof", "door", "box", "cart", "wheel", "table", "bench" };
    public string[] Stone = { "stone", "rock", "cliff", "wall", "brick", "concrete", "boulder" };
    public string[] Metal = { "metal", "iron", "steel", "armor", "armour", "bell", "anvil" };
    public string[] Sand = { "sand", "desert", "wt_terrain" };
    public string[] Snow = { "snow", "ice", "frost" };
    public string[] Dirt = { "dirt", "mud", "grass", "ground", "soil", "terrain" };

    [Header("Budget")]
    [Tooltip("Effects ready per surface; the oldest is reused when they are all playing.")]
    public int PoolPerSurface = 3;
    [Tooltip("No effect beyond this distance from the camera (m).")]
    public float MaxDistance = 70f;

    public Preset Find(ImpactSurface surface)
    {
        foreach (var preset in Presets)
            if (preset.Surface == surface)
                return preset;

        if (surface == ImpactSurface.Release)
            return null;

        foreach (var preset in Presets)
            if (preset.Surface == ImpactSurface.Default)
                return preset;

        return null;
    }
}
