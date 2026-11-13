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
| 防御 DianaDefend | **♦4**（方块 4，红色显示） |

其余卡牌待分配（原则：越稀有点数越大）。

### 1.2.1 组合判定与增益结算（定稿 2026-10）

- **两族**：同点（按手牌张数，2 张起、无上限）、顺子（4 张起，点数 3～A，2 不参与、A 只在高位）。
- **唯一入口**：`DianaPokerCombos.Evaluate(hand)` → `ComboEvaluation(SameRankGroups, Straights)`；
  UI 高亮与所有增益判定都必须走它，禁止各自实现一套。
- **族内永不重叠（可证）**：同点按点数划分；顺子按最长连续段切分且每个点数只取 1 张。
  任何"同族重叠"都会并成更大的组合（同点变更多张、顺子变更长），所以不会出现两个重叠的同族组合。
- **跨族可以重叠**：仅当某点数在手牌里 ≥2 张时发生 —— 顺子借走 1 张，剩下的还能凑同点组。
  例：手牌 `3,3,3,3,4,5,6,6,6,7` → 同点 `3×4`、`6×3`；顺子 `3-4-5-6-7`；3 与 6 被两族共享。
- **判定时机**：**每次手牌变化时重算**（`NPlayerHand.RefreshLayout` 的后置补丁是唯一触发点），
  显示与结算共用同一份判定结果。
- **多组结算**：
  - 互不重叠的多组 → **每组各触发一次**；
  - 存在跨族重叠 → **顺子优先于同点（不看长度）**：与任一顺子共享牌的同点组**整组不触发**。
- **结算去重**：一张牌一旦参与过某一族的结算，就**不再参与该族**后续结算（另一族不受影响）。
  实现时按族过滤掉已参与的牌，再用其余牌重新判定（组内不足阈值自然消失）。
- **显示（已实现，位置待微调）**：
  - 两个文字按钮「同点」「顺子」，挂在战斗 UI 根节点左下角（`PanelOffset = (48, -132)`），
    点击高亮本族全部组、再点取消（`View`：None / SameRank / Straight，默认 None）；两模式互斥。
  - 描边颜色 = **强度档位**（游戏只提供 3 色 `NCardHighlight.gold/red/playableColor`）：
    同点 2/3/≥4 张 → 金/红/青；顺子 4/5/≥6 连 → 金/红/青。
  - 组号标识 = 卡牌右上角空位挂 `Label`（`%OverlayContainer` 子节点，30px 白字 8px 黑描边），
    显示 ①②③…；离开手牌或不属于组合时隐藏。右上角是实测空位（能量石左上、星标其下、附魔左中、
    标题横幅顶部、类型牌底部中央、描述占下半部）。

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

### 1.5 初始遗物：嘉心糖（王牌花色 · 定稿 2026-10）

**核心**：每场战斗开始时，从四种花色里选一个作为本场战斗的[gold]王牌花色[/gold]，
本场只有这个花色的增益生效。

**选择方式**：**每场战斗都重新选**（不记忆上次选择）。做成"海克斯大乱斗"式的四选一——
四张卡片，卡面是花色 + 效果说明（卡片图由用户制作）。

| 花色 | 效果（回合开始、**抽完牌后**判定一次） |
|---|---|
| ♠ 黑桃 | 手牌里每有 3 张黑桃，本回合获得 1 点[gold]力量[/gold] |
| ♥ 红桃 | 手牌里每有 3 张红桃，本回合获得 1 点[gold]敏捷[/gold] |
| ♦ 方块 | 手牌里每有 3 张方块，抽 2 张牌（**抽牌后不再判定本效果**） |
| ♣ 梅花 | 手牌里每有 3 张梅花，获得 1 点[gold]能量[/gold] |

**升级版「最甜甜甜的嘉心糖」**：只降门槛——「每 3 张」→「每 2 张」；效果本身不再加强。

> 命名：基础版 `JiaXinTang`；升级版 `SweetestJiaXinTang`（本地化 key `ZHIJIANG_RELIC_SWEETEST_JIA_XIN_TANG.*`，
> 中文名「最甜甜甜的嘉心糖」，英文暂定 Sweetest Jia Xin Tang）。
> 注册方式照 贝极星 → 闪耀贝极星：`[RegisterTouchOfOrobasRefinement(typeof(SweetestJiaXinTang))]`。

#### 实现要点（已查证）

- **判定钩子**：`AbstractModel.AfterPlayerTurnStart`（`CombatManager.cs:673` 抽牌 → `675` 调用）。
  ⚠️ **不能用** `Hook.AfterSideTurnStart`（`CombatManager.cs:522`）——它在抽牌**之前**，
  数到的是上一回合残留的手牌。
- **力量 / 敏捷（本回合）**：`TemporaryStrengthPower` / `TemporaryDexterityPower` 都是抽象类，
  照本项目 `BpknPower` 的继承写法；正向用 `IsPositive => true`，回合结束自动消失。
- **抽牌**：`CardPileCmd.Draw`。四条效果在同一个钩子里各算一次、不订阅抽牌事件，
  天然满足"抽牌后不再判定"。
- **选花色面板**：复用战斗 UI 注入（与 `DianaComboButtons` 同一套路），四个卡片按钮；
  不依赖原版屏幕系统。
- **升级遗物注册**：`[RegisterTouchOfOrobasRefinement]`（照 贝极星 → 闪耀贝极星）。

#### 已知待平衡（用户决定：先按上表数值实现，之后再调）

- "每 3 张"在花色占比 25%（均匀卡池）时**每回合只有约 10%** 触发；占比 50%、手牌 5 张时约 50%。
- ♣ 梅花"每 3 张 +1 能量"与 ♦ 方块"每 3 张抽 2 张"明显强于 ♠/♥ 的 +1 力量/敏捷。
- 花色目前不是自由构筑维度（牌面按卡类固定）。**建议**（未定）：分配卡池牌面时让花色带功能倾向
  （♠ 攻击 / ♥ 防御 / ♦ 过牌 / ♣ 费用），使"选王牌花色"等于"选流派"。

## 二、遗物

### 2.1 初始遗物

| 遗物 | 稀有度 | 效果 |
|---|---|---|
| 嘉心糖 JiaXinTang | Starter | 每场战斗开始选一个[gold]王牌花色[/gold]，本场只生效该花色的增益（详见 1.5） |
| 最甜甜甜的嘉心糖（升级） | Starter | 同上，门槛「每 3 张」→「每 2 张」 |

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
- [x] 王牌花色四选一卡片图 `images/ui/suit_choice/{spade,heart,diamond,club}.png`（848×1264，上部花色徽记、下部留空给效果文字；四张同规格）

仍是贝拉复制件（待替换）：`Diana_character.png`、`Diana_energy_big/text.png`、`Diana_map_icon.png`、`Diana_site.png`、`Diana_HeartWall_big/text.png`、`multiplayer_hand_diana_*.png`

## 六、待办

- [ ] 两张独有初始牌（凑满 10 张初始卡组）
- [x] 实现「嘉心糖」+「最甜甜甜的嘉心糖」+ 四选一面板（代码已落地，待游戏内验收）
- [x] 王牌花色选择界面：`Ui/DianaSuitChoicePanel.cs`（自绘全屏遮罩 + 4 张卡片 + 悬停/按下缩放动效；卡片 300×447，效果文字放图片下半部留空区）
- [ ] 真机微调：卡片尺寸/文字字号/文字区位置、悬停缩放是否裁边
- [ ] ⚠️ 多人模式未处理：王牌花色存在本客户端静态状态里，联机两端各自选择会不同步（当前仅保证单人正确）
- [ ] 「嘉心糖 / 最甜甜甜的嘉心糖」本地化文案（中英）
- [ ] 战斗人物模型场景 `Diana_character.tscn` + 帧图（idle 196 / die 95 / attack 61 / hurt 39 / cast 50，约 117 MB）——**缺它战斗中显示占位角色**
- [ ] 卡图 `Zhijiang/images/cards/Diana/diana_attack.png`、`diana_defend.png`（构建里仅剩的 2 条 RITSU013 警告）
- [ ] 能量图标、地图图标、篝火图、立绘改成嘉然粉（现在是贝拉珊瑚粉的复制件）
- [ ] 扑克牌核心机制
