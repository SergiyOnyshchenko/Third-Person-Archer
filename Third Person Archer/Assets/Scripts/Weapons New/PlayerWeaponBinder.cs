// Assets/Scripts/Runtime/Weapons/PlayerWeaponBinder.cs
using System.Linq;
using UnityEngine;
using Actor;
using Meta.Weapons;
using Game.Weapons;
using Meta.Weapons.UI.Selection;

public sealed class PlayerWeaponBinder : MonoBehaviour
{
    [Header("Actor & Attachments")]
    [SerializeField] private ActorController actor;   // your player ActorController (required)

    [Header("Catalog & Providers")]
    [SerializeField] private WeaponCatalog weaponCatalog;
    [SerializeField] private ScriptableObject prefabProviderAsset; // IWeaponPrefabProvider (optional if your holders spawn visuals)
    [SerializeField] private ScriptableObject projectileResolverAsset; // IProjectileResolver
    [SerializeField] private ScriptableObject visualResolverAsset;     // IWeaponVisualResolver

    [Header("Backend / Services")]
    [SerializeField] private WeaponsInitializer backend;

    private IWeaponPrefabProvider Prefabs => prefabProviderAsset as IWeaponPrefabProvider;
    private IProjectileResolver Projectiles => projectileResolverAsset as IProjectileResolver;
    private IWeaponVisualResolver Visuals => visualResolverAsset as IWeaponVisualResolver;

    private GameplayEquipperService equipperSvc;

    private void Awake()
    {
        // Build the equipping pipeline
        equipperSvc = new GameplayEquipperService()
            .Add(new BowGameplayEquipper(Projectiles, Visuals))
            .Add(new CrossbowGameplayEquipper(Projectiles, Visuals))
            .Add(new SpearGameplayEquipper(Projectiles, Visuals));
    }

    private void Start()
    {
        EquipEquippedWeapon();
    }

    public void EquipEquippedWeapon()
    {
        if (actor == null) { Debug.LogError("PlayerWeaponBinder: ActorController not assigned."); return; }
        if (backend == null || backend.WeaponRepository == null || backend.EquipmentService == null || backend.StatsService == null)
        {
            Debug.LogError("PlayerWeaponBinder: Backend services not assigned.");
            return;
        }

        var state = backend.WeaponRepository.Load();
        var equippedId = backend.EquipmentService.GetEquippedWeaponId(state);
        if (string.IsNullOrEmpty(equippedId))
        {
            Debug.LogWarning("PlayerWeaponBinder: No equipped weapon.");
            return;
        }

        var def = weaponCatalog.All.FirstOrDefault(d => d.Id == equippedId);
        if (def == null) { Debug.LogError($"Equipped weapon '{equippedId}' not in catalog."); return; }

        var inst = state.Weapons.FirstOrDefault(w => w.WeaponId == equippedId) ?? new WeaponInstance { WeaponId = equippedId };
        var stats = backend.StatsService.Compute(def, inst);

        // If your runtime still needs a prefab under a socket, do it here (optional):
        // var prefab = Prefabs?.GetPrefab(equippedId); ... instantiate per your rig needs ...

        // Perform in-scene wiring (projectile, reload, holders, FPV/TPV)
        equipperSvc.Equip(actor, def, stats);
    }
}