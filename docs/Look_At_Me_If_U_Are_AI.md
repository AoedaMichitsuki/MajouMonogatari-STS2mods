# Cecily 角色开发总规格（Greenfield v2）

## 1. 任务
从零搭建一个 StS2 C# Mod 工程，并完整实现角色 **希赛莉（Cecily）**。  
重点：先搭可复用机制框架，再填角色内容；其中 **咏唱** 是跨角色公共词条。

---

## 2. 总体实现原则
- 公共机制与角色内容分层。
- 手牌顺序改动必须唯一入口。
- 跨回合效果必须由集中调度器处理。
- Hook/Patch 统一注册，不允许散落。
- 流场/连接可触发时，卡牌外圈黄光提示。

---

## 3. 必做架构（目录）

```text
StS2.CecilyMod/
├─ src/
│  ├─ ModEntry/
│  │  ├─ MainFile.cs
│  │  ├─ ModBootstrap.cs
│  │  └─ ModConstants.cs
│  ├─ Shared/
│  │  ├─ Core/
│  │  │  ├─ CombatContext.cs
│  │  │  ├─ SpireFields/
│  │  │  └─ Services/
│  │  ├─ Keywords/
│  │  │  ├─ Chant/
│  │  │  │  ├─ ChantState.cs
│  │  │  │  ├─ ChantScheduler.cs
│  │  │  │  ├─ ChantResolver.cs
│  │  │  │  └─ ChantHooks.cs
│  │  │  ├─ Flow/
│  │  │  │  ├─ FlowEvaluator.cs
│  │  │  │  └─ FlowGlowUpdater.cs
│  │  │  └─ Link/
│  │  │     ├─ LinkState.cs
│  │  │     └─ LinkEvaluator.cs
│  │  ├─ Hand/
│  │  │  ├─ HandOrderService.cs
│  │  │  ├─ ShiftCommand.cs
│  │  │  └─ HandSnapshot.cs
│  │  ├─ Resources/
│  │  │  └─ Breeze/
│  │  │     ├─ BreezeService.cs
│  │  │     └─ BreezeValidation.cs
│  │  ├─ UI/
│  │  │  ├─ CombatWidgets/
│  │  │  └─ CardGlow/
│  │  ├─ Hooks/
│  │  │  ├─ HookRegistry.cs
│  │  │  └─ HarmonyPatches/
│  │  └─ Utils/
│  │     ├─ StringExtensions.cs
│  │     └─ CardExtensions.cs
│  └─ Characters/
│     └─ Cecily/
│        ├─ CecilyCharacter.cs
│        ├─ CecilyCardPool.cs
│        ├─ CecilyRelicPool.cs
│        ├─ Cards/
│        │  ├─ Basic/
│        │  ├─ Common/
│        │  ├─ Rare/
│        │  └─ Special/
│        ├─ Relics/
│        ├─ Powers/
│        ├─ Keywords/
│        │  ├─ CecilyFlowRules.cs
│        │  └─ CecilyLinkRules.cs
│        └─ Registries/
│           ├─ CecilyCardRegistry.cs
│           └─ CecilyRelicRegistry.cs
```

---

## 4. 角色基础信息
- HP：70/70
- 初始金币：99
- 角色背景：拥有掌握微小气流之天生魔法的半魔女，因为友人的实验失误被传送到高塔。

---

## 5. 资源与词条规则（按最新设定）

### 5.1 微风（Breeze）
- 最低只能到 0。
- 打出需要消耗微风的卡牌时，若微风不足则该牌不可打出。

### 5.2 流场（Flow）
- 流场（左/右）（Flow(Left/Right)）：打出时卡在手牌最左/最右触发。
- 流场（空 / Vain）：打出时该牌是手牌唯一卡牌触发。
- 若该牌不是从手牌打出，不触发流场。
- 可触发时外圈黄光。
- 超广域透镜生效时，左右唯一相邻为状态/诅咒也视为满足。

### 5.3 连接（Connection）
- 与本回合打出的上一张牌类型相同触发。
- 可触发时外圈黄光。
- 远距共鸣存在时改为“本回合打出过任意同类型”。

### 5.4 咏唱（Chant，公共词条）
- 打出后：短暂停留屏幕中央触发特效，再回到手牌最右并锁定。
- 若回合结束时仍在手中：下回合开始、抽牌前触发其效果，再进入弃牌堆（或消耗堆）。
- 咏唱牌若带流场：流场成功判定基于打出回合结束自动弃牌前的手牌状态。
- 咏唱判定顺序先于自动弃牌，后于其他回合内事件。
- 多张咏唱按左到右结算。
- 若咏唱牌本回合被丢弃/消耗且未在回合结束时留手，则不触发。
- 咏唱是跨角色公共机制。

### 5.5 锁定（Locked）
- 本回合不可打出。
- 本回合获得保留。

### 5.6 平移（Shift）
- 平移（左/右）X（Shitf(Left/Right)）：手牌中移动 X 个位置。
- 触边继续平移则无事发生。
- 若“理解”已生效，则最左最右视为相邻，可环形平移。

### 5.7 接续（End-to-End）
- 本回合若允许接续，则拥有流场（左）的牌左侧全部为流场（左）牌时也视为满足；流场（右）同理。

---

## 6. 初始遗物与专属遗物（按最新设定）
- 天生魔法（风）：每当你获得 1 次格挡，获得 1 微风。
- 若草色风铃：每当你获得 1 次格挡，获得 2 微风；战斗开始时获得 3 微风。
- 风量计：每当你通过平移移动一张卡牌，获得 1 微风。
- 法力布料：每当你打出一张带“咏唱”的卡牌，获得 3 格挡。
- 完美的四面锥：每场战斗限一次，回合开始时若本场累计受伤 >=4 次，则回合结束后获得额外回合。
- 超广域透镜：触发流场（左/右）时，若相邻唯一一张是状态/诅咒，仍视为可触发。

---

## 7. 卡池清单
以 Character Design 为准
---

## 8. 关键接口与实现要求

### 8.1 必用接口（核心）
- `Hook.AfterCardPlayed`
- `Hook.BeforeCardPlayed`
- `Hook.ShouldPlay`
- `Hook.BeforeHandDraw`
- `Hook.ShouldDraw`
- `Hook.AfterCardDrawn`
- `Hook.AfterCardDiscarded`
- `Hook.AfterCardExhausted`
- `Hook.AfterCardChangedPiles`
- `Hook.BeforeTurnEnd`
- `Hook.AfterTurnEnd`
- `Hook.BeforeSideTurnStart`
- `Hook.AfterSideTurnStart`
- `Hook.AfterEnergyReset`
- `Hook.ShouldPlayerResetEnergy`
- `Hook.AfterBlockGained`
- `Hook.ModifyCardPlayResultPileTypeAndPosition`

### 8.2 关键对象
- `Player.PlayerCombatState`
- `PlayerCombatState.Hand/DrawPile/DiscardPile/ExhaustPile/PlayPile`
- `CardPile.Cards`
- `CardPile.AddInternal/RemoveInternal`
- `CardCmd.Discard/Exhaust/AutoPlay/ApplyKeyword`
- `CardModel.CanPlay/GiveSingleTurnRetain/ShouldGlowGold`
- `NCard.UpdateVisuals`
- `NPlayerHand.ForceRefreshCardIndices`

### 8.3 高风险必须模块化
- `ChantScheduler + ChantResolver`（集中处理咏唱全时序）
- `HandOrderService`（唯一手牌顺序写入口）
- `FlowEvaluator + LinkEvaluator`（判定与卡牌效果解耦）
- `HookRegistry`（统一注册）
- Harmony patch：手牌上限、咏唱中心动画、能量禁令补强

### 8.4 教程、模版、日志
- 所有实体化方式均参考STS2Project/ModTemplate-StS2/content和https://glitchedreme.github.io/SlayTheSpire2ModdingTutorials/的教程
- 实践项目模版可以在/Users/luozikun/Unity Project/Manbo/SlayTheSpire2-Modding-Simple-Tutorial找到
- 日志在~/Library/Application Support/SlayTheSpire2/log

### 8.5 模型归位与注册方式（强约束）

#### 8.5.1 本次致命问题复盘（必须记住）
- 已有多个ai修改的版本发生启动 Fatal：`DuplicateModelException`（重复 canonical model）。
- 根因：同一个模型（如 `CecilyCardPool`）走了两条注册路径：
  1) `ModInitializer` 阶段手动 `ModelDb.Inject(...)`；  
  2) 框架默认模型初始化流程再次创建。
- 结论：**不要在初始化入口手动批量 Inject 常规模型**（角色/卡池/卡牌/遗物/能力）。

#### 8.5.2 以后“模型补上”应放在哪里
- 角色本体：`src/Characters/Cecily/CecilyCharacter.cs`
- 池模型：`src/Characters/Cecily/CecilyCardPool.cs`、`CecilyRelicPool.cs`、`CecilyPotionPool.cs`
- 卡牌：`src/Characters/Cecily/Cards/{Basic|Common|Rare|Special}/`
- 遗物：`src/Characters/Cecily/Relics/`
- 能力/资源 Power：`src/Characters/Cecily/Powers/`
- 公共机制模型（跨角色）：`src/Shared/...`

#### 8.5.3 以后应通过什么方式注册
- 使用声明式方式注册，不在 `ModBootstrap` 里手动 `Inject`：
  - 每个模型使用 `[CustomID(...)]`。
  - 角色卡牌/遗物基类使用 `[Pool(typeof(...Pool))]`。
  - 在角色模型中通过 `ModelDb.CardPool<T>() / ModelDb.RelicPool<T>() / ModelDb.PotionPool<T>()` 关联池。
  - 起始卡组/起始遗物通过 `ModelDb.Card<T>() / ModelDb.Relic<T>()` 引用。
- `ModInitializer` 只做：`HookRegistry`、Harmony patch、配置与日志初始化；不做常规模型注入。

#### 8.5.4 例外情况
- 仅当某对象**确认不在框架自动注册链路**中时，才允许手动 `ModelDb.Inject`。
- 若必须手动 Inject，必须保证全局唯一来源，并在代码注释中写明“为何不能走声明式注册”。

---

## 9. 验收清单
- 微风不足时，所有“消耗微风牌”不可打出。
- 流场/连接提示黄光准确，且与手牌重排同步。
- 咏唱满足“中心动画->回手最右锁定->下回合抽牌前触发->弃/耗”的完整时序。
- 多咏唱同回合按左到右结算。
- “理解”开启后最左最右环形相邻逻辑正确。
- “终章”移动增伤与离手重置正确。
- 完美四面锥额外回合每战限一次。

---

## 10. 输出要求（给 AI）
你必须输出：
1. 完整文件树。
2. 可编译代码（非伪代码）。
3. Hook/Patch 映射表。
4. 咏唱与手牌顺序时序说明。
