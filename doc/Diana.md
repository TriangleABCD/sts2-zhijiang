# 枝江 mod - 角色 - 嘉然（Diana）

> 当前状态：**基础骨架**。只有打击/防御两张牌、初始遗物「嘉心糖」（效果待定）、心之壁第二资源。
> 玩法机制（扑克牌点数/花色相关）尚未定稿，暂未实现。美术暂时复用贝拉资源的复制件。

## 一、总体介绍

### 1.1 角色定位与颜色

- 英文名：Diana
- 初始数值：**生命 92**、金币 99、性别 Feminine
- 介绍词（口号单独一行，用 \n 断行）：
  - zhs「这里是A-SOUL的嘉然Diana。」/「关注嘉然，顿顿解馋！」
  - eng「This is Diana of A-SOUL.」/「Follow Diana, feast forever!」
- **主题色 / 应援色：#E799B0**（嘉然粉；`DianaCharacter.ThemeColor` 与 `SupportColor` 同值，贝拉是两个独立颜色）
- 卡池骨架：常规角色（暂无第二套机制）
- 美术：`Zhijiang/images/characters/Diana/*`（战斗人物模型仍缺，回退占位角色）

颜色相关的一切（照贝拉的做法逐项配置）：

| 用途 | 值 | 位置 |
|---|---|---|
| 卡面/卡牌名字色、遗物与药水实验室描边 | `ThemeColor` #E799B0 | `DianaCharacter.ThemeColor` |
| 地图画线、卡牌拖尾（缎带/剪影/大火花/小火花）、洗牌拖尾 | `SupportColor` #E799B0 | `DianaCharacter.SupportColor` + `Vfx.TrailStyle` |
| 卡池卡框 | `CreateHsvShaderMaterial(0.951, 0.338, 0.906)`（#E799B0 的 HSV） | `DianaCardPool.PoolFrameMaterial` |
| 卡组条目卡色 | `ThemeColor` | `DianaCardPool.DeckEntryCardColor` |
| 能量数字描边 / 卡池能量描边 | #3D1726（`0.24, 0.09, 0.15`） | `DianaCharacter.EnergyLabelOutlineColor`、`DianaCardPool.EnergyOutlineColor` |
| 能量计数器场景文字描边 | `Color(0.24, 0.09, 0.15, 1)` | `scenes/characters/Diana/Diana_energy_counter.tscn` |
| 色彩哲学家事件卡池选项 | 「应援樱粉 / Support Sakura」 | `DianaCardPool` 实现 `IModColorfulPhilosophersCardPool` + `events.json` |
| 洗牌拖尾补丁 | 按角色取 `SupportColor` | `ZhijiangCode/Patches/CharacterShuffleTrailStylePatch.cs` |

> 能量图标 PNG（`Diana_energy_big/text.png`）目前仍是贝拉珊瑚粉的复制件，等专属美术到位再换成嘉然粉。

### 1.2 扑克牌面（已实现基础件）

每张嘉然卡牌带一个**花色关键词** + 一个**点数关键词**（大小王只有点数），卡面把两者合成一行显示在描述上方。

#### 关键词清单（19 个，id 前缀 `ZHIJIANG_KEYWORD_POKER_`）

| 类别 | 关键词 stem | 卡面 | 本地化标题（zhs / eng） |
|---|---|---|---|
| 花色 ×4 | `poker_spade` / `poker_heart` / `poker_diamond` / `poker_club` | ♠ ♥ ♦ ♣ | 符号本身（两种语言相同） |
| 点数 ×13 | `poker_a` … `poker_10` / `poker_j` / `poker_q` / `poker_k` | A 2 … 10 J Q K | 符号本身 |
| 大小王 ×2 | `poker_big_joker` / `poker_small_joker` | — | 大王 / Red Joker、小王 / Black Joker |

> 19 个关键词全部 `IncludeInCardHoverTip = false`：扑克牌面没有需要解释的规则，
> 卡牌 hover 提示里不会出现 ♠ / 3 这两条（`.description` 本地化仍保留，作为备用文案）。

#### 为什么不用 RitsuLib 的关键词注入显示

RitsuLib 的 `ModKeywordCardDescriptionInjector` 是"一个关键词一行 + 句尾补 `card_keywords.PERIOD`"，
用它显示会变成两行 `♠.` / `3.`，做不出牌面 `♠3`。所以：

- 19 个关键词的 `CardDescriptionPlacement` 保持默认 **None**（只做数据 + hover tip，不参与注入）
- 卡面文字由 `ZhijiangCode/Patches/DianaPokerCardTextPatch.cs` 接管：Postfix 原版
  `CardModel.GetDescriptionForPile(PileType, DescriptionPreviewType, Creature)`（与 RitsuLib 注入器同一个方法），
  把花色+点数合成一行 `[gold]♠3[/gold]` 插到描述上方；红桃/方块/大王用红色 `[color=#ff5555]`
- 注册于 `Entry.Initialize()`，**非 critical**：PatchAll 失败只打 WARN，不影响玩法

#### 字体结论（实测 cmap）

- zhs 字体（NotoSansMonoCJKsc / SourceHanSerifSC）**含 ♠♥♦♣**，不含 🃏（U+1F0CF）
- 游戏没有 emoji 字体，所以**大小王不能用 🃏（会显示空白/豆腐块）**，改用文字"大王/小王"
- ⚠️ 英文卡文用 `spectral_bold.ttf`，实测**只有 ♥，没有 ♠♦♣** —— 英文环境下花色符号可能显示不出来，
  待游戏内验证；如果确实缺字，把 `DianaPokerKeywords.BuildLabel` 改成 `[img=18x18]...[/img]` 图标即可（只需改一处）

#### 已分配牌面

| 卡牌 | 牌面 |
|---|---|
| 打击 DianaStrike | **♠3**（黑桃 3） |
| 防御 DianaDefend | **♦3**（方块 3，红色显示） |

其余卡牌待分配（原则：越稀有点数越大）。

### 1.3 第二资源：心之壁

与贝拉的心之壁规则完全相同（开局 5、无上限、回合开始不变化、战斗结束清零），
只是图标是嘉然自己的资源。

- 实现：`ZhijiangCode/SecondResource/DianaHeartWall.cs`（次级资源 id `Zhijiang.DianaHeartWall`）
- 图标：`Zhijiang/images/characters/Diana/Diana_HeartWall_text.png`（48×48）、`Diana_HeartWall_big.png`（256×256）
- 为什么注册成独立的次级资源：RitsuLib 的 `SecondaryResourceDefinition` 图标路径在注册期固定，
  没有"按角色切图"的钩子。独立 id 之后，嘉然换专属图标只改 DianaHeartWall.cs，不影响贝拉。
- 嘉然卡牌消耗心之壁时用 `DianaHeartWall.DianaHeartWallId`（**不是** `HeartWall.HeartWallId`）。
- 本地化：`ZHIJIANG_SECONDARY_RESOURCE_DIANA_HEART_WALL.title/.description`（static_hover_tips.json，中英同步）

### 1.4 初始卡组

| 卡牌 | 数量 |
|---|---|
| 打击（DianaStrike） | 4 |
| 防御（DianaDefend） | 4 |

（共 8 张，待补 2 张独有初始牌凑满 10 张）

### 1.5 初始遗物

「嘉心糖」（`JiaXinTang`）：效果与描述暂空，待玩法定稿后补。

## 二、遗物

### 2.1 初始遗物

| 遗物 | 稀有度 | 效果 |
|---|---|---|
| 嘉心糖 JiaXinTang | Starter | 待定 |

### 2.2 专属遗物

暂无。

## 三、药水

暂无专属药水（`DianaPotionPool` 结构已建好）。

## 四、卡牌

### 4.0 已实现卡牌一览

| 卡牌 | 稀有度 | 类型 | 耗能 | 效果 |
|---|---|---|---|---|
| 打击 DianaStrike | Basic | Attack | 1 | 造成 6（升级 9）点伤害 |
| 防御 DianaDefend | Basic | Skill | 1 | 获得 5（升级 8）点格挡 |

### 4.1 基础牌（Basic）

- **打击 DianaStrike**：1 费，造成 6 点伤害；升级 +3。
- **防御 DianaDefend**：1 费，获得 5 点格挡；升级 +3。

## 五、美术资源进度

已换成嘉然专属（2026-10 由用户提供）：

- [x] 头像 `Diana_character_icon.png`（500×500）、头像轮廓 `Diana_character_icon_outline.png`
- [x] 选人立绘 `Diana_character_select.png`、锁定态 `Diana_character_select_locked.png`（均 848×1264）
- [x] 选人背景 `Diana_character_select_bg.tscn` + `select_bg_images/` 120 帧（1920×1080），已接到 `AssetProfile.Ui.CharacterSelectBgPath`
- [x] 遗物图 `images/relics/JiaXinTang_85x85.png` / `JiaXinTang_256x256.png`

仍是贝拉复制件（待替换）：`Diana_character.png`、`Diana_energy_big/text.png`、`Diana_map_icon.png`、`Diana_site.png`、`Diana_HeartWall_big/text.png`、`multiplayer_hand_diana_*.png`

## 六、待办

- [ ] 两张独有初始牌（凑满 10 张初始卡组）
- [ ] 「嘉心糖」效果与文案
- [ ] 战斗人物模型场景 `Diana_character.tscn` + 帧图（idle 196 / die 95 / attack 61 / hurt 39 / cast 50，约 117 MB）——**缺它战斗中显示占位角色**
- [ ] 卡图 `Zhijiang/images/cards/Diana/diana_attack.png`、`diana_defend.png`（构建里仅剩的 2 条 RITSU013 警告）
- [ ] 能量图标、地图图标、篝火图、立绘改成嘉然粉（现在是贝拉珊瑚粉的复制件）
- [ ] 扑克牌核心机制
