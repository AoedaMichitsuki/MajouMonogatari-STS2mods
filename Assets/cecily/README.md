# Cecily 美术资源交付规范

该目录用于承载希赛莉角色相关美术资源，代码侧通过 `CecilyArtProvider` 统一读取。

## 目录结构

- `cards/portraits/`：卡牌常规立绘（推荐 1000x760）
- `cards/beta/`：卡牌 Beta 立绘（可选）
- `character/ui/`：角色 UI 图（头像、选角图、地图标记、能量图标）
- `character/scenes/`：角色战斗视觉场景
- `powers/small/`：Power 小图标
- `powers/big/`：Power 大图标

## 文件命名

按模型 EntryId 最后一段命名（小写，下划线）：

- `majoumonogatari-sts2mods.cecily.strike` -> `strike.png`
- `majoumonogatari-sts2mods.cecily.wind_bullet` -> `wind_bullet.png`

## 第一版必须资源（basic + breeze）

- `character/scenes/cecily_visual.tscn`
- `character/ui/icon_texture.png`
- `character/ui/char_select_icon.png`
- `character/ui/char_select_locked.png`
- `character/ui/map_marker.png`
- `character/ui/big_energy.png`
- `character/ui/text_energy.png`
- `cards/portraits/strike.png`
- `cards/portraits/defend.png`
- `cards/portraits/wind_bullet.png`
- `cards/portraits/spring_tuft.png`
- `powers/small/breeze_power.png`
- `powers/big/breeze_power.png`

## 占位策略

若目标资源不存在，当前版本会自动回退到 `res://icon.svg`，保证开发期不因美术缺失中断玩法验证。
