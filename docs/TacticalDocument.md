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
