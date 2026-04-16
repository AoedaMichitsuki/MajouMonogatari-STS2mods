 # Cecily Flow 机制实现说明

## 目标
- Flow(Left/Right/Vain) 判定必须基于“打出当下离开手牌前”的真实手牌位置。
- 避免 `BeforeCardPlayed` 时卡已进入 `Play` 牌堆导致取不到手牌索引。
- 避免同一卡牌在不同阶段对象引用切换导致快照丢失。

## 核心方案

### 1) 快照结构
- 文件：`src/Shared/Keywords/Flow/FlowRuntimeState.cs`
- `FlowSnapshot`：
  - `IsLeftmost`
  - `IsRightmost`
  - `IsOnlyCard`
- 运行时缓存：
  - `SnapshotByCard`（按 `CardModel` 引用）
  - `SnapshotByCardPlay`（按 `CardPlay` 引用）
  - `SnapshotByUniqueId`（按反射读取到的 `CardModel.UniqueId`，用于跨实例兜底）

### 2) 快照采集时机
- **手牌移除前缀采集（关键）**
  - Hook：`CardPile.RemoveInternal` prefix
  - 当 `pile.Type == Hand` 时，调用 `FlowRuntimeState.CaptureFromPile(pile, card)`。
  - 这一步能拿到卡离开手牌前的精确 index。
- **常规刷新**
  - Hook：`ShouldPlay` postfix / `AfterCardPlayed` postfix
  - 调用 `FlowRuntimeState.RefreshFromHand(combatState)`，重建当前手牌快照。
- **战斗开始清理**
  - Hook：`BeforeCombatStart` postfix
  - 调用 `FlowRuntimeState.ClearAll()`。

### 3) 读取策略（OnPlay）
- 卡牌在 `OnPlay` 调用：
  - `FlowRuntimeState.TryResolve(cardPlay?.Card ?? this, cardPlay, out snapshot)`
- `TryResolve` 解析顺序：
  1. 先查 `SnapshotByCardPlay`（本次打牌专用快照）
  2. 再查 `SnapshotByCard` / `SnapshotByUniqueId`
  3. 再尝试 `CaptureFromHand(card)`（仅作兜底）
- `CaptureFromHand(CardPlay)` 会先尝试复用已有卡级快照，再绑定到 `CardPlay`，避免在 `BeforeCardPlayed` 阶段因牌已在 `Play` 堆而丢失已采集快照。

### 4) 等价匹配规则（仅兜底）
- 在 `CaptureFromHand(CardModel)` 中，若手牌里找不到同引用对象，则依次尝试：
  1. `DeckVersion` 引用匹配
  2. `CloneOf` 引用匹配
  3. `Owner + Id.Entry` 唯一匹配（若多张同 ID 则放弃，避免误判）

## 当前使用 Flow 的卡牌
- `CecilyWindBulletCard`：只在 `IsLeftmost` 时触发 Flow 增益。
- `CecilySpringTuftCard`：
  - `IsLeftmost` -> 抽 1
  - `IsRightmost` -> 弃 1
- `CecilyBlossomWayCard`：
  - `IsLeftmost` -> 抽 1 且本回合 0 费
  - `IsRightmost` -> 选 1 张手牌 Exhaust

## 已知边界
- `CardModel.UniqueId` 在当前版本可通过反射读取；若未来引擎字段变化，流程仍可依赖 `CardPlay` 与 `CardPile.RemoveInternal(Hand)` 的主链路工作。
- 仍保留 `Flow snapshot unavailable` 告警，便于后续追踪极端时序问题。

---

# Cecily 生物动画入口实现说明

## 1) 目标
- 统一接管角色实体动画触发入口，避免每张卡手写节点查找和动画切换。
- 兼容两类视觉实现：`AnimationPlayer` / `AnimatedSprite2D`。
- 支持“引擎 trigger 自动路由”与“代码手动调用”两种模式。

## 2) 接入位置
- 文件：`src/Shared/Hooks/HookRegistry.cs`
- 入口 patch：`NCreature.SetAnimationTrigger` postfix
- patch 方法：`CreatureSetAnimationTriggerPostfix(NCreature __instance, string trigger)`
- 路由执行：`CreatureAnimationRuntime.TryHandleEngineTrigger(__instance, trigger)`

说明：游戏在攻击/受击/施法/死亡等行为时会调用 `SetAnimationTrigger`，本 mod 在这个统一入口把 trigger 映射到 Cecily 自定义动画。

### 2.1 当前项目的实际触发链路（重要）
- 当前 Cecily 攻击卡（如 `CecilyStrikeCard`、`CecilyWindBulletCard`）在 `OnPlay` 使用的是 `CreatureCmd.Damage(...)`。
- `CreatureCmd.Damage(...)` 这条链路稳定触发的是受击方 `Hit`，不保证给攻击者发 `Attack` trigger。
- 因此“打出攻击卡但未观察到玩家 `Attack` trigger”是符合当前调用链行为的。
- 若需要更接近原版“攻击者出手动画”的行为，优先走 `BaseLib.Utils.CommonActions.CardAttack(...)`（内部走 `AttackCommand` 链路）。

## 3) 运行时模块
- 文件：`src/Shared/Animation/CreatureAnimationRuntime.cs`
- 常量：
  - `CreatureAnimationRuntime.AnimationNames.Idle`
  - `CreatureAnimationRuntime.AnimationNames.Hit`
  - `CreatureAnimationRuntime.AnimationNames.Attack`
  - `CreatureAnimationRuntime.AnimationNames.Cast`
  - `CreatureAnimationRuntime.AnimationNames.Dead`
- 内置 trigger 路由：
  - `Hit -> Hit`
  - `Attack -> Attack`
  - `Cast -> Cast`（若不存在，回退 `Attack`）
  - `Dead -> Dead`
  - 其他 trigger -> `Idle`

## 4) 可配置路由（新增）

运行时支持三层配置：

1. 全局 trigger 路由（对所有卡生效）  
2. 卡牌 trigger 路由（仅当前打出的该 cardId 生效）  
3. 卡牌播放序列（打出该 cardId 时按步骤主动播放）

### 4.1 注册 API
- `RegisterTriggerRoute(trigger, candidates, fromEnd, returnToIdle)`
- `RegisterCardTriggerRoute(cardEntryId, trigger, candidates, fromEnd, returnToIdle)`
- `RegisterCardPlaySequence(cardEntryId, steps)`
- `SuppressTrigger(trigger)` / `UnsuppressTrigger(trigger)`
- `SuppressCardTrigger(cardEntryId, trigger)` / `UnsuppressCardTrigger(cardEntryId, trigger)`
- `ClearTriggerRoute(trigger)`
- `ClearCardAnimationConfig(cardEntryId)`
- `ClearAllCustomConfigs()`

### 4.2 运行优先级
当引擎触发 `SetAnimationTrigger(trigger)` 时，按以下顺序选路由：
1. 当前打出卡牌的 `cardEntryId + trigger` suppress（命中即空操作）  
2. 全局 `trigger` suppress（命中即空操作）  
3. 当前打出卡牌的 `cardEntryId + trigger` 路由  
4. 全局 `trigger` 路由  
5. 内置路由（Hit/Attack/Cast/Dead）  
6. 默认 `Idle`

### 4.3 当前打牌上下文
- `Hook.BeforeCardPlayed` 调 `CreatureAnimationRuntime.BeginCardPlay(cardPlay)` 绑定“当前卡牌上下文”。
- `Hook.AfterCardPlayed` 调 `CreatureAnimationRuntime.EndCardPlay(cardPlay)` 清理上下文。
- 因此“卡牌 trigger 路由”只会在该次打牌对应的 trigger 窗口内生效。

## 5) 执行策略
1. 先做角色过滤：仅处理玩家生物，且满足以下任一条件：
   - `ModelId` 以 `CecilyIds.Character` 结尾；
   - 当前打牌上下文里的 `cardEntryId` 以 `CecilyIds.Character + "_"` 开头（兜底识别）。
2. 在 `NCreature` 子树中优先找 `AnimationPlayer`，否则找 `AnimatedSprite2D`。
3. 按“候选动画名 -> Idle -> 第一个可用动画”解析实际可播动画。
4. `AnimationPlayer`：`Play(...)` 后 `Queue(idle)` 回到待机。
5. `AnimatedSprite2D`：播放非循环动画时，监听 `animation_finished` 一次性回到 `Idle`。

### 5.1 调试日志（排查专用）
运行时会输出：
- `Animation trigger observed ...`：确认引擎 trigger 是否进入了统一入口、是否命中过滤。
- `Animation trigger handled ...` / `Animation trigger suppressed ...`：确认最终路由与是否执行。

## 6) 手动调用规范（给卡牌/遗物/Power）

### 6.1 直接按拥有者生物播放
```csharp
CreatureAnimationRuntime.TryPlayCustomForCardOwner(
    this,
    CreatureAnimationRuntime.AnimationNames.Attack,
    fromEnd: false,
    returnToIdle: true,
    forceRestart: true);
```

### 6.2 按生物对象播放
```csharp
CreatureAnimationRuntime.TryPlayCustom(
    ownerCreature,
    CreatureAnimationRuntime.AnimationNames.Cast,
    fromEnd: false,
    returnToIdle: true,
    forceRestart: true);
```

参数约定：
- `fromEnd`：是否从动画末尾反向播（用于某些回放/倒放需求）。
- `returnToIdle`：播放完成后是否自动回待机。
- `forceRestart`：同名动画正在播放时是否强制重播。

### 6.3 卡牌里“具体怎么插”（可直接参考）
下面示例演示：先手动播一次 `Attack`，再结算伤害，最后再播一次自定义收招动画。

```csharp
protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
{
    if (cardPlay?.Target == null)
    {
        return;
    }

    // 1) 出手前摇（按卡牌拥有者生物播放，也就是主要的攻击动画）
    CreatureAnimationRuntime.TryPlayCustomForCardOwner(
        this,
        CreatureAnimationRuntime.AnimationNames.Attack,
        fromEnd: false,
        returnToIdle: false,
        forceRestart: true);

    // 2) 结算伤害
    await CreatureCmd.Damage(choiceContext, cardPlay.Target, DynamicVars.Damage, Owner?.Creature, this);

    // 3) 收招（用自定义动画名，播完回 Idle，当然也可以没有）
    CreatureAnimationRuntime.TryPlayCustomForCardOwner(
        this,
        "AttackRecover",
        fromEnd: false,
        returnToIdle: true,
        forceRestart: true);
}
```

插入建议：
- 想“先动再打”：把调用放在伤害命令前。
- 想“打完再演出”：把调用放在伤害命令后。
- 需要多段演出：在 `OnPlay` 内多次调用，或改用 `RegisterCardPlaySequence(...)`。

## 7) 配置示例（推荐放到 `ModBootstrap.Initialize`）

### 7.1 改写全局 Cast 动画
```csharp
CreatureAnimationRuntime.RegisterTriggerRoute(
    trigger: "Cast",
    candidates: ["CastSpell", "Cast", "Attack"],
    fromEnd: false,
    returnToIdle: true);
```

### 7.2 给某张卡加专属 Attack 路由
```csharp
CreatureAnimationRuntime.RegisterCardTriggerRoute(
    cardEntryId: CecilyIds.WindBulletCard,
    trigger: "Attack",
    candidates: ["WindBulletAttack", "Attack"],
    fromEnd: false,
    returnToIdle: true);
```

### 7.3 给某张卡屏蔽引擎 Attack trigger（空操作）
```csharp
CreatureAnimationRuntime.SuppressCardTrigger(
    cardEntryId: CecilyIds.WindBulletCard,
    trigger: "Attack");
```

### 7.4 给某张卡配置多段动画序列
```csharp
CreatureAnimationRuntime.RegisterCardPlaySequence(
    CecilyIds.BlossomWayCard,
    [
        new CreatureAnimationRuntime.AnimationStep("CastStart", delayMs: 0),
        new CreatureAnimationRuntime.AnimationStep("CastLoop", delayMs: 120, returnToIdle: false),
        new CreatureAnimationRuntime.AnimationStep("CastEnd", delayMs: 260, returnToIdle: true)
    ]);
```

## 8) 资源路径与兜底
- 文件：`src/Shared/Art/CecilyArtProvider.cs`
- `CharacterVisualScenePath` 现在按顺序尝试：
  1. `res://Assets/cecily/character/scenes/cecily_visual.tscn`
  2. `res://Assets/cecily/scenes/cecily_visual.tscn`

这样可兼容旧目录结构，避免视觉场景路径不一致导致动画入口生效但找不到节点。

## 9) 建议调用时机
- 默认不用手动调用：普通攻击/受击/施法可依赖引擎 trigger 自动路由。
- 手动调用用于“额外演出”：
  - 卡牌 `OnPlay` 里插入特殊动作段（例如连击第 2 段）。
  - 遗物/Power 在特殊事件触发时追加一次演出。

## 10) BaseLib CommonActions 快捷方法

`CommonActions` 位于 `BaseLib.Utils.CommonActions`，是对常见卡牌行为的命令封装。

### 10.1 攻击/格挡
- `CardAttack(CardModel card, CardPlay play, int hitCount = 1, string vfx = null, string sfx = null, string tmpSfx = null)`
- `CardAttack(CardModel card, Creature target, int hitCount = 1, string vfx = null, string sfx = null, string tmpSfx = null)`
- `CardAttack(CardModel card, Creature target, decimal damage, int hitCount = 1, string vfx = null, string sfx = null, string tmpSfx = null)`
- `CardAttack(CardModel card, Creature target, CalculatedDamageVar calculatedDamage, int hitCount = 1, string vfx = null, string sfx = null, string tmpSfx = null)`
- `CardBlock(CardModel card, CardPlay play)`
- `CardBlock(CardModel card, BlockVar blockVar, CardPlay play)`

### 10.2 抽牌/选牌
- `Draw(CardModel card, PlayerChoiceContext context)`
- `SelectCards(CardModel card, LocString prompt, PlayerChoiceContext context, PileType pileType, int count)`
- `SelectCards(CardModel card, LocString prompt, PlayerChoiceContext context, PileType pileType, int minCount, int maxCount)`
- `SelectSingleCard(CardModel card, LocString prompt, PlayerChoiceContext context, PileType pileType)`

### 10.3 Power 施加
- `Apply<TPower>(Creature target, DynamicVarSource dynVarSource, bool silent = false)`
- `Apply<TPower>(IEnumerable<Creature> targets, DynamicVarSource dynVarSource, bool silent = false)`
- `Apply<TPower>(Creature target, CardModel card, bool silent = false)`
- `Apply<TPower>(Creature target, CardModel card, decimal amount, bool silent = false)`
- `ApplySelf<TPower>(CardModel card, bool silent = false)`
- `ApplySelf<TPower>(CardModel card, decimal amount, bool silent = false)`

### 10.4 在本项目的建议
- 需要攻击者出手动画时：优先用 `CommonActions.CardAttack(...)`。
- 只做“纯伤害结算”且不依赖攻击者 trigger 时：可继续用 `CreatureCmd.Damage(...)`。
- 若卡牌演出强依赖自定义触发时序：在 `OnPlay` 内显式手动调用 `CreatureAnimationRuntime.TryPlayCustom...`，不要仅依赖引擎 trigger。
