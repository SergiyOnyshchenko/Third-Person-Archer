# Геймдизайн-аудит Meta-системы

**Дата:** 2026-05-28
**Контекст:** Third-Person Archer — mobile mission-based shooter с meta-progression
**Тип аудита:** Game Design (не code audit, не refactor)
**Базовые документы:** Meta_System_Audit.md, Meta_Balance_Audit.md, Meta_Balance_Fix_Plan.md, Meta_Balance_After_Fix.md, Meta_Balance_Pacing_Validation.md, Meta_Boss_Crossbow_Gate.md, Meta_Balance_After_BossGate.md, Enemy_Stats_Scaling_Audit.md, Enemy_Health_Clamp_Audit_And_Plan.md, Enemy_Health_Clamp_Implementation.md

---

## 1. Краткое резюме

Meta-система в текущем состоянии — это **работающий, связанный meta-loop**, а не просто набор отдельных систем. Ядро — Campaign → Gate → Upgrade → Boss — функционирует как единый путь прогрессии. После фаз балансировки (Phase 2–4) система стала значительно лучше: Spear/Boomerang больше не пролетают все gates бесплатно, token economy больше не затапливает игрока, Crossbow получил роль через Sniper → Boss связку.

**Однако** несколько рисков остаются:
- Zone 2 перегружена gates (7 из 8 миссий — блок), создавая "стену" вместо плавной кривой.
- Contracts как grind-mode остаётся функциональным, но **мотивационно пустым** — игрок не получает ни нарратива, ни разнообразия, ни sense of progress за прохождение Contracts.
- Weapon classes Spear и Boomerang имеют **слабую идентичность в meta**: Spear появляется только с Zone 2, Boomerang — только с Zone 3, при этом в зонах 1 и 3 у них нет своего "дома".
- Loop 2/3 механически работают (gate multipliers, reward multipliers), но **не дают нового контента или mechanics** — replay value строится только на числах.
- UX/подсказки недостаточны: игрок может не понять связь Sniper → Crossbow → Boss без прямого tutorial.

В целом: **система жизнеспособна и математически обоснована**, но требует work на мотивационном слое (зачем, а не как) и on pacing в Zone 2.

---

## 2. Core Loop

### Текущий loop (реконструкция)

```
New Player
  │
  ├─ Zone 1 Campaign (7 миссий, Bow/Shuriken)
  │    └─ g6: Teaching Gate (Bow L1) — первый блок
  │
  ├─ Contracts / Sniper (grind for tokens/cash)
  │
  ├─ Zone 1 Boss (Crossbow gate = 28)
  │    └─ Мотивация: играть Sniper → прокачивать Crossbow
  │
  ├─ Zone 2 Campaign (8 миссий, Bow/Shuriken/Spear)
  │    └─ 7 из 8 — блоки, включая Shuriken2 forced purchase
  │
  ├─ Contracts / Sniper (grind)
  │
  ├─ Zone 2 Boss (Crossbow gate = 37)
  │
  ├─ Zone 3 Campaign (8 миссий, все классы)
  │    └─ Bow2, Shuriken3, Spear2 forced purchases
  │
  └─ Zone 3 нет Boss → Loop End → New Loop (x1.25 / x1.55 gates)
```

### Оценка понятности для игрока

**Хорошо:**
- Campaign → Gate → Upgrade — это интуитивно. Popup объясняет: "твоё оружие слабое, прокачай или купи новое".
- Boss Crossbow Gate привязан к конкретному типу оружия — Crossbow. Popup говорит: "нужен Crossbow".
- WeaponSelectionScreen открывается с нужным классом — UX переход понятный.

**Проблемы:**
- **Связка Sniper → Crossbow не объяснена нигде.** Popup для Boss gate говорит "play Sniper missions to earn Crossbow tokens", но игрок может не знать, что Sniper — это режим, и не понимать, где его найти.
- **Contracts vs Sniper — неочевидный выбор.** Зачем мне Contracts, а не Sniper? Зачем Sniper, а не Contracts? Нет подсказки, какой режим даёт какие rewards.
- **Loop transition** (после Zone 3): просто сброс на Zone 1 с повышенными числами. Нет ни cutscene, ни reward beat, ни объяснения "ты прошёл игру, теперь сложнее". Это может ощущаться как баг.

---

## 3. Mission Types Design

### 3.1 Campaign

**Роль:** Основная progression. Линейная последовательность миссий с weapon class требованиями и damage gates.

**Мотивация игрока:** Продвигаться дальше, открывать зоны, добираться до Boss. Это core loop driver — работает хорошо.

**Награды (после Phase 2):** 70+7N cash, 1 flat token per mission. Rewards линейно растут, но token income плоский (1 за миссию). Это создаёт правильное давление: Campaign даёт cash, но tokens надо добывать через Contracts.

**Риски:**
- **Zone 2 block density (7/8 миссий — блок)** — см. раздел 7. Это главный frustration risk Campaign.
- **Weapon class rotation предсказуема** (Bow → Shuriken → Bow → Shuriken в Zone 1). Игрок может чувствовать однообразие.
- **Spear и Boomerang появляются поздно** (Spear — Zone 2, Boomerang — Zone 3), но игрок не получает ни unlock event, ни intro, ни tutorial для них.

**Связь с Meta:** Сильная. Campaign — единственный способ продвинуться к Boss, единственный источник zone unlock, основной source of cash.

---

### 3.2 Contracts

**Роль:** Grind-mode для cash и AllTokens. Рандомно выбирает уже пройденную Campaign миссию.

**Мотивация игрока:** Единственный обязательный grind-source для tokens (после nerf'а Campaign token rewards). Ожидаемый pace — 6–9 Contracts runs за полный playthrough.

**Награды (после Phase 2):** 40+15N cash, 3+1N AllTokens. Первый run = 40 cash + 3 tokens to each class (15 total). Contracts — primary token faucet.

**Риски:**
- **Мотивация пустая.** Contracts — это "повтори рандомную миссию ради валюты". Нет ни счётчика серий, ни milestone rewards, ни sense of progress. Grind ощущается как chore.
- **AllTokens одновременно** — хорошо для QoL (не надо выбирать класс), но убивает decision-making. Нет причины выбирать "какой именно класс фармить".
- **Не подключён ни к какой progression line.** Contracts не разблокируют ничего, не ведут к Boss, не двигают сюжет. Это чистый faucet.
- **Auto-leveling врагов (contractsDiff = 0.5+0.05N)** — без cap. После 100 completions difficulty=5.5. Нет ceiling, хотя при текущих HpFromGate=1.0 это проблема только для very long grinders.

**Связь с Meta:** Слабая. Contracts нужны экономически, но дизайнерски disconnected.

---

### 3.3 Sniper

**Роль:** Crossbow-focused mode. One-shot feel (HP=1 всегда). Основной source of CrossbowTokens. Имеет собственный Crossbow damage gate (Sniper Access Gate).

**Мотивация игрока:**
- Уникальный gameplay feel (one-shot Crossbow).
- Необходим для прокачки Crossbow → прохождения Boss gate.
- Лучший cash per run (150+50N vs Contracts 40+15N).

**Награды:** 150+50N cash, 2+1N CrossbowTokens. Sniper — лучший cash mode, и единственный source of CrossbowTokens.

**Риски:**
- **Sniper Access Gate может запутать.** Игрок хочет играть Sniper, но ему говорят "прокачай Crossbow". Но Crossbow tokens даёт... Sniper. Chicken-and-egg проблема смягчена (первые 3 Sniper бесплатные), но UX должен чётко объяснять это.
- **Два gate'а одновременно** (Sniper Access Gate + Boss Crossbow Gate) — оба просят Crossbow. Это дизайнерски правильно (одно направление усилий), но UX должен разделять: "это для Sniper", "это для Boss".
- **Только 7 Sniper миссий** (5 в Zone 1, 2 в Zone 2). В Zone 3 Sniper миссий нет. Ограниченный контент.

**Связь с Meta:** Сильная через Boss Crossbow Gate. Sniper — обязательный stepping stone к Boss.

---

### 3.4 Boss

**Роль:** Zone-ending milestone. Открывается после completion всех Campaign миссий в зоне + Crossbow gate.

**Мотивация игрока:** Strongest reward beat. Ощущение "закрытия" зоны. 300+100N cash + 3+2N AllTokens. Boss — самая награждаемая single mission.

**Награды:** Значительные, но не ломающие. AllTokens (3+2N) — ощутимый, но не crushing token spike. Cash (300+100N) — лучший single-run cash.

**Риски:**
- **Boss gameplay disconnected от meta.** Boss HP = player.Damage × 10. Это значит Boss difficulty определяется текущим оружием игрока, а не meta-прогрессией. Gate проверяет Crossbow, но gameplay использует текущее equipped weapon. Это может быть confusing: "зачем я качал Crossbow, если Boss дерусь Bow?".
- **Zone 3 не имеет Boss.** Это означает, что после Zone 3 Campaign нет финального момента — просто loop reset. Это anticlimax.
- **Boss reward AllTokens включает все классы.** Для одной миссии это ок, но это означает, что Boss rewards не создают direction — они просто "всем понемногу".

**Связь с Meta:** Ключевая. Boss — trigger для zone unlock и loop transition.

---

## 4. Gates и Player Motivation

### 4.1 Campaign Gates

**Текущее состояние (после Phase 2):**

| Gate | Класс | Действие | Тип |
|------|-------|----------|-----|
| g6 | Bow | Bow1 L1 (50 cash, 1 tok) | Teaching gate |
| g8 | Shuriken | Shuriken1 L1 | Teaching gate |
| g9 | Bow | Bow1 L4 | Moderate |
| g10 | Spear | Spear1 L1 | Teaching gate |
| g11 | Shuriken | Shuriken1 L7 | Heavy |
| g12 | Bow | Bow1 L7 | Moderate |
| g13 | Spear | Spear1 L4 | Moderate |
| g14 | Shuriken | Shuriken2 purchase | Forced buy |
| g16 | Bow | Bow2 purchase | Forced buy |
| g18 | Spear | Spear1 max | Heavy |
| g19 | Boomerang | Boom1 L3 | Moderate |
| g21 | Shuriken | Shuriken3 purchase | Forced buy |
| g22 | Spear | Spear2 purchase | Forced buy |

**Оценка:**

- **Zone 1:** 1 gate из 7 миссий (g6). Отлично как onboarding — мягко учит системе.
- **Zone 2:** 7 gates из 8 миссий. **Слишком много.** Особенно проблемны кластеры g8/g9/g10 (три подряд) и g11/g12/g13 (ещё три подряд) + g14 (forced purchase). Игрок Zone 2 — это "стена gates с перерывами на 1 свободную миссию".
- **Zone 3:** 6 gates из 8 миссий. Ожидаемо для late-game, но 4 forced purchases — значимое давление.

**Вывод:** Gates математически корректны, но pacing в Zone 2 создаёт risk of frustration (см. раздел 10).

---

### 4.2 Boss Crossbow Gate

**Текущие значения:**
- Zone 1 Boss: Crossbow dmg ≥ 28 (Xbow1 L4)
- Zone 2 Boss: Crossbow dmg ≥ 37 (Xbow1 L10 или Xbow2 L0)

**Оценка:**

- Хорошо: gate создаёт прямой loop (Campaign done → Boss blocked → play Sniper → upgrade Crossbow → Boss open). Это ясная мотивация.
- Хорошо: 2–3 Sniper runs для Zone 1 Boss, 2–3 для Zone 2 — разумный grind.
- Риск: Zone 1 Boss gate (28) может быть слишком лёгким — игрок может случайно пройти его, если он потратил стартовые CrossbowTokens на upgrade.
- Риск: Zone 2 Boss gate (37) = Xbow1 max. Два пути (max Xbow1 или base Xbow2) — хорошо для player agency.

**Вывод:** Хорошо работает. Создаёт нужное давление без over-gating.

---

### 4.3 Sniper Access Gate

**Текущие значения:** 15 + 3 × sniperCompletedIndex

- Первые 3 Sniper — бесплатно (Xbow1 base=22 > gate 15/18/21).
- Sniper 4: gate=24 → нужен Xbow1 L2.
- Sniper 8: gate=39 → нужен Xbow2.

**Оценка:**

- Хорошо: self-funding loop. Sniper rewards (CrossbowTokens) покрывают стоимость upgrades для следующего gate.
- Хорошо: 3 бесплатных runs — мягкий onboarding в Sniper mode.
- Умеренный риск: два Crossbow gates (Sniper Access + Boss) указывают в одном направлении (upgrade Crossbow), но два отдельных popup'а могут путать.

**Вывод:** Правильно спроектирован. Дополняет Boss gate, не дублирует его.

---

### 4.4 Forced Purchase Gates

**Affected gates:** g14 (Shuriken2), g16 (Bow2), g21 (Shuriken3), g22 (Spear2).

**Оценка:**

- Хорошо: Popup корректно отличает "upgrade your weapon" от "buy a new weapon" (CanUpgradeToPass flag). Это критично для UX.
- Хорошо: WeaponSelectionScreen открывается с нужным классом preselected.
- Риск: Forced purchase — самый frustrating момент, если у игрока нет ресурсов. Shuriken2 стоит 650 cash + 15 tokens. Если у игрока 0 tokens, он должен сделать 3–4 Contracts runs. Это не блок, но grind wall.
- Риск: 4 forced purchases за 23 миссии (~17%) — это ок как %, но они кластеризуются в Zone 2 exit + Zone 3.

**Вывод:** Механически правильно. UX popup'ы работают. Frustration risk — в token availability, не в системе gates.

---

### 4.5 Общая оценка gates

| Вопрос | Ответ |
|--------|-------|
| Понятны ли gates игроку? | Да, через popup. Но нет pre-gate warning. |
| Слишком ли много gates? | Zone 2: да. Zone 1/3: нет. |
| Слишком ли мягкие? | Zone 1 Boss gate: возможно. Остальные: нет. |
| Где нужен tutorial? | Sniper → Crossbow → Boss связка. |

---

## 5. Economy / Rewards

### 5.1 Cash Economy

**Sources:**
| Source | Per run | Notes |
|--------|---------|-------|
| Campaign | 70+7N | Основной source |
| Contracts | 40+15N | Растёт быстрее Campaign |
| Sniper | 150+50N | Лучший cash per run |
| Boss | 300+100N | One-time spike per zone |
| Starting | 100 | — |

**Sinks:**
- Weapon upgrades (50–130 cash per level)
- Weapon purchases (550–1000 cash)
- Energy refill (если есть ad-free модель)

**Оценка:**

- Cash **не является bottleneck** после Zone 1. К моменту Shuriken2 forced purchase (g14) у игрока ~1600 cumulative cash. Shuriken2 стоит 650. У игрока surplus.
- Cash surplus растёт к Zone 3 — к моменту Spear2 purchase (1000 cash) у игрока ~3400 cumulative cash.
- **Вывод:** Cash — вспомогательный ресурс, не primary bottleneck. Primary bottleneck — tokens.

**Предположение:** Если cash не является bottleneck, игрок не чувствует "заработал деньги" как reward. Cash rewards могут ощущаться meaningless к mid-game.

---

### 5.2 Token Economy

**Sources:**
| Source | Per run | Recipient |
|--------|---------|-----------|
| Campaign | 1 flat | Class-specific |
| Contracts | 3+1N | ALL classes |
| Sniper | 2+1N | Crossbow only |
| Boss | 3+2N | ALL classes |
| Starting | 1 per class | — |

**Sinks:**
- Weapon upgrades (1–5 tokens per level, depending on weapon)
- Weapon purchases (12–25 tokens)

**Оценка:**

- Tokens — **primary bottleneck.** Campaign даёт всего 1 token per mission. Для Bow1 L4 нужно 4 tokens. Это 4 Campaign Bow missions (но Bow missions чередуются с другими классами, так что нужно ~8 Campaign missions для 4 Bow tokens). Contracts — обязательный faucet.
- AllTokens в Contracts и Boss — QoL, но **убивает specialization decision.** Игрок не выбирает, какой класс фармить — все получают одинаково.
- **Spear token cost (startTok=1, maxTok=3)** — после Phase 3 fix всё ещё выше среднего (total ~20 tokens to max Spear1 vs ~14.5 for others). Spear upgrade ощущается дороже, что consistent с его ролью high-damage weapon.

**Риски:**
- При 1 token per Campaign mission и ~9 missions per class в full playthrough, Campaign даёт ~9 tokens per class. Этого хватает на 4–5 upgrades, но не на max starter + purchase tier 2. **Contracts обязательны.** Это intentional, но грань между "обязательный grind" и "engaging side activity" тонкая.
- Sniper — единственный source of CrossbowTokens. Если игрок не любит Sniper gameplay, Boss навсегда заблокирован. Нет альтернативного пути к Crossbow tokens.

---

### 5.3 Starting Currencies

| Currency | Amount |
|----------|--------|
| Cash | 100 |
| All tokens | 1 each |

**Оценка:** Минималистично. 1 token per class — хватает ровно на 1 upgrade level. Хорошо: не даёт free ride. Плохо: ощущение "пустого старта", нет initial momentum.

---

### 5.4 Boss Rewards

Boss: 300+100N cash + 3+2N AllTokens. Первый Boss = 300 cash + 3×5 = 15 tokens total. Это ощутимый spike, но не game-breaking (хватает на ~3 upgrade levels per class).

**Оценка:** Правильно откалиброван после Phase 2 nerf'а (было 10+10N AllTokens). Текущие 3+2N — significant but not economy-breaking.

---

### 5.5 Energy System

Energy system существует (EnergyConfig, EnergyService), но в документах не обсуждалась как bottleneck. Предполагаю:
- Energy regen + ad refill = мягкий session limiter.
- Если energy costs per mission type различаются (Campaign > Contracts?), это может влиять на grind efficiency.

**Факт:** Energy system присутствует, но детали её impact на grind pace не проверены в рамках этого аудита. Это **слепое пятно** — energy может быть скрытым bottleneck или non-factor.

---

## 6. Weapon Progression

### 6.1 Bow

**Роль:** Standard all-rounder. Первый weapon class. Присутствует во всех зонах.

**Progression pressure:** Высокое. Teaching gate (g6), несколько upgrade gates, forced Bow2 purchase (g16).

**Мотивация покупать новые weapons:** Чётко работает. Bow1 max=55 < g16 gate=57 → Bow2 обязателен.

**Риск useless:** Нет. Bow — most-used weapon class в Campaign.

**Связь с mission types:** Campaign only. Нет Bow-specific contracts/sniper.

**Оценка:** Отличная progression line. Самый полный arc из всех weapon classes.

---

### 6.2 Crossbow

**Роль:** Sniper-exclusive и Boss gate weapon. Никогда не назначается в Campaign missions.

**Progression pressure:** Двойное — Sniper Access Gate + Boss Crossbow Gate. Оба требуют Crossbow upgrades.

**Мотивация покупать новые weapons:** Xbow1 max=37, Xbow2 base=37. Для Zone 2 Boss хватает Xbow1 max. Xbow2/Xbow3 нужны в loop 1/2.

**Риск useless:** Нет — Boss gate делает Crossbow обязательным.

**Риск too mandatory:** Среднее. Crossbow обязателен для Boss, но Boss rewards (cash + AllTokens) оправдывают investment.

**Связь с mission types:** Sniper + Boss. Это unique dual-purpose weapon class.

**Оценка:** Хорошо спроектирован после Phase 4. Единственный weapon class с собственным dedicated mode (Sniper). Потенциальная проблема: **нет gameplay diversity** — Crossbow используется одинаково в Sniper и Boss, но Sniper gameplay (one-shot) фундаментально отличается от Boss gameplay.

---

### 6.3 Spear

**Роль:** High-damage, slow weapon. Появляется с Zone 2.

**Progression pressure:** После Phase 2 — хорошее. Spear1 base=65 проходит g7 (55), но блокируется на g10 (70). Forced purchase Spear2 на g22 (130).

**Мотивация покупать новые weapons:** Spear2 обязателен для g22. Spear3 — для loop 1/2.

**Риск useless:** Нет (после Phase 2 fix).

**Риск "опоздавшего" оружия:** Средний. Spear появляется только на g7 (Zone 2, mission 1). До этого игрок не знает о Spear. Нет Spear unlock event, tutorial, или intro.

**Связь с mission types:** Campaign only.

**Оценка:** Progression line работает, но **отсутствует onboarding для Spear.** Игрок впервые видит Spear gate на Zone 2 entry без предупреждения.

---

### 6.4 Shuriken

**Роль:** Rapid fire, low per-shot damage. Присутствует во всех зонах.

**Progression pressure:** Высокое. Самый дорогой weapon class по числу forced purchases: Shuriken2 (g14) + Shuriken3 (g21).

**Мотивация покупать новые weapons:** Чёткая. Shuriken1 max=35 < g14 gate=38 → forced Shuriken2. Shuriken2 max=50 < g21 gate=52 → forced Shuriken3.

**Риск too expensive:** Средний. 2 forced purchases + multiple upgrades. Суммарный token cost — самый высокий из всех weapon classes. Shuriken tokens нужны больше, чем любые другие.

**Связь с mission types:** Campaign only.

**Оценка:** Работает как "вечный bottleneck weapon". Это нормально дизайнерски (always something to upgrade), но может frustrate players, которые не любят Shuriken gameplay.

---

### 6.5 Boomerang

**Роль:** High-damage, mid-speed weapon. Появляется только в Zone 3.

**Progression pressure:** Минимальное. Всего 2 Boomerang Campaign missions (g15, g19). g15 — free pass (base=55 > 52.5), g19 — единственный блок (need L3).

**Мотивация покупать новые weapons:** Почти отсутствует. Boomerang1 L3 покрывает g19. Boomerang2 не нужен до loop 1/2.

**Риск useless:** Высокий. Boomerang — **наименее задействованный weapon class.** Всего 2 Campaign missions. Нет Boomerang-specific mode. Нет forced purchase в loop 0. Игрок может забыть о Boomerang.

**Связь с mission types:** Campaign only (2 missions).

**Оценка:** **Weakest weapon class дизайнерски.** Мало контента, мало pressure, позднее появление. Ощущается как afterthought.

---

### 6.6 Weapon Progression Summary

| Class | Missions | Forced Buys (loop 0) | First Block | Onboarding | Design Strength |
|-------|----------|---------------------|-------------|------------|-----------------|
| Bow | 8 | 1 (Bow2) | g6 | Хороший | Сильный |
| Crossbow | 0 Campaign, 7 Sniper | 0 (loop 0) | Sniper 4 | Средний | Сильный (после Phase 4) |
| Spear | 5 | 1 (Spear2) | g10 | Слабый | Средний |
| Shuriken | 8 | 2 (Shuriken2/3) | g8 | Хороший | Сильный, но frustrating |
| Boomerang | 2 | 0 | g19 | Слабый | Слабый |

---

## 7. Pacing по зонам

### Zone 1 — Onboarding

**7 миссий: g0–g6 (Bow/Shuriken)**

| Метрика | Значение |
|---------|----------|
| Gates | 1 из 7 (g6 teaching gate) |
| Новые weapon classes | Нет (Bow + Shuriken — starter weapons) |
| Cash earned | ~637 + 100 start = 737 |
| Tokens earned | 4 Bow, 3 Shuriken |
| Expected grind | 0 Contracts runs |
| Sniper available | Да (free first 3 runs) |
| Boss available | После Campaign complete + Crossbow gate |

**Оценка:** Отлично. Мягкий onboarding. Один teaching gate на последней миссии — идеальный timing. Игрок проходит 6 missions learning gameplay, потом впервые встречает gate и учится upgrade flow. Sniper доступен для exploration, но не обязателен.

**Где стоит вставить hint:** После g6 gate popup — small tutorial: "Contracts дают токены для прокачки всех видов оружия. Sniper даёт CrossbowTokens."

---

### Zone 2 — Pressure

**8 миссий: g7–g14 (Bow/Shuriken/Spear)**

| Метрика | Значение |
|---------|----------|
| Gates | 7 из 8 (!!!) |
| Новые weapon classes | Spear (первое появление) |
| Forced purchases | 1 (Shuriken2 at g14) |
| Expected grind | 2–3 Contracts runs |
| Boss available | После Campaign + Crossbow gate (37) |

**Оценка: ПРОБЛЕМНАЯ.**

Зона 2 — это gate-after-gate. Из 8 миссий только g7 (Spear free pass) не является блоком. Остальные 7 требуют upgrade/purchase. Кластеры:
- **g8/g9/g10** — три подряд (разные классы, разные costs). Acceptable, но ощущается как "вход в Zone 2 стоит 3 upgrades".
- **g11/g12/g13** — ещё три подряд. К этому моменту игрок уже потратил 4–5 upgrades. Теперь нужно ещё 3. Tokens may run out.
- **g14** — forced Shuriken2 purchase. Самый дорогой момент Zone 2 (650 cash + 15 tokens).

**Проблема:** Нет "reward beat" между кластерами. Игрок блокируется, идёт в Contracts, возвращается, проходит 1–2 missions, снова блокируется. Цикл "play → blocked → grind → play → blocked" без pauses.

**Где нужна передышка:** Между g10 и g11 (после Spear teaching gate). Если бы g10 был free pass вместо g7, игрок получил бы 2 free missions подряд (g7 + g10), что бы разбило wall.

**Где стоит вставить tutorial:** На появлении Spear (g7) — "New weapon class unlocked: Spear!" popup или similar.

---

### Zone 3 — Late-Game Pressure

**8 миссий: g15–g22 (все классы включая Boomerang)**

| Метрика | Значение |
|---------|----------|
| Gates | 6 из 8 |
| Новые weapon classes | Boomerang (первое появление) |
| Forced purchases | 3 (Bow2, Shuriken3, Spear2) |
| Expected grind | 3–5 Contracts/Sniper runs |
| Boss | НЕТ (Zone 3 не имеет Boss) |

**Оценка:** Ожидаемо тяжёлая для late-game. 3 forced purchases — серьёзные milestone'ы. Каждый требует significant token investment.

**Проблема:** Zone 3 не имеет Boss. Это означает **нет climax.** Игрок проходит 8 тяжёлых миссий с 3 forced purchases, и... loop reset. Нет final battle, нет big reward, нет cutscene. Anticlimax.

**Где стоит вставить reward beat:** После g22 (последняя миссия) — significant reward или narrative beat перед loop transition.

---

### Zone Pacing Summary

| Zone | Gates/Missions | Feel | Problem |
|------|---------------|------|---------|
| Zone 1 | 1/7 (14%) | Relaxed onboarding | Нет |
| Zone 2 | 7/8 (87%) | Wall of gates | **Block density** |
| Zone 3 | 6/8 (75%) | Heavy but expected | **No Boss = no climax** |

---

## 8. Loop 2/3 Design

### Gate Multipliers

| Loop | GateDamage | CampaignDifficulty | Reward Cash |
|------|------------|-------------------|-------------|
| 0 | 1.0 | 1.0 | 1.0 |
| 1 | 1.25 | 1.3 | 1.1 |
| 2 | 1.55 | 1.5 | 1.2–1.25 |

### Weapon Tier Requirements Per Loop

| Gate | Loop 0 | Loop 1 (×1.25) | Loop 2 (×1.55) |
|------|--------|----------------|----------------|
| g22 Spear | 130 → Spear2 | 162.5 → Spear2 L7 | 201.5 → Spear3 |
| g16 Bow | 57 → Bow2 | 71.25 → Bow2 mid | 88.35 → Bow2 max or Bow3 |
| Boss Zone 2 | 37 → Xbow1 max | 46.25 → Xbow2 L6 | 57.35 → Xbow3 L1 |

### Оценка

**Хорошо:**
- Gate multipliers создают правильное давление. Loop 1 требует Tier 2 weapon upgrades, Loop 2 — Tier 3 purchases. Это organic progression ladder.
- Reward multipliers (1.1x / 1.2x cash) частично компенсируют повышенные costs. Tokens не растут с loops, что сохраняет token pressure.

**Проблемы:**
- **Weapon class rotation (WeaponRequirementService)** меняет weapon assignments в loops: `(baseClass + loop × step) % classCount`. Это значит, что в Loop 1 миссия, которая была Bow, становится Crossbow; Shuriken → Boomerang; и т.д. **Это может сломать calibrated gate values.** Миссия g6, которая была teaching Bow gate, в loop 1 становится другим weapon class с другим gate profile. Гейты могли быть calibrated для одного weapon class, но rotation меняет weapon class без пересчёта gate difficulty.
- **Enemy HP clamp** (MaxShotsToKill) сохраняет shots-to-kill constant across loops. Это правильно — loop difficulty через gates, не через bullet sponge. Но если clamp делает Loop 2 enemies feel identical to Loop 0, loop может ощущаться как "тот же контент с bigger numbers".
- **Нет нового контента в loops.** Те же missions, те же enemies, те же zones. Только numbers change. Replay value зависит целиком от weapon rotation + harder gates.
- **Enemy damage hardcoded to 5** across all loops. Enemies не становятся опаснее — только tankier (через gate damage scaling). Это undermines loop difficulty perception.

**Grind pressure:**
- Loop 1: ~6–9 Contracts runs (same as loop 0, but token needs are higher for Tier 2 max + Tier 3 purchases).
- Loop 2: ~10+ Contracts runs. Tier 3 weapons expensive.

**Bullet sponge risk:** Mitigated by health clamp. Bow max shots = 4, Spear max = 2. Works correctly.

**Оценка:** Механически работает. Мотивационно слабо — "те же миссии, но числа больше" не compelling для mobile retention.

---

## 9. UX / Player Clarity

### Что понятно:

| Момент | UX Coverage |
|--------|-------------|
| Mission locked by damage gate | Popup с текущим/требуемым damage + кнопка "Go to Weapons" |
| Upgrade vs buy new weapon | CanUpgradeToPass hint selection (Phase 3) |
| Boss requires Crossbow | Popup: "Boss requires stronger Crossbow" |
| Sniper requires Crossbow | Popup: "Sniper access requires stronger Crossbow" |
| Weapon selection screen | Opens with correct class preselected |

### Что НЕ понятно (потенциальные проблемы):

| Момент | Проблема |
|--------|----------|
| Зачем играть Contracts | Нет onboarding / tutorial. Игрок может не знать, что Contracts — primary token source |
| Зачем играть Sniper | Popup Boss gate says "play Sniper", но нет tutorial объясняющего Sniper mode |
| Где взять tokens | Popup says "upgrade Crossbow" but doesn't say WHERE to get CrossbowTokens |
| Как работает weapon class rotation в missions | Нигде не объяснено, что Campaign миссии требуют конкретный weapon class |
| Что такое Loop transition | Loop reset happens silently. Нет notification: "Congratulations! New Game+ started" |
| Energy as limiter | Если energy ограничивает play sessions, нет объяснения regen mechanics |
| Contracts → which missions are available | Contracts picks random completed mission. Player doesn't choose |
| Что даёт каждый тип миссии | Нет summary: "Campaign: cash + class tokens; Contracts: cash + all tokens; Sniper: cash + Crossbow tokens" |

### UX Recommendations (не code changes, а design needs):

1. **Mission Type Tooltip:** При первом выборе каждого типа миссии — short description с reward icons.
2. **Token Source Hint:** Gate popup should say WHERE to get tokens: "Play Contracts to earn {weapon class} tokens."
3. **Sniper Onboarding:** When Boss gate blocks for first time — mini-tutorial: "Sniper missions use Crossbow. Complete them to earn CrossbowTokens."
4. **Loop Transition Screen:** After Zone 3 Campaign complete (or Zone 2 Boss, when it's the last Boss) — "Campaign Complete! Starting New Challenge..." screen.
5. **New Weapon Class Intro:** When Spear first appears (g7) and Boomerang (g15) — unlock celebration popup.

---

## 10. Frustration Risks

### F1 — Zone 2 Gate Wall
**Severity: Critical**

7 из 8 миссий Zone 2 — блоки. Кластеры g8/g9/g10 и g11/g12/g13 создают "стену". Игрок может hit 3 blocks подряд, каждый требующий upgrade разного weapon class + Contracts runs для tokens.

**Как проявится:** Игрок заходит в Zone 2, проходит 1 миссию (g7 free), потом блокируется. Идёт в Contracts. Возвращается. Проходит g8. Блокируется на g9. Идёт в Contracts. Возвращается. Проходит g9. Блокируется на g10. Три цикла "play → blocked → grind" за 4 миссии.

**Усиливающий фактор:** Каждый блок требует tokens разного класса (Shuriken, Bow, Spear). Contracts дают AllTokens, но 3+1N per run — нужно несколько runs, чтобы накопить enough для каждого класса.

---

### F2 — Forced Purchase Without Resources
**Severity: Major**

Shuriken2 (g14) стоит 15 ShurikenTokens. Campaign даёт 7 к этому моменту. Gap = 8 tokens. Нужно ~3 Contracts runs. Если игрок не знает про Contracts или не хочет grind, он stuck.

**Как проявится:** Popup: "Buy Shuriken2". Игрок: "У меня 7 tokens, нужно 15." Идёт в shop. Не может купить. Не понимает, откуда взять tokens. Может уйти из игры.

---

### F3 — Crossbow as Mandatory Side-Quest
**Severity: Major**

Crossbow tokens available ONLY from Sniper mode. Если игрок не любит Sniper gameplay (one-shot Crossbow feel), Boss навсегда blocked. Нет альтернативного source of CrossbowTokens.

**Как проявится:** Игрок заканчивает Zone 1 Campaign. Boss blocked. Popup: "play Sniper". Игрок пробует Sniper, не нравится. Boss остаётся blocked. Zone 2 не открывается.

---

### F4 — Непонятный Sniper/Boss Gate Link
**Severity: Major**

Boss popup says "play Sniper missions", но:
- Игрок может не знать, где Sniper в UI.
- Popup text — hardcoded English, не локализован.
- Нет прямой кнопки "Go to Sniper" из Boss popup (есть только "Go to Weapons").

**Как проявится:** Игрок видит popup "play Sniper", закрывает popup, не знает, что делать. Может не найти Sniper tab в mission selection.

---

### F5 — Too Many Currencies
**Severity: Minor**

7 currencies: Cash + 5 weapon tokens + Energy. Каждый weapon class имеет свой token type. Это standard для mobile, но может overwhelm casual players.

**Как проявится:** Игрок видит 5 разных token counters. Не понимает, какой для чего. Путает BowToken и CrossbowToken.

---

### F6 — No Campaign Replay Before Boss
**Severity: Minor**

После завершения всех Campaign missions в зоне, Campaign недоступен (all completed). Если Boss blocked по Crossbow gate, единственные доступные modes — Contracts и Sniper. Игрок не может replay Campaign missions для fun или practice.

**Как проявится:** "Я хочу просто поиграть Campaign, но все missions completed и Boss locked." Единственный вариант — grind Contracts.

---

### F7 — Zone 3 No Boss = No Climax
**Severity: Minor**

Zone 3 — последняя зона, но не имеет Boss. После 8 тяжёлых миссий (3 forced purchases) — просто loop reset. Нет final challenge, нет big reward.

**Как проявится:** "Я прошёл всё, и... ничего?" Anticlimax. Может feel like the game is unfinished.

---

## 11. Boredom Risks

### B1 — Contracts Monotony
**Severity: Major**

Contracts = random replay of completed Campaign missions. No progression, no milestones, no variety. Required ~6–9 times per playthrough. Risk of becoming chore.

**Как проявится:** "Again? Another random mission for 3 tokens?" Especially if Contracts keeps picking the same mission.

---

### B2 — Boomerang Underserved
**Severity: Major**

2 Campaign missions. No dedicated mode. No forced purchases in loop 0. Boomerang feels like "extra weapon" with minimal meta presence.

**Как проявится:** Игрок forgets Boomerang exists. When g19 blocks for Boom upgrade, it feels random: "Why do I need to upgrade a weapon I barely use?"

---

### B3 — Loop 2/3 Same Content
**Severity: Major**

Same missions, same enemies, same zones. Only numbers change. No new mechanics, enemies, or narrative.

**Как проявится:** "I already played these missions. Why am I playing them again?" Mobile retention requires either new content or strong social/competitive hooks. Neither exists.

---

### B4 — Cash Surplus = Meaningless Rewards
**Severity: Minor**

Cash is never the bottleneck. By mid-game, cash rewards feel meaningless because player always has surplus. Victory screen shows cash earned, but it doesn't matter.

**Как проявится:** Player ignores cash counter. Cash reward on victory screen = "whatever". Token reward is the only one that matters.

---

### B5 — Overgrind Breaks Challenge
**Severity: Minor**

Health clamp prevents bullet sponge, but overleveled player one-shots everything. If player does 10+ Contracts runs before advancing, Campaign enemies are trivial.

**Как проявится:** "These enemies are too easy. Why are they dying in 1 shot?" — because player overleveled through grinding.

---

### B6 — Sniper Limited Content
**Severity: Minor**

Only 7 Sniper missions (5 + 2). Если Sniper gameplay repetitive, required 4–6 Sniper runs для Boss gates может feel grindy.

---

## 12. Что уже хорошо

1. **Gate system математически solid.** После Phase 2–4, gates создают правильное pressure для каждого weapon class. Ни один class не проходит все gates бесплатно.

2. **Teaching gate (g6) — отличный design.** Первый блок в конце Zone 1, дешёвый (50 cash, 1 token), учит upgrade flow без frustration.

3. **Sniper → Crossbow → Boss loop.** Самая сильная связка в Meta-системе. Создаёт ясный мотивационный path с self-funding token economy.

4. **CanUpgradeToPass popup distinction.** UX корректно отличает "upgrade" от "buy new weapon". Это предотвращает confusion на forced purchase gates.

5. **Health clamp system.** Решает bullet sponge проблему элегантно. Per-weapon-class shots-to-kill profiles — designer-friendly и maintainable.

6. **Token economy balance.** После Phase 2 nerf'ов: Campaign = 1 flat token, Contracts = primary faucet. Это создаёт correct pressure loop: Campaign drives direction, Contracts funds progression.

7. **Boss HP = player.Damage × 10.** Elegant auto-scaling — Boss always proportional to player power. No need for Boss-specific balance tuning.

8. **Sniper one-shot feel** preserved through formula (HP always rounds to 1). Not a code hack — natural result of SniperHpFromGate=0.01.

9. **Loop multipliers** mechanically correct. Gates scale, rewards partially compensate, weapon tiers provide clear upgrade path per loop.

10. **Popup → WeaponSelectionScreen with class preselected.** Smooth UX flow from block notification to action.

---

## 13. Что стоит улучшить в первую очередь

### P0 — Срочно

**P0-1: Разгрузить Zone 2 gate density.**
Сейчас 7/8 миссий — блоки. Нужно снизить до 4–5 максимум. Варианты:
- Поднять starter weapon damage для Spear (base 65 → 75, чтобы g10=70 стал free pass)
- Или снизить gate growth для одного из weapon classes в Zone 2 range
- Или переставить weapon class assignments, чтобы создать 2 free-pass missions вместо 1

**P0-2: Добавить token source hint в gate popup.**
Popup should say: "Play Contracts to earn tokens" или "Play Sniper to earn CrossbowTokens". Без этого forced purchase gates — стена без выхода для uninformed player.

---

### P1 — Важно

**P1-1: Onboarding для Contracts и Sniper modes.**
При первом unlock (via MetaModeUnlockConfig company level) — popup или tutorial text: что это за режим, какие rewards, зачем играть.

**P1-2: Weapon class unlock events.**
Spear (Zone 2) и Boomerang (Zone 3) появляются без fanfare. Нужен unlock popup: "New weapon class: Spear! Equip it in your loadout."

**P1-3: Loop transition screen.**
После Zone 3 Campaign complete → "Campaign Complete! New Challenge begins." Не silent loop reset.

**P1-4: Альтернативный source of CrossbowTokens.**
Хотя бы маленький: Contracts could give 1 CrossbowToken per run наряду с AllTokens. Или Boss could give extra CrossbowTokens. Это уберёт "Sniper is mandatory" feeling.

---

### P2 — Позже

**P2-1: Contracts progression.**
Добавить milestone rewards: каждые 5 Contracts runs → bonus reward (extra tokens, exclusive skin, etc.). Это превращает grind в progression.

**P2-2: Boomerang content.**
Добавить 1–2 Boomerang missions в Zone 2 (переназначив Bow или Shuriken missions). Это даёт Boomerang раннее появление и stronger progression pressure.

**P2-3: Cash sink.**
Cash не является bottleneck. Добавить cash sinks: cosmetics, energy instant refill, weapon skins. Без этого cash rewards feel meaningless.

**P2-4: Enemy damage unfreezing.**
Damage hardcoded to 5. Это placeholder. Unfreezing damage + archetype multipliers (RangedActive=2.0) добавит difficulty variance.

**P2-5: Contracts difficulty cap.**
contractsDiff unbounded (0.5+0.05N). Добавить cap (e.g., max 3.0) чтобы Contracts не стали as hard as Campaign при very high completions.

---

### P3 — Nice-to-have

**P3-1: Mission rewards preview.**
Показать expected rewards до запуска миссии (cash + tokens). Помогает player decision-making: "стоит ли мне играть Contracts или Sniper?"

**P3-2: Weapon class icon в gate popup.**
Показать иконку weapon class рядом с damage bar. Визуальное подкрепление: "тебе нужен Bow, вот он."

**P3-3: "Recommended weapon" перед Campaign mission.**
Показать required weapon class + текущий damage vs. gate requirement ДО нажатия Play. Превентивный hint вместо реактивного popup.

**P3-4: Zone 3 Boss.**
Добавить Boss в Zone 3 (если контент ready). Это создаст proper climax перед loop transition.

**P3-5: Loop-specific visual theme.**
Loop 1/2 — slight visual change (color tint, "hard mode" badge) чтобы differentiate loops visually, не только numerically.

---

## 14. Вопросы к дизайнеру

1. **Intended grind per zone:** Сколько Contracts runs считается нормой перед переходом в следующую зону? Текущий pace — 2–3 per zone. Это ок, или должно быть больше/меньше?

2. **Zone 2 gate density:** 7/8 миссий — блоки. Это intentional "difficulty spike zone", или Zone 2 должна быть мягче? Сколько gates per zone считается нормой: 3? 5? 7?

3. **Contracts identity:** Contracts — это "необходимый grind" или "fun side mode"? Если second — нужен gameplay variety. Если first — нужны milestone rewards для мотивации.

4. **Cash bottleneck:** Должен ли cash быть bottleneck? Сейчас cash в surplus к mid-game. Если cash должен быть limited, нужно снизить cash rewards или повысить weapon costs.

5. **Sniper обязательность:** Должен ли Sniper быть ОБЯЗАТЕЛЬНЫМ для Boss? Сейчас нет альтернативного source of CrossbowTokens. Если Sniper optional — нужен второй source.

6. **Overgrind tolerance:** Насколько player может overgrind? Если 20 Contracts runs перед Zone 2 делают Campaign trivial — это ок? Или нужен cap на overleveling?

7. **Weapon purchases per loop:** Сколько forced weapon purchases ожидается в Loop 0? Сейчас 4 (Shuriken2, Bow2, Shuriken3, Spear2). Это много? Мало?

8. **Loop 2/3 hardness:** Насколько Loop 2 должен быть жёстче Loop 0? Сейчас gates ×1.55, difficulty ×1.5. Это enough, или нужно stronger escalation? Или softer?

9. **Boomerang future:** Будут ли добавлены Boomerang missions в Zone 1/2, или Boomerang remains late-game-only?

10. **Zone 3 Boss:** Планируется ли Zone 3 Boss, или Zone 3 intentionally без Boss?

11. **Enemy damage:** Когда планируется unfreeze enemy damage (hardcoded 5)? Это placeholder или design decision?

12. **Energy impact:** Насколько energy system ограничивает play sessions? Является ли energy significant factor в grind pacing?

13. **Crossbow в Boss gameplay:** Player upgrades Crossbow для Boss gate, но Boss gameplay использует equipped weapon (Bow/Spear/etc). Boss HP = player.Damage × 10 (не Crossbow damage). Это intentional disconnect, или Crossbow должен somehow participate в Boss fight?

14. **Weapon class rotation в loops:** WeaponRequirementService ротирует classes в Loop 1+. Были ли gate values проверены для rotated assignments? Bow gate на Shuriken mission в Loop 1 может дать unexpected difficulty.

---

## 15. Файлы и данные, которые были просмотрены

### Документы

| Файл | Содержание |
|------|-----------|
| `Assets/Meta/Docs/Meta_System_Audit.md` | Полное описание Meta-системы, архитектура, services, flows |
| `Assets/Meta/Docs/Meta_Balance_Audit.md` | Математический аудит gates, weapons, rewards, scenarios |
| `Assets/Meta/Docs/Meta_Balance_Fix_Plan.md` | План Phase 2: gate profiles, weapon stats, rewards, loop mults |
| `Assets/Meta/Docs/Meta_Balance_After_Fix.md` | Итоги Phase 2: таблицы gates, rewards, scenarios |
| `Assets/Meta/Docs/Meta_Balance_Pacing_Validation.md` | Phase 3: Zone 2 pacing, consecutive blocks, Sniper UX |
| `Assets/Meta/Docs/Meta_Boss_Crossbow_Gate.md` | Phase 4: Boss Crossbow Gate design, formula, token flow |
| `Assets/Meta/Docs/Meta_Balance_After_BossGate.md` | Phase 4 итоги: full gate summary, player flow |
| `Assets/Meta/Docs/Enemy_Stats_Scaling_Audit.md` | Enemy HP/damage formula, archetype multipliers, mission type scaling |
| `Assets/Meta/Docs/Enemy_Health_Clamp_Audit_And_Plan.md` | Health clamp design: shots-to-kill profiles, formula, examples |
| `Assets/Meta/Docs/Enemy_Health_Clamp_Implementation.md` | Health clamp implementation: final formula, before/after tables |

### Код (основные файлы Meta-системы)

| Файл | Роль |
|------|------|
| `BalanceConfig.cs` | Главный баланс SO: gate API, enemy stats, rewards |
| `BalanceConfig.GateModule.cs` | Gate формулы: Campaign, Sniper, Boss profiles |
| `BalanceConfig.EnemyModule.cs` | Enemy HP/damage factors, archetype profiles, health clamp profiles |
| `BalanceConfig.RewardModule.cs` | Reward формулы per mission type |
| `BalanceConfig.LoopModule.cs` | Loop multipliers |
| `BalanceConfig.EconomyPacingModule.cs` | Editor planning (removed in Phase 2) |
| `BalanceConfig.Progressions.cs` | LinearProgression helper |
| `MissionGateService.cs` | Gate checks: Campaign, Sniper, Boss |
| `MissionAvailabilityService.cs` | Availability logic per mission type |
| `MissionStartService.cs` | Mission launch + gate result attachment |
| `MissionRewardService.cs` | Reward calculation |
| `PlayMissionPresenter.cs` | Play button + gate popup triggers |
| `DamageGatePopupController.cs` | Gate popup rendering + hint selection |
| `DamageGatePopupArgs.cs` | Popup data (CanUpgradeToPass, customTitle/hint) |
| `MissionAvailability.cs` | AvailabilityBlockReason enum |
| `MissionGateResult.cs` | Gate result DTO (CanUpgradeToPass) |
| `MainMenuRuntime.cs` | Service composition, menu lifecycle |
| `LoadoutSnapshotWeaponStatService.cs` | Gate reads equipped damage |
| `EnergyConfig.cs` | Energy system configuration |
| `ZoneData.cs` | Zone structure, boss/campaign completion |
| `MissionData.cs` | Single mission definition |
| `MissionSegmentData.cs` | Segment progress tracking |
| `MissionType.cs` | Campaign/Contracts/Sniper/Boss enum |
| `CurrencyType.cs` | Currency enum (Cash + 5 tokens) |

### Assets

| Файл | Данные |
|------|--------|
| `BalanceConfig.asset` | Все числовые значения: gates, rewards, loop mults, enemy params |
| `CurrencyStartingBalanceConfig.asset` | Starting balances: 100 cash, 1 each token |
| `LoadoutSnapshot.asset` | Current equipped weapons + computed stats |
| `Spear WeaponDef 1/2/3.asset` | Spear damage values (65/110, 110/190, 190/300) |
| `Boomerang WeaponDef 1/2.asset` | Boomerang damage values (55/80, 80/130) |
| `ZoneData 3 (Wild West).asset` | Zone 3 structure: 8 Campaign + 2 Sniper, no Boss |
