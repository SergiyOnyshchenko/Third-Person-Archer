# Player Motivation & Meaning — Design Ideas

**Дата:** 2026-05-28
**Статус:** Идеи (не реализовано)
**Базовый документ:** `Meta_GameDesign_Audit_RU.md`
**Цель:** Добавить мотивацию, смысл и ясность через UI, popup'ы, tutorial'ы, reward beats, подсказки — **без крупных redesign'ов геймплея или баланса.**

---

## Проблемы из аудита (для справки)

| # | Проблема | Severity |
|---|----------|----------|
| 1 | Contracts — мотивационно пустой grind | Major |
| 2 | Sniper → Crossbow → Boss связка не объяснена | Major |
| 3 | Spear и Boomerang появляются без onboarding | Major |
| 4 | Loop transition — silent reset | Major |
| 5 | Gate popup не говорит, ГДЕ взять ресурсы | Critical |
| 6 | Нет mission type tooltips (зачем Contracts? зачем Sniper?) | Major |
| 7 | Cash rewards meaningless к mid-game | Minor |
| 8 | Zone 2 gate wall без reward beats | Critical |
| 9 | Zone 3 no Boss = anticlimax | Minor |
| 10 | Weapon class rotation в Campaign не объяснена | Minor |

---

## 1. First-Time Popups

Popup'ы, которые появляются **один раз** при первом событии. Каждый popup сохраняет `shown` flag через PlayerPrefs / аналог. Игрок видит popup, нажимает кнопку, popup больше не появляется.

---

### 1.1 «Новый тип миссии: Contracts»

**Триггер:** Первый раз, когда Contracts tab становится доступным (по company level / MetaModeUnlockConfig).

**Экран:**

```
┌──────────────────────────────────────┐
│                                      │
│      [Contracts Icon]                │
│                                      │
│   КОНТРАКТЫ РАЗБЛОКИРОВАНЫ           │
│                                      │
│   Контракты — повторные миссии       │
│   за награды.                        │
│                                      │
│   Каждый контракт приносит           │
│   монеты и жетоны ВСЕХ видов         │
│   оружия.                            │
│                                      │
│   Выполняй контракты, чтобы          │
│   прокачивать своё снаряжение.       │
│                                      │
│          [ПОНЯТНО]                   │
│                                      │
└──────────────────────────────────────┘
```

**Кнопка:** «Понятно» → закрывает popup, переключает tab на Contracts.

**Что решает:** Проблема #1 (Contracts motivation) + #6 (mission type tooltips). Игрок сразу знает: Contracts = tokens всех классов.

**Реализация:** Простой — один popup prefab + `FirstTimePopupService` с PlayerPrefs key.

---

### 1.2 «Новый тип миссии: Sniper»

**Триггер:** Первый раз, когда Sniper tab доступен.

**Экран:**

```
┌──────────────────────────────────────┐
│                                      │
│      [Sniper/Crossbow Icon]          │
│                                      │
│   СНАЙПЕР РАЗБЛОКИРОВАН             │
│                                      │
│   Снайперские миссии используют      │
│   арбалет. Каждый враг —             │
│   один выстрел.                      │
│                                      │
│   Награды: монеты +                  │
│   жетоны арбалета.                   │
│                                      │
│   Прокачивай арбалет здесь,          │
│   чтобы открыть Босса!              │
│                                      │
│          [ПОНЯТНО]                   │
│                                      │
└──────────────────────────────────────┘
```

**Кнопка:** «Понятно» → закрывает popup, переключает tab на Sniper.

**Что решает:** Проблема #2 (Sniper → Crossbow → Boss) + #6 (mission type tooltips). Подсказка «прокачивай арбалет → откроешь Босса» — ключевая мотивационная связка.

**Реализация:** Аналогично 1.1.

---

### 1.3 «Новый тип миссии: Boss»

**Триггер:** Первый раз, когда Boss tab появляется (после completion всех Campaign missions в зоне).

**Экран:**

```
┌──────────────────────────────────────┐
│                                      │
│      [Boss Icon / Zone Boss Art]     │
│                                      │
│   БОСС ЗОНЫ ДОСТУПЕН                │
│                                      │
│   Босс — сильнейший враг зоны.       │
│   Победи его, чтобы перейти          │
│   дальше.                            │
│                                      │
│   Для битвы с боссом нужен           │
│   достаточно сильный арбалет.        │
│                                      │
│   Награда: большой бонус монет       │
│   и жетонов всех видов.             │
│                                      │
│          [К БОССУ]                   │
│                                      │
└──────────────────────────────────────┘
```

**Кнопка:** «К Боссу» → переключает tab на Boss.

**Что решает:** Проблема #2 (Sniper → Boss link). Игрок знает: Boss = нужен арбалет, Boss = big reward.

**Реализация:** Аналогично 1.1.

---

### 1.4 «Новый класс оружия: Spear»

**Триггер:** Первый вход в Zone 2 Campaign (g7).

**Экран:**

```
┌──────────────────────────────────────┐
│                                      │
│      [Spear Weapon Art]              │
│                                      │
│   НОВЫЙ КЛАСС ОРУЖИЯ: КОПЬЁ         │
│                                      │
│   Копьё — мощное оружие ближнего     │
│   и дальнего боя.                    │
│                                      │
│   Некоторые миссии этой зоны         │
│   требуют копьё определённой         │
│   силы.                              │
│                                      │
│   Копьё уже в твоём снаряжении!     │
│                                      │
│          [В БОЙ]                     │
│                                      │
└──────────────────────────────────────┘
```

**Кнопка:** «В бой» → закрывает popup, запускает g7 миссию.

**Что решает:** Проблема #3 (Spear без onboarding). Игрок знает: Spear есть, миссии будут требовать его.

**Реализация:** Простой popup. Триггер — при переходе к первой миссии Zone 2.

---

### 1.5 «Новый класс оружия: Boomerang»

**Триггер:** Первый вход в Zone 3 Campaign (g15).

**Экран:** Аналогично 1.4, но с Boomerang art и текстом:

```
   НОВЫЙ КЛАСС ОРУЖИЯ: БУМЕРАНГ

   Бумеранг — универсальное оружие
   средней дальности.

   В этой зоне тебе понадобится
   бумеранг.

   Бумеранг уже в твоём снаряжении!
```

**Что решает:** Проблема #3 (Boomerang без onboarding).

---

### 1.6 «Первый Gate Block»

**Триггер:** Первый раз, когда игрок видит gate popup (обычно g6, teaching gate).

**Дополнительный overlay** поверх стандартного gate popup:

```
┌──────────────────────────────────────┐
│  ┌────────────────────────────────┐  │
│  │   [Standard Gate Popup]        │  │
│  │   Bow damage: 35 / need: 37   │  │
│  │   [Go to Weapons]             │  │
│  └────────────────────────────────┘  │
│                                      │
│  ╔══════════════════════════════════╗ │
│  ║  ПОДСКАЗКА                      ║ │
│  ║                                 ║ │
│  ║  Чтобы пройти миссию, прокачай  ║ │
│  ║  оружие нужного класса.         ║ │
│  ║                                 ║ │
│  ║  Жетоны для прокачки можно      ║ │
│  ║  получить в Контрактах.         ║ │
│  ╚══════════════════════════════════╝ │
└──────────────────────────────────────┘
```

**Что решает:** Проблема #5 (gate popup не говорит WHERE) + первичный tutorial gate system.

**Реализация:** Tooltip overlay, показывается один раз поверх `DamageGatePopupController`.

---

### 1.7 «Первый Boss Gate Block»

**Триггер:** Первый раз, когда Boss gate popup появляется.

**Дополнительный overlay:**

```
  ╔══════════════════════════════════╗
  ║  ПОДСКАЗКА                      ║
  ║                                 ║
  ║  Для доступа к Боссу прокачай   ║
  ║  арбалет.                       ║
  ║                                 ║
  ║  Жетоны арбалета можно          ║
  ║  получить в Снайперских         ║
  ║  миссиях.                       ║
  ╚══════════════════════════════════╝
```

**Что решает:** Проблема #2 (Sniper → Crossbow → Boss связка). Чёткий path: Boss blocked → Sniper → CrossbowTokens → upgrade → Boss.

---

### 1.8 «Первый Forced Purchase»

**Триггер:** Первый раз, когда gate popup с `CanUpgradeToPass = false` (т.е. нужна покупка нового оружия).

**Дополнительный overlay:**

```
  ╔══════════════════════════════════╗
  ║  ПОДСКАЗКА                      ║
  ║                                 ║
  ║  Твоё оружие на максимуме.      ║
  ║  Для этой миссии нужно          ║
  ║  более мощное оружие.           ║
  ║                                 ║
  ║  Перейди в снаряжение и         ║
  ║  купи новую версию.             ║
  ╚══════════════════════════════════╝
```

**Что решает:** Проблема #5 (popup clarity). Явно объясняет разницу между «прокачай» и «купи новое».

---

### Summary: First-Time Popups

| ID | Popup | Триггер | Решает проблемы |
|----|-------|---------|----------------|
| 1.1 | Contracts unlocked | Contracts tab available | #1, #6 |
| 1.2 | Sniper unlocked | Sniper tab available | #2, #6 |
| 1.3 | Boss available | Campaign zone complete | #2 |
| 1.4 | Spear class | Enter Zone 2 Campaign | #3 |
| 1.5 | Boomerang class | Enter Zone 3 Campaign | #3 |
| 1.6 | First gate block | First gate popup | #5 |
| 1.7 | First Boss block | First Boss gate popup | #2 |
| 1.8 | First forced purchase | First CanUpgradeToPass=false | #5 |

**Общая реализация:** `FirstTimePopupService` с dictionary `<string, bool>` saved через PlayerPrefs. Каждый popup — `ScreenView` subclass или overlay component. Триггеры — в `PlayMissionPresenter`, `MissionAvailabilityService`, или mission selection UI. Затраты: 1 сервис + 8 popup prefabs. Нет балансовых изменений.

---

## 2. Улучшение Gate Popup Hints

Текущий `DamageGatePopupController` показывает:
- Текущий damage vs. требуемый damage
- Hint: «Upgrade» (CanUpgradeToPass=true) или «Buy new weapon» (false)
- Кнопка: «Go to Weapons»

### Проблема
Popup не говорит, **где взять ресурсы** для upgrade/purchase. Игрок знает «нужно 15 tokens», но не знает «tokens можно получить в Contracts».

### Предлагаемые 4 варианта hint'ов

**Вариант A — Static token source hint (минимальный):**

Добавить в popup текст:

```
Жетоны [WeaponClass] можно получить:
• Контракты — жетоны всех видов
• Снайпер — жетоны арбалета (только Crossbow)
```

Логика: if weapon == Crossbow → show «Снайпер»; else → show «Контракты». Статический текст, привязан к weapon class в `DamageGatePopupArgs`.

**Effort:** Минимальный. Добавить 1–2 строки в popup layout + условный текст в `DamageGatePopupController`.

---

**Вариант B — Dynamic resource check (средний):**

Проверить текущие ресурсы игрока и показать конкретный hint:

```
if (hasEnoughTokens && hasEnoughCash):
    "У тебя достаточно ресурсов! Перейди в снаряжение."
elif (hasEnoughCash && !hasEnoughTokens):
    "Не хватает жетонов. Пройди [Контракты/Снайпер], чтобы заработать."
elif (!hasEnoughCash):
    "Не хватает монет. Пройди несколько миссий."
```

**Effort:** Средний. Нужен access к `CurrencyService` из popup. `DamageGatePopupArgs` расширяется полями `hasEnoughTokens`, `hasEnoughCash`.

---

**Вариант C — Shortcut button (средний):**

Добавить вторую кнопку в popup:

```
[К СНАРЯЖЕНИЮ]    [К КОНТРАКТАМ]
```

«К Контрактам» → закрывает popup, переключает tab на Contracts.
Для Crossbow: «К Снайперу» → переключает на Sniper tab.

**Effort:** Средний. Второй button в popup layout. Навигация через `UINavigator` / tab switching.

---

**Вариант D — Recommended action (полный):**

Popup анализирует текущее состояние и рекомендует конкретное действие:

```
╔══════════════════════════════════════╗
║  РЕКОМЕНДАЦИЯ                       ║
║                                     ║
║  Пройди 2 контракта, чтобы          ║
║  заработать нужные жетоны.          ║
║                                     ║
║  [К КОНТРАКТАМ]  [К СНАРЯЖЕНИЮ]     ║
╚══════════════════════════════════════╝
```

Расчёт: `missingTokens = requiredTokens - currentTokens; runsNeeded = ceil(missingTokens / tokensPerContractRun)`.

**Effort:** Высокий. Нужен расчёт missing resources, estimation runs needed, два варианта кнопок. Зависит от reward estimation logic.

---

### Рекомендация

**Phase A → Вариант A** (статический hint). Минимальные затраты, решает основную проблему.
**Phase B → Вариант C** (shortcut button). Добавляет actionability — игрок может сразу перейти к нужному режиму.
**Phase C → Вариант D** (recommended action). Только если данные показывают, что игроки всё ещё теряются после Phase A+B.

Вариант B (dynamic check) можно skip — его ценность покрывается Вариантом D, а сам по себе он не добавляет actionability.

---

## 3. Mission Type Cards / Tooltips

### Проблема
Игрок видит tabs: Campaign, Contracts, Sniper, Boss. Нет информации о наградах, gameplay, или зачем играть каждый тип.

### Решение: Info Icon на каждом tab

На каждом mission type tab — маленькая `[i]` иконка. При нажатии — tooltip/card:

---

**Campaign card:**

```
┌──────────────────────────────────┐
│  КАМПАНИЯ                        │
│                                  │
│  Основная линия миссий.          │
│  Продвигайся по зонам и          │
│  открывай новые вызовы.          │
│                                  │
│  Награды:                        │
│  💰 Монеты                       │
│  🎯 Жетоны класса миссии         │
│                                  │
│  Каждая миссия требует           │
│  определённый класс оружия.      │
│                                  │
│              [OK]                │
└──────────────────────────────────┘
```

---

**Contracts card:**

```
┌──────────────────────────────────┐
│  КОНТРАКТЫ                       │
│                                  │
│  Повторные миссии за награды.    │
│  Сложность растёт с каждым       │
│  выполненным контрактом.         │
│                                  │
│  Награды:                        │
│  💰 Монеты                       │
│  🎯 Жетоны ВСЕХ классов оружия   │
│                                  │
│  Лучший способ получить          │
│  жетоны для прокачки.            │
│                                  │
│              [OK]                │
└──────────────────────────────────┘
```

---

**Sniper card:**

```
┌──────────────────────────────────┐
│  СНАЙПЕР                         │
│                                  │
│  Миссии с арбалетом.             │
│  Каждый враг — один точный       │
│  выстрел.                        │
│                                  │
│  Награды:                        │
│  💰 Монеты (увеличенные)         │
│  🎯 Жетоны арбалета              │
│                                  │
│  Прокачивай арбалет, чтобы       │
│  открыть Босса зоны!             │
│                                  │
│              [OK]                │
└──────────────────────────────────┘
```

---

**Boss card:**

```
┌──────────────────────────────────┐
│  БОСС ЗОНЫ                       │
│                                  │
│  Сильнейший враг зоны.           │
│  Победи его, чтобы получить      │
│  большую награду.                │
│                                  │
│  Награды:                        │
│  💰 Монеты (большие)             │
│  🎯 Жетоны ВСЕХ классов          │
│                                  │
│  Требуется достаточно            │
│  сильный арбалет!                │
│                                  │
│              [OK]                │
└──────────────────────────────────┘
```

---

**Реализация:** 4 popup'а (`MissionTypeInfoPopup`), привязанных к `[i]` button на каждом tab. Текст — static strings (или localization keys). Не зависит от баланса.

**Effort:** Низкий. 4 текстовых popup'а + 4 кнопки в tab headers.

---

## 4. Contracts — Идеи мотивации

Contracts — функциональный grind-mode, но мотивационно пустой. Ниже — 10 идей для добавления meaning без крупного redesign'а.

---

### 4.1 Milestone Rewards (P0, Low effort)

**Идея:** Каждые N контрактов — бонусная награда.

| Контракт # | Milestone Reward |
|------------|-----------------|
| 3 | +50 cash бонус |
| 5 | +5 AllTokens бонус |
| 10 | +100 cash + 10 AllTokens |
| 15 | +150 cash + 15 AllTokens |
| 20 | +200 cash + 20 AllTokens |

**UI:** Прогресс-бар на Contracts tab: «До бонуса: 2/5 контрактов».

**Что решает:** Micro-goal внутри grind. Игрок видит «ещё 2 контракта до бонуса» — не бесконечный grind, а конкретная цель.

**Effort:** Low. Счётчик completions + reward popup + progress bar widget.

---

### 4.2 Daily Contract Bonus (P1, Low effort)

**Идея:** Первый контракт в день — двойная награда.

```
ЕЖЕДНЕВНЫЙ БОНУС
Первый контракт за день даёт
двойные награды!
```

**UI:** Badge «x2» на Contracts tab, если daily bonus available.

**Что решает:** Retention hook. Причина заходить каждый день.

**Effort:** Low. Date check + multiplier on first daily completion.

---

### 4.3 Contract Streak Counter (P1, Low effort)

**Идея:** Серия (streak) контрактов подряд без поражения → growing bonus.

| Streak | Bonus |
|--------|-------|
| 3 | +10% cash |
| 5 | +20% cash + 1 extra AllToken |
| 10 | +50% cash + 3 extra AllTokens |

**UI:** Streak counter на victory screen: «Серия: 5 контрактов!»

**Что решает:** Skill-based engagement. Хорошие игроки получают больше → sense of mastery.

**Effort:** Low. Counter reset on fail + multiplier on reward.

---

### 4.4 Contract Challenge / Modifier (P2, Medium effort)

**Идея:** Каждый контракт получает случайный modifier:

- «Быстрый бой» — таймер, бонус за скорость
- «Без урона» — бонус за 0 damage taken
- «Точный стрелок» — бонус за accuracy
- «Минимум выстрелов» — бонус за economy

**UI:** Modifier показывается перед стартом миссии. Бонус — дополнительные tokens или cash.

**Что решает:** Variety внутри повторных миссий. Каждый контракт ощущается чуть по-другому.

**Effort:** Medium. Modifier system + condition tracking + bonus calculation. Зависит от наличия accuracy/damage/time stats в gameplay.

---

### 4.5 «Контракт дня» — Featured Mission (P2, Medium effort)

**Идея:** Вместо random mission — одна featured mission в день с повышенными наградами.

```
КОНТРАКТ ДНЯ
Миссия: [Zone 1, Mission 3]
Награда: x1.5

Обновление через: 14:32:05
```

**Что решает:** Shared daily goal. Если есть social features — все играют одну миссию. Если нет — просто variety.

**Effort:** Medium. Daily rotation logic + UI highlight.

---

### 4.6 Token Class Choice (P2, Medium effort)

**Идея:** Перед стартом контракта игрок выбирает **один бонусный класс** — получает +2 extra tokens этого класса помимо AllTokens.

```
ВЫБЕРИ БОНУСНЫЙ КЛАСС:
[Bow +2]  [Spear +2]  [Shuriken +2]  [Boom +2]
```

**Что решает:** Decision-making в Contracts. Игрок выбирает, какой класс фармить. Adds player agency.

**Effort:** Medium. Class selection UI + modified reward formula.

---

### 4.7 Contracts Rank / Title (P2, Low effort)

**Идея:** Декоративный rank, растущий с числом completed contracts:

| Contracts | Rank |
|-----------|------|
| 0 | Новобранец |
| 5 | Наёмник |
| 15 | Ветеран |
| 30 | Элита |
| 50 | Легенда |

**UI:** Rank badge на Contracts tab + popup при повышении.

**Что решает:** Sense of progression в otherwise flat mode. Cosmetic reward.

**Effort:** Low. Thresholds + string lookup + badge display.

---

### 4.8 «Серия контрактов» — Mini-Campaign (P3, High effort)

**Идея:** 3 контракта подряд (не random, а фиксированная последовательность) с narrative frame и bonus reward за серию.

```
СЕРИЯ КОНТРАКТОВ: «Зачистка периметра»
Миссия 1/3: [Zone 1, Mission 2]
Миссия 2/3: [Zone 1, Mission 5]
Миссия 3/3: [Zone 2, Mission 1]

Бонус за серию: 100 cash + 5 AllTokens
```

**Что решает:** Narrative frame для grind. Контракты ощущаются как мини-история, не как random replay.

**Effort:** High. Series definition data + progress tracking + bonus reward + UI.

---

### 4.9 Contracts Leaderboard / Score (P3, Medium effort)

**Идея:** Personal best score (time, accuracy, damage taken) per contract mission. Beat your score → extra reward.

**Что решает:** Replayability через self-competition. Работает без social features.

**Effort:** Medium. Score tracking per mission + comparison + bonus calculation.

---

### 4.10 Random Weapon Restriction (P3, Medium effort)

**Идея:** Некоторые контракты требуют **конкретный weapon class** (как Campaign). Это даёт reason to upgrade all weapons, не только main ones.

**Что решает:** Weapon diversity. Boomerang и Spear получают использование за пределами Campaign.

**Effort:** Medium. Weapon requirement on Contract missions + gate check. Риск: может заблокировать grind, если weapon too weak. Нужен fallback (always one no-restriction contract available).

---

### Summary: Contracts Ideas

| ID | Идея | Priority | Effort | Impact |
|----|------|----------|--------|--------|
| 4.1 | Milestone rewards | P0 | Low | High |
| 4.2 | Daily bonus | P1 | Low | Medium |
| 4.3 | Streak counter | P1 | Low | Medium |
| 4.4 | Challenge modifiers | P2 | Medium | High |
| 4.5 | Featured mission | P2 | Medium | Medium |
| 4.6 | Token class choice | P2 | Medium | Medium |
| 4.7 | Rank/title | P2 | Low | Low |
| 4.8 | Mini-campaign series | P3 | High | High |
| 4.9 | Leaderboard/score | P3 | Medium | Medium |
| 4.10 | Random weapon req | P3 | Medium | Medium |

**Рекомендация:** Начать с **4.1 (Milestones)** — минимальные затраты, максимальный impact. Потом **4.2 + 4.3** (daily + streak) для retention. Остальное — по результатам тестирования.

---

## 5. Sniper — Идеи мотивации

Sniper уже имеет unique gameplay (one-shot) и direct meta-purpose (CrossbowTokens → Boss gate). Проблемы:
- Мало контента (7 миссий)
- Связка Sniper → Boss не объяснена (решается popup 1.2 и 1.7)
- Нет internal progression в Sniper

---

### 5.1 Sniper Accuracy Rating (P1, Low effort)

**Идея:** После каждой Sniper mission — accuracy rating (shots fired / enemies killed). Higher accuracy → bonus reward.

| Accuracy | Rating | Bonus |
|----------|--------|-------|
| 100% | Идеально | +2 CrossbowTokens |
| 90%+ | Отлично | +1 CrossbowToken |
| 80%+ | Хорошо | — |
| <80% | Сносно | — |

**UI:** Rating screen после victory с accuracy % и star rating.

**Что решает:** Skill-based engagement. Sniper gameplay идеально подходит для accuracy metric.

**Effort:** Low. Accuracy tracking (shots fired already tracked?) + rating display + conditional bonus.

---

### 5.2 Sniper Time Challenge (P2, Medium effort)

**Идея:** Таймер на каждой Sniper mission. Beat par time → bonus cash.

```
Par Time: 45s
Your Time: 38s — РЕКОРД!
Бонус: +50 cash
```

**Что решает:** Replayability для 7 Sniper missions. Speed-running adds variety.

**Effort:** Medium. Timer + par time per mission + personal best tracking.

---

### 5.3 Sniper Progress Tracker (P1, Low effort)

**Идея:** Визуальный прогресс на Sniper tab:

```
Арбалет: Lv.4 (dmg 28)
Следующий: Lv.5 (dmg 30) — нужно 3 жетона
До Босса зоны 1: ✅ (хватает)
До Босса зоны 2: 37 — нужно ещё 9 dmg
```

**Что решает:** Прямая визуальная связка Sniper → Boss gate. Игрок видит конкретный прогресс к конкретной цели.

**Effort:** Low. Read current Crossbow stats + Boss gate thresholds (уже есть в `BalanceConfig`). Display widget.

---

### 5.4 Sniper Weapon Skin Unlock (P3, Medium effort)

**Идея:** После 5 / 10 / 15 Sniper completions — cosmetic Crossbow skin.

**Что решает:** Collecting motivation для Sniper. Если skin system уже существует (есть `PlayerSkinDatabase`), расширить на weapon skins.

**Effort:** Medium. Skin assets + unlock condition + application.

---

### Summary: Sniper Ideas

| ID | Идея | Priority | Effort | Impact |
|----|------|----------|--------|--------|
| 5.1 | Accuracy rating | P1 | Low | Medium |
| 5.2 | Time challenge | P2 | Medium | Medium |
| 5.3 | Progress tracker | P1 | Low | High |
| 5.4 | Weapon skin unlock | P3 | Medium | Low |

**Рекомендация:** **5.3 (Progress tracker)** — самый ценный. Прямо показывает «ты здесь, Boss требует вот столько». Потом **5.1 (Accuracy rating)** для skill engagement.

---

## 6. Weapon Class Intro / Unlock Moments

### Текущее состояние
Spear и Boomerang появляются в Campaign missions без предупреждения. Игрок впервые видит gate popup и может не понимать, что это за weapon class.

### 6.1 Weapon Class Unlock Popup (описан в 1.4 / 1.5)

Popup'ы при первом появлении Spear (g7) и Boomerang (g15) — см. раздел 1.

### 6.2 Weapon Class Preview в Loadout

**Идея:** До unlock'а weapon class — показывать silhouette / locked icon в loadout с подписью «Откроется в Зоне N».

```
Loadout Screen:
[Bow: Lv.3]  [Xbow: Lv.0]  [Shuriken: Lv.0]
[🔒 Копьё: Зона 2]  [🔒 Бумеранг: Зона 3]
```

**Что решает:** Anticipation. Игрок видит, что будут новые weapon classes, и ждёт их.

**Effort:** Low. Locked state в loadout UI + zone index check.

---

### 6.3 Weapon Class Trial

**Идея:** При первом появлении weapon class (g7 для Spear, g15 для Boomerang) — mission-specific tutorial hint:

```
Это твоя первая миссия с копьём!
Копьё наносит высокий урон
с каждого броска.
```

Hint появляется **в gameplay** (overlay text или brief pause) в первые 5 секунд миссии.

**Что решает:** In-gameplay onboarding. Игрок понимает, как играть с новым weapon class.

**Effort:** Medium. In-game overlay system + trigger per weapon class + text content.

---

### Summary: Weapon Class Moments

| ID | Идея | Priority | Effort |
|----|------|----------|--------|
| 6.1 | Unlock popup (1.4/1.5) | P0 | Low |
| 6.2 | Locked preview in loadout | P1 | Low |
| 6.3 | In-game trial hint | P2 | Medium |

---

## 7. Reward Beats и Victory Moments

### Проблема
Нет «больших моментов» в progression. Rewards одинаково отображаются (cash + tokens на victory screen). Нет разницы между «первая миссия» и «forced purchase gate passed».

### 7.1 Zone Complete Celebration (P0, Low effort)

**Триггер:** Последняя Campaign mission в зоне завершена.

```
┌──────────────────────────────────────┐
│                                      │
│     [Zone Art / Banner]              │
│                                      │
│   ЗОНА 1 ПРОЙДЕНА!                  │
│                                      │
│   Ты завершил все кампании           │
│   Японской зоны.                     │
│                                      │
│   Босс зоны теперь доступен!        │
│                                      │
│   Итого заработано:                  │
│   💰 637 монет                       │
│   🎯 4 жетона лука                   │
│   🎯 3 жетона сюрикена               │
│                                      │
│       [К БОССУ]  [ДАЛЕЕ]             │
│                                      │
└──────────────────────────────────────┘
```

**Что решает:** Milestone moment. Ощущение «я что-то завершил» + прямой путь к Boss.

**Effort:** Low. Trigger = zone completion check + summary popup + optional totals display.

---

### 7.2 Boss Victory Celebration (P0, Low effort)

**Триггер:** Boss defeated.

```
┌──────────────────────────────────────┐
│                                      │
│     [Boss Defeated Art]              │
│                                      │
│   БОСС ПОБЕЖДЁН!                    │
│                                      │
│   Ты покорил [Zone Name]!           │
│                                      │
│   Награды:                           │
│   💰 300 монет                       │
│   🎯 3 жетона каждого класса        │
│                                      │
│   Следующая зона разблокирована!    │
│                                      │
│          [ДАЛЕЕ]                     │
│                                      │
└──────────────────────────────────────┘
```

**Что решает:** Biggest reward beat в игре. Boss — climax зоны, и victory должен ощущаться как таковой.

**Effort:** Low. Special victory screen after Boss mission type detected.

---

### 7.3 Weapon Upgrade Confirmation (P1, Low effort)

**Триггер:** После upgrade или purchase оружия, которое проходит gate.

```
┌──────────────────────────────────────┐
│                                      │
│   [Weapon Art: Bow Lv.4]            │
│                                      │
│   ЛУК УЛУЧШЕН!                      │
│   Урон: 35 → 43                     │
│                                      │
│   Теперь ты можешь пройти           │
│   миссию [Campaign g9]!             │
│                                      │
│        [ИГРАТЬ]  [OK]                │
│                                      │
└──────────────────────────────────────┘
```

**Что решает:** Immediate feedback loop. Upgrade → gate opens → play. Прямая actionability.

**Effort:** Low. After upgrade → check if any previously blocked mission now passes → show hint. Зависит от доступа к gate check API из weapon upgrade UI.

---

### 7.4 First Kill With New Weapon (P2, Low effort)

**Триггер:** Первое убийство с оружием, которое было только что upgraded / purchased.

In-game floating text:

```
   НОВАЯ СИЛА!
   Урон: 43
```

**Что решает:** Tangible feel of upgrade. Игрок видит, что upgrade имел эффект.

**Effort:** Low. Damage number already shown? Add special effect on first hit with newly upgraded weapon.

---

### 7.5 Zone 3 Campaign Complete — Loop Transition Beat (P0, Low effort)

**Триггер:** Последняя миссия Zone 3 (g22) завершена.

```
┌──────────────────────────────────────┐
│                                      │
│     [Campaign Complete Banner]       │
│                                      │
│   КАМПАНИЯ ЗАВЕРШЕНА!               │
│                                      │
│   Ты прошёл все три зоны.           │
│   Но враги стали сильнее...         │
│                                      │
│   НОВОЕ ИСПЫТАНИЕ начинается!       │
│                                      │
│   • Враги мощнее                     │
│   • Ворота усилены                   │
│   • Награды увеличены                │
│                                      │
│       [НАЧАТЬ ИСПЫТАНИЕ]             │
│                                      │
└──────────────────────────────────────┘
```

**Что решает:** Проблема #4 (silent loop reset). Игрок понимает: это не баг, это new challenge.

**Effort:** Low. Trigger = Zone 3 Campaign complete + loop counter increment. Popup before loop reset.

---

### Summary: Reward Beats

| ID | Момент | Триггер | Priority | Effort |
|----|--------|---------|----------|--------|
| 7.1 | Zone complete | Last Campaign mission in zone | P0 | Low |
| 7.2 | Boss victory | Boss defeated | P0 | Low |
| 7.3 | Upgrade → gate opens | Weapon upgrade passes gate | P1 | Low |
| 7.4 | First kill with upgrade | First hit after upgrade | P2 | Low |
| 7.5 | Loop transition | g22 complete | P0 | Low |

---

## 8. Loop Transition Design

### Текущее состояние
После Zone 3 Campaign complete → silent reset to Zone 1 с multiplied gates. Нет UI, нет explanation, нет ceremony.

### 8.1 Loop Transition Screen (описан в 7.5)

Popup / full-screen с объяснением: «Кампания завершена, начинается новое испытание, враги сильнее, награды выше».

### 8.2 Loop Badge / Indicator

**Идея:** На main menu — видимый loop indicator:

```
Loop 0: нет badge
Loop 1: ★ badge
Loop 2: ★★ badge
```

**UI:** Small badge рядом с zone selector или player name.

**Что решает:** Visual acknowledgment — игрок видит, что он «дальше», чем loop 0 player.

**Effort:** Low. Loop counter display.

---

### 8.3 Loop Rewards Preview

**Идея:** На Loop transition screen — preview новых gate requirements vs. текущих weapon stats:

```
Новые требования:
Bow gate g6: 37 → 46 (×1.25)
Твой Bow: 55 — проходит ✅

Spear gate g10: 70 → 87.5 (×1.25)
Твой Spear: 65 — не проходит ❌
→ Нужен Spear upgrade!
```

**Что решает:** Clarity. Игрок знает, что ждёт в следующем loop, и может preparation.

**Effort:** Medium. Gate recalculation preview + comparison with current stats.

---

### 8.4 Loop-Specific Narrative Frame

**Идея:** Каждый loop — минимальный narrative:

```
Loop 1: "Враги вернулись сильнее. Покажи, на что ты способен."
Loop 2: "Последнее испытание. Только лучшие выживут."
```

1–2 строки текста на Loop transition screen.

**Что решает:** Meaning. Loop — не «тот же контент с bigger numbers», а «новый challenge с narrative frame».

**Effort:** Low. Static text per loop index.

---

### Summary: Loop Transition

| ID | Идея | Priority | Effort |
|----|------|----------|--------|
| 8.1 | Transition screen | P0 | Low |
| 8.2 | Loop badge | P1 | Low |
| 8.3 | Requirements preview | P2 | Medium |
| 8.4 | Narrative frame | P1 | Low |

---

## 9. Zone 2 Gate Wall — Motivation Layer

Zone 2 имеет 7/8 gates. Это структурная проблема (решается балансом — P0-1 из аудита). Но даже при текущем pacing можно добавить мотивационный слой:

### 9.1 Gate Progress Bar для Zone

**Идея:** На zone map — visual progress:

```
Зона 2: ████████░░░░ 3/8 миссий
Следующий блок: Shuriken dmg 26
Твой Shuriken: 25 — нужен +1 upgrade
```

**Что решает:** Visibility. Игрок видит, сколько осталось, и что конкретно нужно для следующей миссии.

**Effort:** Low. Progress bar + next gate preview.

---

### 9.2 Mid-Zone Milestone

**Идея:** После прохождения 4/8 миссий зоны — маленький reward beat:

```
ЗОНА 2: ПОЛОВИНА ПРОЙДЕНА!
Бонус: +50 cash + 3 AllTokens
```

**Что решает:** Разбивает wall. Вместо 8 gates подряд — 4 + milestone + 4.

**Effort:** Low. Midpoint check + bonus reward popup.

---

### 9.3 «Рекомендуемое действие» перед blocked mission

**Идея:** На mission card, которая заблокирована, вместо серого «Play» показать:

```
[Bow: 35/43 dmg]
Рекомендация: пройди 1 контракт,
чтобы заработать жетон лука.
[К КОНТРАКТАМ]
```

**Что решает:** Proactive guidance вместо reactive gate popup. Игрок знает, что делать, до нажатия Play.

**Effort:** Medium. Pre-gate check на mission card + recommendation logic.

---

### Summary: Zone 2 Motivation

| ID | Идея | Priority | Effort |
|----|------|----------|--------|
| 9.1 | Zone progress bar | P1 | Low |
| 9.2 | Mid-zone milestone | P1 | Low |
| 9.3 | Pre-gate recommendation | P2 | Medium |

---

## 10. Priority Plan

### P0 — Must Have (решает критические проблемы)

| ID | Что | Проблема | Effort |
|----|-----|----------|--------|
| 1.6 | First gate block tutorial | #5 (no resource hint) | Low |
| 1.7 | First Boss block tutorial | #2 (Sniper→Boss link) | Low |
| 2-A | Static token source hint | #5 (no resource hint) | Low |
| 7.1 | Zone complete celebration | #8 (no reward beats) | Low |
| 7.2 | Boss victory celebration | #8 (no reward beats) | Low |
| 7.5 | Loop transition screen | #4 (silent loop reset) | Low |
| 4.1 | Contracts milestones | #1 (empty Contracts) | Low |

**Total P0:** 7 items, all Low effort.

---

### P1 — Should Have (улучшает UX и мотивацию)

| ID | Что | Проблема | Effort |
|----|-----|----------|--------|
| 1.1 | Contracts unlock popup | #1, #6 | Low |
| 1.2 | Sniper unlock popup | #2, #6 | Low |
| 1.3 | Boss available popup | #2 | Low |
| 1.4 | Spear class popup | #3 | Low |
| 1.5 | Boomerang class popup | #3 | Low |
| 1.8 | First forced purchase tutorial | #5 | Low |
| 2-C | Shortcut button in gate popup | #5 | Medium |
| 3.x | Mission type info cards | #6 | Low |
| 4.2 | Daily contract bonus | #1 | Low |
| 4.3 | Contract streak | #1 | Low |
| 5.1 | Sniper accuracy rating | #2 | Low |
| 5.3 | Sniper progress tracker | #2 | High impact, Low effort |
| 6.2 | Locked weapon preview | #3 | Low |
| 7.3 | Upgrade → gate opens hint | — | Low |
| 8.2 | Loop badge | #4 | Low |
| 8.4 | Loop narrative frame | #4 | Low |
| 9.1 | Zone progress bar | #8 | Low |
| 9.2 | Mid-zone milestone | #8 | Low |

**Total P1:** 18 items, mostly Low effort.

---

### P2 — Nice to Have

| ID | Что | Effort |
|----|-----|--------|
| 2-D | Recommended action in popup | High |
| 4.4 | Contract challenge modifiers | Medium |
| 4.5 | Featured daily mission | Medium |
| 4.6 | Token class choice | Medium |
| 5.2 | Sniper time challenge | Medium |
| 6.3 | In-game weapon trial hint | Medium |
| 7.4 | First kill with upgrade | Low |
| 8.3 | Loop requirements preview | Medium |
| 9.3 | Pre-gate recommendation | Medium |

**Total P2:** 9 items, mostly Medium effort.

---

### P3 — Future

| ID | Что | Effort |
|----|-----|--------|
| 4.7 | Contracts rank/title | Low |
| 4.8 | Mini-campaign series | High |
| 4.9 | Contracts leaderboard | Medium |
| 4.10 | Random weapon restriction | Medium |
| 5.4 | Sniper weapon skin | Medium |

**Total P3:** 5 items.

---

## 11. Implementation Roadmap

### Phase A — Core Clarity (1 sprint)

**Цель:** Игрок понимает, что делать при блоке.

1. `FirstTimePopupService` — framework для one-time popups (PlayerPrefs-based)
2. Gate popup static hint (Variant A) — «жетоны в Контрактах / Снайпере»
3. First gate block tutorial overlay (1.6)
4. First Boss block tutorial overlay (1.7)
5. Loop transition screen (7.5)

**Результат:** Новый игрок получает guidance при каждом блоке. Loop transition не silent.

**Dependencies:** `DamageGatePopupController` (для hint добавления), `PlayMissionPresenter` (для tutorial triggers), loop reset logic (для transition screen trigger).

---

### Phase B — Reward Beats (1 sprint)

**Цель:** Прохождение ощущается как achievement.

1. Zone complete celebration (7.1)
2. Boss victory celebration (7.2)
3. Contracts milestone rewards (4.1) + progress bar UI

**Результат:** 3 момента «я что-то завершил» добавлены в progression.

**Dependencies:** Zone completion check (уже есть в `ZoneData.IsBossUnlocked()`), Boss mission type detection, Contracts completion counter.

---

### Phase C — Onboarding (1 sprint)

**Цель:** Каждый новый контент — объяснён.

1. Contracts unlock popup (1.1)
2. Sniper unlock popup (1.2)
3. Boss available popup (1.3)
4. Spear class popup (1.4)
5. Boomerang class popup (1.5)
6. First forced purchase tutorial (1.8)
7. Mission type info cards (3.x)

**Результат:** Все mission types и weapon classes имеют intro moment.

**Dependencies:** `MetaModeUnlockConfig` (для mode unlock detection), mission index tracking (для weapon class first appearance).

---

### Phase D — Engagement Features (1–2 sprints)

**Цель:** Grind-modes ощущаются engaging.

1. Daily contract bonus (4.2)
2. Contract streak (4.3)
3. Sniper accuracy rating (5.1)
4. Sniper progress tracker (5.3)
5. Shortcut button in gate popup (2-C)
6. Upgrade → gate opens hint (7.3)

**Результат:** Contracts и Sniper имеют internal motivation. Gate popup actionable.

**Dependencies:** Accuracy stat tracking (для Sniper), daily timestamp tracking, `CurrencyService` access from popup (для upgrade check).

---

### Phase E — Polish (1–2 sprints)

**Цель:** Additional variety и clarity.

1. Zone progress bar (9.1)
2. Mid-zone milestone (9.2)
3. Loop badge (8.2) + narrative frame (8.4)
4. Locked weapon preview (6.2)
5. Contract challenge modifiers (4.4) — если resources allow

**Результат:** Full motivation layer. Все проблемы из аудита addressed.

**Dependencies:** Modifier system requires gameplay stat hooks. Others — UI only.

---

### Phase Summary

| Phase | Sprint | Items | Focus |
|-------|--------|-------|-------|
| A | 1 | 5 | Gate clarity + loop transition |
| B | 1 | 3 | Reward beats |
| C | 1 | 7 | Onboarding popups |
| D | 1–2 | 6 | Engagement features |
| E | 1–2 | 5 | Polish + modifiers |

---

## 12. Технические зависимости

### Новые сервисы

| Сервис | Роль | Где создаётся |
|--------|------|---------------|
| `FirstTimePopupService` | Tracks shown popups via PlayerPrefs | `MainMenuRuntime.Awake()` |
| `ContractsMilestoneService` | Tracks Contracts completions, triggers milestones | `MainMenuRuntime.Awake()` |
| `DailyBonusService` | Tracks daily first completion | `MainMenuRuntime.Awake()` |

### Модифицируемые файлы

| Файл | Изменение |
|------|-----------|
| `DamageGatePopupController.cs` | Добавить token source hint text + shortcut button |
| `DamageGatePopupArgs.cs` | Добавить `tokenSourceHint` string field |
| `PlayMissionPresenter.cs` | Trigger first-time tutorials, zone complete check |
| `MissionStartService.cs` | Trigger weapon class intro popups |
| `MainMenuRuntime.cs` | Register new services |

### Новые UI prefabs

| Prefab | Тип |
|--------|-----|
| `FirstTimePopup` | Reusable template с title, body, icon, 1–2 buttons |
| `ZoneCompletePopup` | Zone-specific celebration с totals |
| `BossVictoryPopup` | Boss-specific celebration |
| `LoopTransitionScreen` | Full-screen с loop info |
| `MissionTypeInfoCard` | 4 variants (Campaign, Contracts, Sniper, Boss) |
| `ContractsMilestonePopup` | Milestone reward display |

### Localization

Все тексты в popup'ах → через localization system (если есть) или string constants (если нет). Аудит отмечал, что Boss hint text hardcoded English — этот план предполагает русскоязычные тексты, но **все strings должны быть готовы к localization**.

---

## 13. Вопросы к дизайнеру

1. **Popup fatigue:** 8 first-time popup'ов + milestone popup'ы + celebration screens. Не слишком ли много popup'ов? Нужен ли «do not show again» toggle, или one-time показ достаточен?

2. **Contracts milestone rewards:** Предложенные значения (50 cash за 3 контракта, 5 AllTokens за 5, и т.д.) — ориентировочные. Нужна ли проверка с текущим balance, чтобы milestones не ломали экономику?

3. **Daily bonus magnitude:** x2 rewards за первый дневной контракт. Не слишком ли щедро? Или x1.5 достаточно? Или fixed flat bonus (+20 cash + 2 AllTokens)?

4. **Sniper accuracy tracking:** Есть ли уже tracking shots fired / enemies killed в Sniper gameplay? Если нет — реализация accuracy rating требует добавления gameplay stats.

5. **Token source hint — тон:** «Пройди Контракты, чтобы заработать жетоны» — это directive. Нужен ли более мягкий тон: «Жетоны можно заработать в Контрактах»? Или ещё мягче: просто иконка Contracts рядом с token display?

6. **Loop badge visibility:** Badge на main menu — видимый всем (если есть social features) или только игроку? Если social — badge = bragging rights. Если solo — просто visual indicator.

7. **Zone complete celebration — totals:** Показывать ли cumulative totals за всю зону (сколько заработано за N миссий)? Или только reward за последнюю миссию? Totals — более impressive, но требуют tracking.

8. **Pre-gate recommendation (9.3):** Рекомендация «пройди 1 контракт» требует estimation token income per Contracts run. Формула rewards уже доступна через `BalanceConfig`, но нужен ли этот level of detail, или достаточно generic «пройди контракты»?

9. **Contract challenge modifiers (4.4):** Есть ли в gameplay tracked stats (accuracy, damage taken, time, ammo used)? От этого зависит, какие modifiers возможны.

10. **Weapon class preview (6.2):** Показывать locked Spear и Boomerang в Loadout с самого начала? Или это спойлер? Если Zone 2/3 — surprise content, preview убирает surprise.

11. **Energy integration:** Popup'ы mention Contracts и Sniper как «пойди поиграй». Если энергия ограничена, игрок может не иметь возможности «просто пойти поиграть». Нужен ли учёт energy в рекомендациях?

12. **Localization scope:** Текущий Boss hint hardcoded English. Весь этот plan предполагает русские тексты. Будет ли localization system до реализации этих popup'ов, или hardcode Russian first?

13. **Contracts — AllTokens vs. class choice (4.6):** Идея выбора бонусного класса добавляет agency, но усложняет UI. Стоит ли это делать, или AllTokens + milestone — достаточно для motivation?
