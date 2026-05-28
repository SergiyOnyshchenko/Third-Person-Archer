# Meta System Audit

> Документ описывает фактическое состояние системы в папке `Assets/Meta` по состоянию на дату аудита.
> Никаких изменений в код не вносилось. Оценки и рекомендации отсутствуют.

---

## 1. Overview

`Assets/Meta` — верхнеуровневый модуль, отвечающий за всё, что происходит **вне геймплейной сцены**:

- Прогрессию игрока (зоны, миссии, луп-система)
- Запуск миссий (выбор → проверка gate → загрузка сцены)
- Завершение миссий (награды, сохранение прогресса)
- Экономику (кошелёк, валюты, покупки, апгрейды оружия)
- Оружейную систему (каталог, экипировка, апгрейд, статы, класс-unlock)
- Энергетическую систему (тикающая энергия с реген и ad-refill)
- Балансный конфиг (сложность врагов, gate-check, формулы наград, луп-мультипликаторы)
- UI для главного меню и экрана победы

Система не использует сторонний DI-фреймворк. Сервисы создаются вручную в `MainMenuRuntime` (главное меню) и `GameplayRuntime` (геймплейная сцена). Доступ к сервисам — через эти runtime-объекты или через статические фасады (`Economy.Wallet`, `ServiceLocator`).

---

## 2. Folder Structure

```
Assets/Meta/
├── Balance/
│   ├── Config/                        # Частичные классы BalanceConfig
│   │   ├── BalanceConfig.cs           # Главный ScriptableObject (partial)
│   │   ├── BalanceConfig.LoopModule.cs
│   │   ├── BalanceConfig.EnemyModule.cs
│   │   ├── BalanceConfig.GateModule.cs
│   │   ├── BalanceConfig.RewardModule.cs
│   │   ├── BalanceConfig.WeaponClassPerformanceModule.cs
│   │   ├── BalanceConfig.EconomyPacingModule.cs
│   │   ├── BalanceConfig.Progressions.cs
│   │   └── BalanceConfig.RoundingModule.cs
│   │   └── BalanceConfig.asset        # Singleton-ассет
│   ├── EnemyArchetype.cs              # Enum типов врагов
│   └── MetaLoopProgressData.cs        # GameData: луп-прогресс (SO + persist)
│
├── Debug/
│   └── DebugMissionWindow.cs          # IMGUI-окно для ручного запуска миссий
│
├── Economy/
│   ├── CurrencyType.cs                # Enum валют
│   ├── CurrencyStartingBalanceConfig.cs
│   ├── Economy.cs                     # Статический фасад Economy.Wallet
│   ├── EconomyInstaller.cs            # MonoBehaviour: создаёт WalletService
│   ├── IWallet.cs
│   ├── UpgradePriceProfile.cs         # SO: кривая цен апгрейда
│   ├── WalletService.cs               # Реализация IWallet
│   ├── WeaponCurrencyUtility.cs       # Маппинг WeaponClass -> CurrencyType
│   ├── Storage/
│   │   ├── ICurrencyStorage.cs
│   │   ├── CurrencyStoragePlayerPrefs.cs
│   │   └── CurrencyStorageSaveSystem.cs
│   ├── UI/
│   │   ├── CostView.cs
│   │   ├── CurrencyCounterView.cs
│   │   ├── CurrencyVisualLibrary.cs
│   │   ├── PurchaseButtonPresenter.cs
│   │   └── PurchaseButtonView.cs
│   └── Data/
│       ├── CurrencyStartingBalanceConfig.asset
│       └── CurrencyVisualLibrary.asset
│
├── Energy/
│   ├── Config/
│   │   └── EnergyConfig.asset
│   └── Scripts/
│       ├── EnergyConfig.cs            # SO: все параметры энергии
│       ├── EnergyConfigEditor.cs      # Editor: custom Inspector с валидацией
│       ├── EnergyService.cs           # Логика энергии (чистый C#, не MB)
│       ├── EnergyServiceRunner.cs     # MonoBehaviour: тикает EnergyService
│       ├── EnergyState.cs             # Сериализуемое состояние энергии
│       ├── TimeProviders.cs           # ITimeProvider + реализации
│       ├── EnergyBarView.cs           # UI: полоска энергии
│       └── MissionEnergyCostView.cs   # UI: стоимость миссии в энергии
│
├── Game Data/
│   ├── GameData.cs                    # Базовый абстрактный SO
│   └── GameDataManager.cs             # MonoBehaviour DataManager (singleton)
│
├── Missions/
│   ├── Model/
│   │   ├── MissionData.cs             # SO: одна миссия (сцена, тип, класс оружия)
│   │   ├── MissionSegmentData.cs      # [Serializable]: сегмент (Campaign/Boss/Sniper)
│   │   ├── MissionProgressData.cs     # GameData SO: все зоны + selection + persist
│   │   ├── ZoneData.cs                # SO: одна зона (набор сегментов)
│   │   ├── MissionType.cs             # Enum: Campaign, Contracts, Sniper, Boss
│   │   └── MetaModeUnlockConfig.cs    # SO: уровни company для разблокировки режимов
│   ├── Services/
│   │   ├── Interfaces/                # 10 интерфейсов сервисов
│   │   ├── MissionAvailabilityService.cs
│   │   ├── MissionCatalogService.cs
│   │   ├── MissionCompletionService.cs
│   │   ├── MissionContextService.cs
│   │   ├── MissionGateService.cs
│   │   ├── MissionProgressAdapter.cs
│   │   ├── MissionProgressReadAdapter.cs
│   │   ├── MissionProgressWriteAdapter.cs
│   │   ├── MissionRewardService.cs
│   │   ├── LoopProgressService.cs
│   │   ├── CompanyLevelService.cs
│   │   └── WeaponRequirementService.cs
│   ├── Controllers/
│   │   ├── MainMenuRuntime.cs         # MonoBehaviour: точка сборки всех сервисов меню
│   │   ├── MainMenuServices.cs        # Контейнер сервисов (передаётся в UI)
│   │   ├── GameplayRuntime.cs         # MonoBehaviour: точка сборки для геймплея
│   │   ├── MissionSelectorButtonPresenter.cs
│   │   ├── MissionTypeSelectionPresenter.cs
│   │   ├── PlayMissionPresenter.cs
│   │   ├── ZoneMapPresenter.cs
│   │   └── ZoneSelectionInitializer.cs
│   ├── Launch/
│   │   ├── MissionLaunchRequest.cs    # SO: cross-scene payload (меню -> геймплей)
│   │   ├── MissionSceneBootstrapper.cs
│   │   ├── MissionStartService.cs
│   │   └── ContractPoolService.cs
│   ├── Gate/
│   │   ├── DamageGatePopupArgs.cs
│   │   └── DamageGatePopupController.cs
│   ├── View/
│   │   ├── BossProgressView.cs
│   │   ├── PlayButtonView.cs
│   │   ├── ZoneMapView.cs
│   │   ├── ZoneSelectionView.cs
│   │   └── Victory/
│   │       ├── VictoryScreenSubState.cs   # SubState: показ экрана победы
│   │       ├── VictoryContext.cs
│   │       ├── VictoryContextFactory.cs
│   │       ├── IVictorySegment.cs
│   │       ├── VictoryHeaderSegment.cs
│   │       ├── VictoryStatsSegment.cs
│   │       ├── VictoryRewardsSegment.cs
│   │       ├── VictoryMultiplierSegment.cs
│   │       ├── VictoryTokenRowView.cs
│   │       ├── CurrencyAmountRowView.cs
│   │       └── PriceView.cs
│   ├── MissionAvailability.cs
│   ├── MissionCompleteResult.cs
│   ├── MissionContext.cs
│   ├── MissionGateResult.cs
│   ├── MissionOutcome.cs
│   ├── MissionReward.cs
│   └── CampaignLevelFormatUtility.cs
│
├── Weapons/
│   ├── Catalog/
│   │   ├── WeaponCatalog.cs           # SO: все WeaponDef, кеш по Id и классу
│   │   ├── WeaponCatalogEditor.cs     # Editor: MenuItem для создания ассета
│   │   ├── WeaponDef.cs               # SO: одно оружие (статы, цена, upgrade)
│   │   ├── WeaponEnums.cs             # WeaponClass enum
│   │   ├── WeaponStats.cs             # Struct: Damage, Balance, Distance, ...
│   │   └── WeaponsState.cs            # [Serializable]: список WeaponInstance
│   ├── Defs/                          # Фактически WeaponDef.cs дублируется/перемещён
│   ├── Loadout/
│   │   ├── LoadoutSnapshot.cs         # SO: текущий equipped-слот каждого класса
│   │   ├── LoadoutSnapshotEditor.cs   # Editor: кнопка Refresh в Inspector
│   │   ├── LoadoutSnapshotUpdater.cs  # Обновляет снэпшот при смене экипировки
│   │   └── LoadoutSnapshotWeaponStatService.cs  # ILoadoutWeaponStatService
│   ├── Repositories/
│   │   ├── IWeaponRepository.cs
│   │   └── WeaponRepository.cs        # Обёртка SaveSystem для WeaponsState.json
│   ├── Services/
│   │   ├── Stats/
│   │   │   ├── IStatsService.cs
│   │   │   └── StatsService.cs        # Lerp BaseStats -> MaxStats по upgrade curve
│   │   ├── Equipment/
│   │   │   ├── IEquipmentService.cs
│   │   │   └── EquipmentService.cs    # Equip/GetEquipped, persist через repo
│   │   ├── Upgrades/
│   │   │   ├── IUpgradeService.cs
│   │   │   └── UpgradeService.cs      # CanUpgrade, TryUpgrade (wallet check)
│   │   ├── Selection/
│   │   │   ├── WeaponSelectionService.cs
│   │   │   └── MissionPreparationService.cs
│   │   └── Abstractions/
│   │       └── MoneyAndTime.cs
│   ├── Bootstrap/
│   │   ├── WeaponsInitializer.cs      # MonoBehaviour: создаёт все weapon-сервисы
│   │   ├── DefaultLoadoutConfig.cs    # SO: стартовая экипировка по умолчанию
│   │   └── DefaultLoadoutApplier.cs   # Применяет default loadout, если нет сохранения
│   ├── Class Unlock/
│   │   ├── IWeaponClassUnlockService.cs
│   │   ├── WeaponClassUnlockService.cs
│   │   └── Popups/
│   │       ├── WeaponClassUnlockedPopupArgs.cs
│   │       ├── WeaponClassUnlockedPopupScreen.cs
│   │       ├── WeaponUnlockPopupsController.cs
│   │       ├── WeaponUnlockPopupsProgress.cs
│   │       ├── WeaponUnlockedPopupArgs.cs
│   │       ├── WeaponUnlockedPopupScreen.cs
│   │       └── IWeaponUnlockPopupFlow.cs
│   ├── Configs/
│   │   └── WeaponScoringConfig.cs
│   ├── Missions/
│   │   └── MissionRequirement.cs
│   └── UI/
│       ├── Loadout/
│       │   ├── LoadoutScreen.cs
│       │   └── LoadoutSlotView.cs
│       ├── Selection/
│       │   ├── WeaponSelectionScreen.cs  # Полный экран выбора/покупки/апгрейда
│       │   ├── WeaponSelectionArgs.cs
│       │   ├── WeaponListItemView.cs
│       │   ├── WeaponPricePanelView.cs
│       │   ├── WeaponStatView.cs
│       │   ├── WeaponStatsPanelView.cs
│       │   ├── WeaponStatsRangeConfig.cs
│       │   └── WeaponUpgradeLevelView.cs
│       └── Utilities/
│           ├── IWeaponIconProvider.cs
│           ├── IWeaponPrefabProvider.cs
│           ├── WeaponClassIconLibrary.cs
│           ├── WeaponIconLibrary.cs
│           └── WeaponPrefabMap.cs
│
├── IGlobalCampaignIndexProvider.cs
├── IWeaponLoadoutProvider.cs
└── IZoneIndexProvider.cs
```

---

## 3. Core Data / ScriptableObjects

| Ассет / класс | Тип | Назначение |
|---|---|---|
| `BalanceConfig` | `ScriptableObject` (partial) | Все формулы баланса: сложность врагов, gate-check, награды, луп-мультипликаторы. Разбит на 8 inner-классов (modules). |
| `MetaLoopProgressData` | `GameData` (SO) | Луп-индекс, счётчики Contracts/Sniper, persist через `SaveSystem`. |
| `MissionProgressData` | `GameData` (SO) | Все зоны (`List<ZoneData>`), текущий выбор зоны и типа миссии, last unlocked zone. Сохраняется через `SaveSystem`. |
| `ZoneData` | `ScriptableObject` | Одна зона: набор `MissionSegmentData` (Campaign, Boss, Sniper). Содержит методы проверки прогресса (IsBossCompleted, IsCampaignComplete). |
| `MissionData` | `ScriptableObject` | Одна миссия: id, тип, сцена, weapon class (base/fixed/rotation), reward override. |
| `MissionLaunchRequest` | `ScriptableObject` | Cross-scene payload: заполняется в меню (`MissionStartService`), читается в геймплее (`MissionSceneBootstrapper`). |
| `LoadoutSnapshot` | `ScriptableObject` | 5 слотов (один на `WeaponClass`): equipped weapon, upgrade level, damage, computed stats. Служит мостом между меню и геймплеем для gate-check. |
| `MetaModeUnlockConfig` | `ScriptableObject` | Company level для разблокировки Contracts и Sniper. |
| `WeaponCatalog` | `ScriptableObject` | Список всех `WeaponDef`. Runtime-кеш по Id и по классу. |
| `WeaponDef` | `ScriptableObject` | Одно оружие: id, иконка, класс, BaseStats/MaxStats, кривая апгрейда, цена покупки и апгрейда, `UpgradePriceProfile`. |
| `UpgradePriceProfile` | `ScriptableObject` | Кривая стоимости апгрейда (cash + tokens) для каждого уровня. |
| `CurrencyStartingBalanceConfig` | `ScriptableObject` | Стартовые балансы и кепы каждой валюты. |
| `EnergyConfig` | `ScriptableObject` | Все параметры энергии: max, regen interval, стоимости миссий по типу, ad refill, daily reset. |
| `DefaultLoadoutConfig` | `ScriptableObject` | Экипировка по умолчанию (weapon id на класс), применяется один раз при первом запуске. |

### Хранимые данные (через `MissionSegmentData` внутри ZoneData)

Каждый сегмент (`MissionSegmentData`) хранит:
- `_currentIndex` — текущая активная миссия в сегменте
- `_totalCompletedCount` — суммарно завершено
- `_completedOnce[]` — флаг "пройдено хотя бы раз" на каждую миссию
- Всё это сохраняется в `SaveSystem` с ключом `{zoneId}_{segmentType}_current/total/once_N`

---

## 4. Runtime Services and Logic

### 4.1 Сборка сервисов: MainMenuRuntime

`MainMenuRuntime` (MonoBehaviour, сцена главного меню) — центральная точка сборки. В `Awake` создаёт все сервисы вручную и публикует их через `MainMenuServices`:

| Сервис | Класс | Роль |
|---|---|---|
| `IMetaProgressReadOnly` | `MissionProgressReadAdapter` | Чтение прогресса из `MissionProgressData` |
| `IMetaProgressWrite` | `MissionProgressWriteAdapter` | Запись прогресса |
| `ILoopProgress` | `LoopProgressService` | Текущий луп-индекс, balance loop index |
| `IMissionCatalogService` | `MissionCatalogService` | Глобальный индекс миссии, поиск по зонам |
| `IWeaponRequirementService` | `WeaponRequirementService` | Вычисляет требуемый `WeaponClass` с учётом луп-ротации |
| `IMissionContextService` | `MissionContextService` | Строит `MissionContext` для текущего выбора |
| `ILoadoutWeaponStatService` | `LoadoutSnapshotWeaponStatService` | Читает урон из `LoadoutSnapshot` для gate |
| `IMissionGateService` | `MissionGateService` | Проверяет damage gate для Campaign |
| `ICompanyLevelService` | `CompanyLevelService` | Company level = globalCampaignCount + 1 |
| `IMissionAvailabilityService` | `MissionAvailabilityService` | Проверяет все условия доступности миссии |
| `IContractPoolService` | `ContractPoolService` | Случайный pick из завершённых campaign-миссий |
| `IMissionStartService` | `MissionStartService` | Запуск выбранной миссии (проверки + заполнение LaunchRequest) |

### 4.2 Сборка сервисов: GameplayRuntime

`GameplayRuntime` (MonoBehaviour, геймплейная сцена) создаёт подмножество сервисов:

- `MissionRewardService` — расчёт наград
- `MissionCompletionService` — применение прогресса + вызов reward
- `LoopProgressService` — для доступа к луп-индексу

Контекст строится из `MissionLaunchRequest` (если заполнен) или пересчитывается из `MissionProgressData`.

### 4.3 Weapon Services (WeaponsInitializer)

`WeaponsInitializer` (MonoBehaviour) создаёт:

| Сервис | Класс | Роль |
|---|---|---|
| `IWeaponRepository` | `WeaponRepository` | Load/Save `WeaponsState.json` |
| `IStatsService` | `StatsService` | Lerp BaseStats → MaxStats по AnimationCurve |
| `IEquipmentService` | `EquipmentService` | Equip/GetEquipped per class |
| `IUpgradeService` | `UpgradeService` | CanUpgrade, TryUpgradeWithCurrencies, TryUpgradeWithAd |
| `IWeaponClassUnlockService` | `WeaponClassUnlockService` | Unlock Spear/Boomerang по zone index |
| `LoadoutSnapshotUpdater` | — | Реактивно обновляет `LoadoutSnapshot` при смене экипировки/апгрейде |

`WeaponsInitializer` регистрирует `IWeaponClassUnlockService` в `ServiceLocator`.

### 4.4 Economy

`EconomyInstaller` (MonoBehaviour, DontDestroyOnLoad) создаёт `WalletService` и публикует его через `Economy.Wallet` (статический фасад).

Хранилище выбирается компиляционным символом:
- По умолчанию: `CurrencyStoragePlayerPrefs`
- При `#define ECONOMY_USE_SAVESYSTEM` и флаге `useSaveSystem`: `CurrencyStorageSaveSystem`

### 4.5 Energy

`EnergyService` (чистый C#, не MonoBehaviour) — вся логика энергии.
`EnergyServiceRunner` (MonoBehaviour) — тикает `EnergyService` в `Update`, вызывает `RecalculateFromNow()` при возврате приложения на передний план.

Состояние сохраняется через `SaveSystem.Save("EnergyState", _state)`.

---

## 5. UI / Menu Integration

Все UI-компоненты в главном меню подписываются на `MainMenuRuntime.Ready` (если runtime ещё не готов) или напрямую инициализируются из `MainMenuRuntime.Instance.Services`.

При любом изменении выбора (зона, тип миссии) `MainMenuRuntime` вызывает `Services.NotifyMenuStateChanged()`, который рассылает событие `OnMenuStateChanged` всем подписанным презентерам.

| Компонент | Роль |
|---|---|
| `PlayMissionPresenter` | Кнопка Play: проверяет доступность, запускает `TryStartSelected()`, обрабатывает gate-popup |
| `MissionTypeSelectionPresenter` | Переключение Campaign/Contracts/Sniper/Boss |
| `MissionSelectorButtonPresenter` | Кнопка выбора конкретной миссии внутри типа |
| `ZoneMapPresenter` | Карта зон, навигация |
| `ZoneSelectionInitializer` | Устанавливает начальную зону при старте |
| `BossProgressView` | Слайдер прогресса до boss unlock |
| `DamageGatePopupController` | Popup при недостаточном уроне (IReceivesArgs) |
| `WeaponSelectionScreen` | Полный экран выбора/покупки/апгрейда оружия по классу |
| `LoadoutScreen` / `LoadoutSlotView` | Экран экипировки (слоты по классам) |
| `VictoryScreenSubState` | SubState экрана победы после миссии |

`WeaponSelectionScreen` реализует `IReceivesArgs<WeaponSelectionArgs>` (new UI-паттерн) и получает `WeaponClass` и `CampaignLevel` как аргументы при открытии через `IUINavigator`.

Victory screen открывается как `SubState` (кастомная state machine) и проходит через сегменты: Header → Stats → Rewards → Multiplier. Payout (`Economy.Wallet.Add`) применяется при нажатии Continue.

---

## 6. Gameplay Scene Integration

После запуска сцены:

1. `MissionSceneBootstrapper.Awake()` читает `MissionLaunchRequest`:
   - Применяет `RequiredWeaponClass` через `IGameplayLoadoutApplier`
   - Заполняет `RuntimeMissionSession` (статический класс с данными сессии)
2. `GameplayRuntime.Awake()` создаёт сервисы и строит `MissionContext` из `MissionLaunchRequest`.
3. Геймплейные системы (например, `EnemyStatsApplier`) могут обращаться к `GameplayRuntime.Instance` за:
   - `Context` — тип миссии, зона, луп, класс оружия
   - `Balance` — `BalanceConfig` для вычисления HP/damage врагов
   - `ContractsCompletedIndex`, `SniperCompletedIndex` — грайнд-счётчики
4. По завершении миссии вызывается `GameplayRuntime.Instance.CompleteMission(outcome)`:
   - `MissionCompletionService.Complete()` → `MissionRewardService.Calculate()` → `ApplyProgress()`
5. `VictoryScreenSubState` (геймплейная сцена) читает результат и применяет payout в кошелёк.

---

## 7. Save / Progress Flow

### Используемая система сохранения

Все сохранения идут через `SaveSystem` (статический класс, предположительно за пределами `Assets/Meta`). Методы: `SaveSystem.Save(key, value)` / `SaveSystem.Load(key, default)`.

### Что и где сохраняется

| Данные | Ключ(и) сохранения | Кто сохраняет |
|---|---|---|
| Текущая выбранная зона | `selected_zone_index` | `MissionProgressData` |
| Тип выбранной миссии | `selected_mission_type` | `MissionProgressData` |
| Last unlocked zone | `last_unlocked_zone_index` | `MissionProgressData` |
| Прогресс сегмента (current/total/once) | `{zoneId}_{type}_current`, `_total`, `_once_N` | `MissionSegmentData` |
| Луп-индекс, loops completed | `meta_loop_current_loop`, `meta_loop_completed_loops` | `MetaLoopProgressData` |
| Contracts/Sniper count | `meta_loop_contracts_completed`, `meta_loop_sniper_completed` | `MetaLoopProgressData` |
| Оружейный инвентарь | `WeaponsState.json` | `WeaponRepository` |
| Валюты | PlayerPrefs (или SaveSystem) per CurrencyType | `WalletService` через storage |
| Энергия | `EnergyState` | `EnergyService` |

### Разблокировка зон

Зона `i` открыта, если зона `i-1` завершена:
- Если в зоне есть Boss: boss должен быть пройден
- Иначе: все Campaign-миссии пройдены хотя бы раз

При завершении последнего Boss последней зоны: `MetaLoopProgressData.OnFullCampaignAndAllBossesCompleted()` → `loopIndex++`, сброс на зону 0.

---

## 8. Editor Tools, Generators, Utilities

| Файл | Тип | Назначение |
|---|---|---|
| `WeaponCatalogEditor.cs` | `MenuItem` | Создаёт ассет `WeaponCatalog` через диалог |
| `WeaponCatalog` → `ContextMenu("Validate Duplicates")` | ContextMenu | Проверяет уникальность Id всех WeaponDef в каталоге |
| `EnergyConfigEditor.cs` | Custom Editor | Показывает warnings из `EnergyConfig.GetValidationErrors()` прямо в Inspector |
| `LoadoutSnapshotEditor.cs` | Custom Editor | Кнопка "Refresh" в Inspector для ручного обновления `LoadoutSnapshot` |
| `DebugMissionWindow.cs` | IMGUI MonoBehaviour | Полноэкранное отладочное окно (runtime): запуск любой миссии напрямую, просмотр и изменение валют. Управляется через `_show` флаг. |
| `MissionData.OnValidate()` | `OnValidate` | Автозаполнение id (GUID) и displayName при создании ассета; Sniper-миссии форсируют `useFixedWeaponClass = true`. |
| `WeaponCatalog.OnValidate()` | `OnValidate` | Перестраивает runtime-кеши при изменении в Editor. |

---

## 9. Deprecated or Unused-Looking Parts

### Закомментированный метод

В `MissionAvailabilityService.cs` присутствует полная закомментированная версия `GetAvailability()` (строки 17–54). Она идентична активной версии по логике, но была заменена версией с раздельной обработкой Contracts/Sniper (без требования `ctx.IsValid`). Блок не удалён.

### `IMissionStartService` отсутствует как интерфейс в отдельном файле

`IMissionStartService` определён прямо внутри `MissionStartService.cs` (не в папке `Interfaces/`), в отличие от остальных интерфейсов.

### `WeaponSelectionService.cs` и `MissionPreparationService.cs`

Файлы существуют, но явных вызовов из других компонентов Meta-системы в просмотренных файлах не обнаружено. Возможно, используются в других частях проекта.

### `MoneyAndTime.cs`

Содержит абстракцию `Abstractions/MoneyAndTime.cs` в папке Weapons/Services. Конкретное использование в просмотренных файлах не выявлено.

### `WeaponScoringConfig.cs`

Файл `Configs/WeaponScoringConfig.cs` существует. Использование внутри Meta-системы в просмотренных файлах не обнаружено.

### `MissionRequirement.cs`

`Weapons/Missions/MissionRequirement.cs` — использование не выявлено в просмотренных файлах Meta.

### `IGlobalCampaignIndexProvider.cs`, `IWeaponLoadoutProvider.cs`, `IZoneIndexProvider.cs`

Три интерфейса в корне `Assets/Meta/`. Реализации в папке `Meta/` не обнаружены. Возможно, реализации находятся за пределами папки `Meta`.

---

## 10. Full Flow Example: From Selecting a Mission to Completing It

### Шаг 1 — Старт главного меню

- `DataManager.Awake()` инициализирует все `GameData` SO (`MissionProgressData`, `MetaLoopProgressData`), вызывая `Initialize()` → `LoadPersistentData()`.
- `EconomyInstaller.Awake()` создаёт `WalletService`, публикует в `Economy.Wallet`.
- `WeaponsInitializer.Awake()` создаёт все weapon-сервисы, применяет default loadout если нужно, обновляет `LoadoutSnapshot`.
- `MainMenuRuntime.Awake()` читает `MissionProgressData` из `DataManager`, форсирует зону на last unlocked, собирает все mission-сервисы в `MainMenuServices`, испускает `Ready`.

### Шаг 2 — Игрок выбирает зону и тип миссии

- `ZoneMapPresenter` / `MissionTypeSelectionPresenter` вызывают `ProgressData.SelectZone()` / `SelectMissionType()`.
- `MissionProgressData` сохраняет выбор через `SaveSystem` и испускает `OnZoneChanged` / `OnMissionTypeChanged`.
- `MainMenuRuntime` ловит события, вызывает `Services.NotifyMenuStateChanged()`.
- Все презентеры обновляют состояние кнопок/UI.

### Шаг 3 — Игрок нажимает Play

- `PlayMissionPresenter.OnPlayClicked()` вызывает `MissionStartService.TryStartSelected()`.
- `MissionContextService.BuildSelectedContext()` собирает `MissionContext` (зона, тип, миссия, луп, weapon class).
- `MissionAvailabilityService.GetAvailability(ctx)` проверяет:
  - Campaign: `MissionGateService.CheckCampaignGate()` сравнивает damage из `LoadoutSnapshot` с требованием из `BalanceConfig`.
  - Boss: `ZoneData.IsBossUnlocked()`.
  - Contracts/Sniper: company level >= threshold из `MetaModeUnlockConfig`.
- Если gate не пройден → `DamageGatePopupController` через `IUINavigator.ShowPopup()`.
- Если всё ок → `MissionLaunchRequest.Set(...)` заполняется параметрами миссии.
- `ScenesLoader.LoadScene(missionData.Scene.ScenePath)` загружает геймплейную сцену.

### Шаг 4 — Геймплейная сцена

- `MissionSceneBootstrapper.Awake()` читает `MissionLaunchRequest`, вызывает `IGameplayLoadoutApplier.ApplyWeaponClass()`, заполняет `RuntimeMissionSession`.
- `GameplayRuntime.Awake()` создаёт `MissionRewardService` + `MissionCompletionService`, строит `MissionContext` из `LaunchRequest`.
- Геймплейные системы получают стат врагов через `GameplayRuntime.Instance.Balance.GetEnemyStats(ctx, archetype, ...)`.

### Шаг 5 — Завершение миссии

- Геймплейная логика вызывает `GameplayRuntime.Instance.CompleteMission(MissionOutcome.Completed)`.
- `MissionCompletionService.Complete()`:
  - `MissionRewardService.Calculate(ctx)` → вычисляет cash + tokens по формулам `BalanceConfig` (с луп-мультипликаторами).
  - `ApplyProgress(ctx)` → для Campaign: `ZoneData.AdvanceMission()` → `MissionSegmentData.Advance()` → сохранение. Для Boss: дополнительно `AdvanceToNextZoneOrLoop()`.
- `VictoryScreenSubState.Enter()` читает `GameplayRuntime.Instance.Context` и `Rewards`, строит `VictoryContext` и `FinalRewardBundle`.
- Последовательно показываются сегменты: Header → Stats → Rewards → Multiplier.
- При нажатии Continue: `ApplyPayout()` добавляет cash и tokens в `Economy.Wallet` → `WalletService.Add()` → сохранение через currency storage.
- `OnVictoryScreenFinished` → переход в следующее состояние (предположительно возврат в меню).

---

## Список просмотренных файлов

```
Assets/Meta/Balance/Config/BalanceConfig.cs
Assets/Meta/Balance/Config/BalanceConfig.LoopModule.cs
Assets/Meta/Balance/Config/BalanceConfig.EnemyModule.cs  (не читался, упомянут структурно)
Assets/Meta/Balance/Config/BalanceConfig.GateModule.cs   (не читался, упомянут структурно)
Assets/Meta/Balance/Config/BalanceConfig.RewardModule.cs (не читался, упомянут структурно)
Assets/Meta/Balance/MetaLoopProgressData.cs
Assets/Meta/Balance/EnemyArchetype.cs                   (не читался, упомянут структурно)
Assets/Meta/Debug/DebugMissionWindow.cs
Assets/Meta/Economy/CurrencyType.cs
Assets/Meta/Economy/Economy.cs
Assets/Meta/Economy/EconomyInstaller.cs
Assets/Meta/Economy/IWallet.cs                          (не читался, упомянут структурно)
Assets/Meta/Economy/WalletService.cs
Assets/Meta/Economy/WeaponCurrencyUtility.cs            (не читался, упомянут структурно)
Assets/Meta/Economy/UpgradePriceProfile.cs              (не читался, упомянут структурно)
Assets/Meta/Economy/Storage/ICurrencyStorage.cs         (не читался, упомянут структурно)
Assets/Meta/Economy/Storage/CurrencyStoragePlayerPrefs.cs (не читался)
Assets/Meta/Economy/Storage/CurrencyStorageSaveSystem.cs  (не читался)
Assets/Meta/Energy/Scripts/EnergyConfig.cs
Assets/Meta/Energy/Scripts/EnergyService.cs
Assets/Meta/Energy/Scripts/EnergyConfigEditor.cs        (не читался, упомянут структурно)
Assets/Meta/Energy/Scripts/EnergyServiceRunner.cs       (не читался, упомянут структурно)
Assets/Meta/Energy/Scripts/EnergyState.cs               (не читался, упомянут структурно)
Assets/Meta/Energy/Scripts/TimeProviders.cs             (не читался, упомянут структурно)
Assets/Meta/Game Data/GameData.cs
Assets/Meta/Game Data/GameDataManager.cs
Assets/Meta/Missions/Model/MissionData.cs
Assets/Meta/Missions/Model/MissionSegmentData.cs
Assets/Meta/Missions/Model/MissionProgressData.cs
Assets/Meta/Missions/Model/ZoneData.cs
Assets/Meta/Missions/Model/MissionType.cs               (не читался, упомянут структурно)
Assets/Meta/Missions/Model/MetaModeUnlockConfig.cs      (не читался, упомянут структурно)
Assets/Meta/Missions/MissionContext.cs
Assets/Meta/Missions/MissionAvailability.cs             (не читался, упомянут структурно)
Assets/Meta/Missions/MissionCompleteResult.cs           (не читался, упомянут структурно)
Assets/Meta/Missions/MissionGateResult.cs               (не читался, упомянут структурно)
Assets/Meta/Missions/MissionOutcome.cs                  (не читался, упомянут структурно)
Assets/Meta/Missions/MissionReward.cs                   (не читался, упомянут структурно)
Assets/Meta/Missions/CampaignLevelFormatUtility.cs      (не читался, упомянут структурно)
Assets/Meta/Missions/Controllers/MainMenuRuntime.cs
Assets/Meta/Missions/Controllers/MainMenuServices.cs
Assets/Meta/Missions/Controllers/GameplayRuntime.cs
Assets/Meta/Missions/Controllers/PlayMissionPresenter.cs
Assets/Meta/Missions/Controllers/MissionSelectorButtonPresenter.cs (не читался)
Assets/Meta/Missions/Controllers/MissionTypeSelectionPresenter.cs  (не читался)
Assets/Meta/Missions/Controllers/ZoneMapPresenter.cs               (не читался)
Assets/Meta/Missions/Controllers/ZoneSelectionInitializer.cs       (не читался)
Assets/Meta/Missions/Launch/MissionLaunchRequest.cs
Assets/Meta/Missions/Launch/MissionStartService.cs
Assets/Meta/Missions/Launch/MissionSceneBootstrapper.cs
Assets/Meta/Missions/Launch/ContractPoolService.cs      (не читался, упомянут структурно)
Assets/Meta/Missions/Gate/DamageGatePopupController.cs
Assets/Meta/Missions/Gate/DamageGatePopupArgs.cs        (не читался, упомянут структурно)
Assets/Meta/Missions/Services/MissionAvailabilityService.cs
Assets/Meta/Missions/Services/MissionCatalogService.cs  (не читался, упомянут структурно)
Assets/Meta/Missions/Services/MissionCompletionService.cs
Assets/Meta/Missions/Services/MissionContextService.cs
Assets/Meta/Missions/Services/MissionGateService.cs
Assets/Meta/Missions/Services/MissionRewardService.cs
Assets/Meta/Missions/Services/MissionProgressReadAdapter.cs  (не читался)
Assets/Meta/Missions/Services/MissionProgressWriteAdapter.cs (не читался)
Assets/Meta/Missions/Services/MissionProgressAdapter.cs      (не читался)
Assets/Meta/Missions/Services/LoopProgressService.cs    (не читался, упомянут структурно)
Assets/Meta/Missions/Services/CompanyLevelService.cs    (не читался, упомянут структурно)
Assets/Meta/Missions/Services/WeaponRequirementService.cs (не читался, упомянут структурно)
Assets/Meta/Missions/View/Victory/VictoryScreenSubState.cs
Assets/Meta/Missions/View/Victory/VictoryContext.cs     (не читался, упомянут структурно)
Assets/Meta/Missions/View/Victory/VictoryContextFactory.cs (не читался, упомянут структурно)
Assets/Meta/Missions/View/BossProgressView.cs           (не читался, упомянут структурно)
Assets/Meta/Missions/View/PlayButtonView.cs             (не читался, упомянут структурно)
Assets/Meta/Weapons/Catalog/WeaponCatalog.cs
Assets/Meta/Weapons/Catalog/WeaponCatalogEditor.cs
Assets/Meta/Weapons/Catalog/WeaponEnums.cs              (не читался, упомянут структурно)
Assets/Meta/Weapons/Catalog/WeaponStats.cs              (не читался, упомянут структурно)
Assets/Meta/Weapons/Catalog/WeaponsState.cs             (не читался, упомянут структурно)
Assets/Meta/Weapons/Defs/WeaponDef.cs
Assets/Meta/Weapons/Loadout/LoadoutSnapshot.cs
Assets/Meta/Weapons/Loadout/LoadoutSnapshotEditor.cs    (не читался, упомянут структурно)
Assets/Meta/Weapons/Loadout/LoadoutSnapshotUpdater.cs   (не читался, упомянут структурно)
Assets/Meta/Weapons/Loadout/LoadoutSnapshotWeaponStatService.cs (не читался, упомянут структурно)
Assets/Meta/Weapons/Repositories/WeaponRepository.cs
Assets/Meta/Weapons/Services/Stats/StatsService.cs
Assets/Meta/Weapons/Services/Equipment/EquipmentService.cs (не читался, упомянут структурно)
Assets/Meta/Weapons/Services/Upgrades/UpgradeService.cs
Assets/Meta/Weapons/Bootstrap/WeaponsInitializer.cs
Assets/Meta/Weapons/Bootstrap/DefaultLoadoutConfig.cs   (не читался, упомянут структурно)
Assets/Meta/Weapons/Bootstrap/DefaultLoadoutApplier.cs  (не читался, упомянут структурно)
Assets/Meta/Weapons/Class Unlock/WeaponClassUnlockService.cs
Assets/Meta/Weapons/UI/Selection/WeaponSelectionScreen.cs
Assets/Meta/Weapons/Configs/WeaponScoringConfig.cs      (не читался)
Assets/Meta/Weapons/Missions/MissionRequirement.cs      (не читался)
Assets/Meta/IGlobalCampaignIndexProvider.cs             (не читался)
Assets/Meta/IWeaponLoadoutProvider.cs                   (не читался)
Assets/Meta/IZoneIndexProvider.cs                       (не читался)
```
