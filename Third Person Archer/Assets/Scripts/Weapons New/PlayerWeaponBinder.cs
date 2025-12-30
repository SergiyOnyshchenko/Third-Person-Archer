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
    [SerializeField] private MissionLaunchRequest _missionLaunchRequest;
    [SerializeField] private LoadoutSnapshot _loadoutSnapshot;
    [SerializeField] private ScriptableObject prefabProviderAsset; // IWeaponPrefabProvider (optional if your holders spawn visuals)
    [SerializeField] private ScriptableObject projectileResolverAsset; // IProjectileResolver
    [SerializeField] private ScriptableObject visualResolverAsset;     // IWeaponVisualResolver

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
            .Add(new SpearGameplayEquipper(Projectiles, Visuals))
            .Add(new ShurikenGameplayEquipper(Projectiles, Visuals))
            .Add(new BoomerangGameplayEquipper(Projectiles, Visuals));
    }

    private void Start()
    {
        if (_missionLaunchRequest != null && _missionLaunchRequest.HasRequest)
        {
            if (_loadoutSnapshot.TryGet(_missionLaunchRequest.RequiredWeaponClass, out LoadoutSnapshot.Slot slot))
            {
                EquipWeapon(slot.Weapon, slot.Stats);
            }
        }
    }

    public void EquipWeaponWithoutStats(WeaponDef def)
    {
        EquipWeapon(def, new WeaponStats());
    }

    public void EquipWeapon(WeaponDef def, WeaponStats stats)
    {
        /*
        WeaponsInitializer backend = WeaponsInitializer.Instance;

        if (actor == null) { Debug.LogError("PlayerWeaponBinder: ActorController not assigned."); return; }
        if (backend == null || backend.WeaponRepository == null || backend.EquipmentService == null || backend.StatsService == null)
        {
            Debug.LogError("PlayerWeaponBinder: Backend services not assigned.");
            return;
        }
        */

        /*
        var def = weaponCatalog.All.FirstOrDefault(d => d.Id == equippedId);
        if (def == null) { Debug.LogError($"Equipped weapon '{equippedId}' not in catalog."); return; }

        var inst = state.Weapons.FirstOrDefault(w => w.WeaponId == equippedId) ?? new WeaponInstance { WeaponId = equippedId };
        var stats = backend.StatsService.Compute(def, inst);
        */

        equipperSvc.Equip(actor, def, stats);
    }
}