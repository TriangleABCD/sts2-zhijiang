<!-- 由 Trae 项目记忆迁移而来，原文件：.trae/memory/projects/-d-Files-Project-sts2-mod-project-sts2-zhijiang/project_memory.md（迁移于 2026-08-15）。DSH 会在每次会话启动时自动加载本文件作为工作区指令；请在 DSH 中继续维护本文件。 -->

# 枝江 Mod 项目记忆

## 新增卡牌标准化流程

每次新增贝拉卡牌时，先向用户确认以下信息，全部确认后再开始编码：

### 必问项

1. **卡牌名称**（中文 + 英文）
2. **稀有度**：Basic / Common / Uncommon / Rare / Ancient
3. **卡牌类型**：Attack / Skill / Power
4. **耗能**：耗能点数（0~X）
5. **目标类型**：Self / AnyEnemy / AllEnemies / AllAllies / 其他
6. **效果描述**：打出后的具体效果（包括具体数值）
7. **升级效果**：升级后效果有何变化（数值变化、添加/移除关键词等）
8. **特殊属性**：是否需要消耗(Exhaust)、固有(Innate)、保留(Retain) 等关键词
9. **是否初始卡**：是则需指定数量（如 1 张或 4 张），否则为普通可收集卡
10. **是否消耗心之壁**：是则需指定消耗量

### 需要修改的文件

| 文件 | 内容 |
|------|------|
| `ZhijiangCode/Cards/Bella/{CardName}.cs` | 卡牌代码 |
| `Zhijiang/localization/zhs/cards.json` | 中文本地化 |
| `Zhijiang/localization/eng/cards.json` | 英文本地化 |
| `doc/Bella.md` | 卡牌文档 |

### 代码规范

- 继承 `ModCardTemplate`
- 使用 `[RegisterCard(typeof(BellaCardPool))]` 注册到贝拉卡池
- 若是初始卡，额外添加 `[RegisterCharacterStarterCard(typeof(BellaCharacter), N)]`
- 本地化 key 格式：`ZHIJIANG_CARD_{类名Slugify大写}.title/description`（项目里顺手写的 `.smartDescription` 游戏不读，见下）
- Slugify 规则：PascalCase → SCREAMING_SNAKE_CASE（如 `ASoulIsComing` → `A_SOUL_IS_COMING`；数字不前后加下划线，如 `LoopIn20` → `LOOP_IN20`）；算法见原版 `StringHelper.Slugify`（大写边界插下划线、转大写、数字视为普通字符，边缘如 `HTTPServer2Card` → `H_TT_P_SERVER2_CARD`）
- 参考原版游戏对应效果卡牌的实现
- 升级改耗能用 `base.EnergyCost.UpgradeBy(-1)`（参考 `IceBeauty`/`LoopIn20`）；加减关键词用 `AddKeyword`/`RemoveKeyword`；改次级费用用 `this.SecondaryCosts().Set(...)` 覆盖（参考 `LoopIn20` 150→120）
- 卡牌本地化只有 `.title` + `.description` 两个 key（原版 cards.json 没有 smartDescription）；`smartDescription` 只属于 Power/Orb——运行时实例化版、用 `{Amount}`/`{OwnerName}` 显示实时数值，`description` 是静态概念版。项目 cards.json 里每卡多写的 smartDescription 是习惯性冗余、游戏不读
- 卡池卡框色/能量色在 `BellaCardPool` 配置：`MaterialUtils.CreateHsvShaderMaterial(0.015f, 0.47f, 0.859f)`（#DB7D74），`EnergyColorName = "Bella"`，卡面/名字色取 `BellaCharacter.ThemeColor`（0.42, 0.65, 0.72）；地图画线色是 `BellaCharacter.MapDrawingColor`（#DB7D74），与 ThemeColor 相互独立
- 本地化术语高亮统一用原版风格 `[gold]...[/gold]`（不要再用 `[green]`）：白拉/黑拉/黑白拉/白拉/黑拉、阳牌/阴牌/阴阳牌、阴阳性/阴阳差/阴阳平衡、敏捷/力量/格挡、消耗/升级、抽牌堆/弃牌堆/手牌、易伤/虚弱，中英文两个文件同步修改

### 注意事项

- 新卡在卡牌纵览显示"未知"是正常的，需游戏中实际遇到后才会解锁显示
- 有多名玩家时注意用 `&& c.Player != base.Owner` 排除自身重复
- 添加卡牌到抽牌堆/弃牌堆后需调用 `CardCmd.PreviewCardPileAdd()` 刷新 UI
- 遗物能力图标需要禁用显示
- **能力图标规则**：只有能力牌（CardType.Power）直接施加的能力才显示图标（`IsVisibleInternal = true`）；非能力牌（Attack/Skill）用能力间接实现效果的能力一律不显示图标（`IsVisibleInternal = false`）。`IsVisible` 不是虚方法，只能覆写 `IsVisibleInternal`（false 时连 hover tip 也不生成）
- **本地化必须中英同步**：zhs 新增条目而 eng 缺失会导致构建警告，新内容两个文件一起改
- **卡牌文案风格对齐原版（2026-09 对原版 `localization/zhs/cards.json` 587 条的调研结论）**：
  - 「你」「此牌」都是原版官方用词，**不要删**——原版 269 条含「你」（45.8%）、17 张含「此牌」（跨全部 5 个角色：ANGER/LUNAR_BLAST/MURDER/PRECISE_CUT/ROCKET_PUNCH…），我方比例（48.9% / 6.7%）与之持平。「此牌」= 正在被打出的这张牌本身（耗能/伤害/复制品），「这张牌」= 作为牌堆里一张实体的牌（原版 49 条里 28 条提到手牌/弃牌堆/抽牌堆）——涉及牌堆条件时才用「这张牌」
  - **伤害加成从句省略主语、写完整句**：zh「…造成的伤害就额外增加{X}点。」（GANG_UP）/「…此牌就额外造成{X}点伤害。」（MURDER）/「…，伤害+{X}。」（PERFECTED_STRIKE，仅此 1 例）；en「Deals {X} additional damage for each …」（原版 9 例）。**不要写**「此牌伤害 +{X}」/「This deals +{X} damage」
  - **多句描述在句号后换行分段**（JSON 里写 `\n`）：原版 51%（301/587）的卡都这么做，如「造成X点伤害。`\n`将一张此牌的复制品加入你的弃牌堆。」；`{}` formatter 块内不断行；en 断行要避开 `A.T. Field` 这类缩写
  - 清点"已发生的出牌"用「本回合中每打出过一张…」（LUNAR_BLAST）；原版「本回合中」7 例、阿拉伯数字（`1张`）与原版混用可接受
  - **术语统一**：卡面一律写 `[gold]反差牌[/gold]`（= 与当前白拉/黑拉状态阴阳相反的牌），长句写法「阴阳性与当前黑白拉状态相反的牌」已废弃；定义只写在「阳/阴」两个关键词的 hover 说明里（`card_keywords.json` 中英同步），卡面不再展开解释
  - **会变化的次数用 `{InCombat:（攻击N次）|}` 实时显示**：加 `CalculatedVar("CalculatedHits")`（配 `CalculationBaseVar`/`CalculationExtraVar`）+ 静态 multiplier 函数，`OnPlay` 里用 `((CalculatedVar)DynamicVars["CalculatedHits"]).Calculate(cardPlay.Target)` 取同一数值，保证卡面预览与实战一致（参考原版 `FlakCannon` 散射炮 / 本项目 `MoreLoverMorePowerful`）
    - ⚠️ **计算公式是 `CalculationBase + CalculationExtra × multiplier`**：multiplier 只能返回「额外次数」，固定的基础次数交给 `CalculationBaseVar`，否则重复计数（踩过：`MoreLoverMorePowerful` 的 multiplier 返回 `1 + 反差牌数` 且 base 又是 1，开局 0 张反差牌就显示/打出 2 次）。对照：`Npk48` 是 base 0 + 牛批数；`LittleLighter` 是 base 0 + `1 + 累计加成`——两种写法都对，但 base 与 multiplier 的职责不能重叠
  - 倒计时/回合数类描述用「{Turns}回合后，…」（原版「在{Turns}回合结束后」是病句，不要照抄）；临时改耗能写「…本回合的耗能减少1」（原版「这张牌的耗能减少1」）
    - ⚠️ **伤害型计算变量必须配 `ExtraDamageVar`**：`CalculatedDamageVar`（参考原版 `BodySlam` 全身撞击）的 `GetExtraVar()` 取的是 `DynamicVars.ExtraDamage`，所以 CanonicalVars 里要写 `new CalculationBaseVar(0m)` + **`new ExtraDamageVar(1m)`** + `new CalculatedDamageVar(ValueProp.Move).WithMultiplier(...)`；配成 `CalculationExtraVar`（那是**普通** `CalculatedVar` 用的键 `CalculationExtra`）会在卡牌预览时抛 `KeyNotFoundException: 'ExtraDamage'`。**症状**：控制台 `card ZHIJIANG_CARD_XXX` 加牌不成功、没有任何报错，**一张卡卡在屏幕左上角**（异常发生在 `CardPileCmd.Add → CreateCardNodeAndUpdateVisuals → NCard.UpdateVisuals` 里，节点没被摆位；异常被 `TaskHelper.LogTaskExceptions` 吞进日志）。踩过：`IronStick`（2026-10 已修）
- **描述中的占位符必须对应 CanonicalVars 中定义的 DynamicVar 名称**，C# 常量（如 `private const int HitCount`）不是 DynamicVar，需在描述中直接写死数值，否则卡面会显示原始占位符文本
- `CardPlay` 在 `MegaCrit.Sts2.Core.Entities.Cards`、`CardModel` 在 `MegaCrit.Sts2.Core.Models`；卡牌/遗物里 `base.Owner` 已是 Player，取生物用 `base.Owner.Creature`（不要写 `.Player`）。⚠️ 遗物回调参数里的 player 引用跨回合会变，施加能力一律用 `base.Owner.Creature`，否则第二回合起加成失效（历史 bug）
- DynamicVar 的 `BaseValue` 是 decimal，传给 int 参数要 cast 或取 `.IntValue`（如 `(int)DynamicVars["HeartWallGain"].BaseValue`，参考 `PreventionShot`）
- 临时减力量用 `TemporaryStrengthPower` + `IsPositive => false`（参考 `BpknPower`）
- `GetTeammatesOf` 单人局也会返回自己，联机逻辑一律先 `c.Player != base.Owner` 去重（参考 `ASoulIsComing`）
- 测试时卡池卡牌太少会导致商店黑屏（保证卡池里有足够多的卡再逛商店）；游戏内按 `~` 打开控制台，`card ZHIJIANG_CARD_XXX`（本地化 key）可直接把卡加入手牌
- CanonicalVars 只做"描述占位符→数值"映射、不是效果逻辑：若 OnPlay 里手动生成卡牌（如灵魂），不要再加对应 CardsVar，否则引擎会额外自动生成一份、总数翻倍
- 项目启用 ImplicitUsings（net9.0 / LangVersion 13 / Nullable enable），但 `CardPlay`/`CardModel` 等仍需显式 using
- 充能球 API 坑：RitsuLib 无 Orb 操作封装，直接用原生 `OrbCmd`/`OrbQueue`；`OrbCmd.Channel` 无栏位时自动补 1 栏，栏位满时不会自动激发而是直接替换——想腾位置要先 `OrbCmd.EvokeNext`（参考 `EvilBellaPower`）；`OrbCmd.AddSlots/RemoveSlots` 上限 10；栏位队列取 `player.PlayerCombatState.OrbQueue`（`Orbs`/`Capacity`）
- "每当生命值减少"没有独立事件，用 `AfterDamageReceived` + `result.UnblockedDamage > 0` 判定（被格挡不掉血不触发，参考 `EvilBellaPower`）；治疗走 `AfterCurrentHpChanged`（delta 为负即伤害）；`DamageResult` 常用字段：UnblockedDamage/BlockedDamage/TotalDamage/WasFullyBlocked/WasTargetKilled
- 远古体系注册：遗物升级 `[RegisterTouchOfOrobasRefinement(typeof(KiraBellaris))]`（贝极星→闪耀贝极星，升级遗物 `RelicRarity` 保持 `Starter`，未命中降级为 Circlet）；卡牌转化 `[RegisterArchaicToothTranscendence(typeof(MadCow))]`（勇敢牛牛→疯牛，转化保留升级与附魔）；远古卡仍 `[RegisterCard(typeof(BellaCardPool))]` 但 Ancient 不进奖励池
- ⚠️ **初始遗物 `Rarity` 必须是 `RelicRarity.Starter`**，且建议同时 `IsAllowedInShops => false`：否则会被商店 RelicGrabBag 当作 Common/Uncommon/Rare 抽出来售卖，也会被「先古之民」事件漏判（该事件靠 `Rarity == RelicRarity.Starter` 找初始遗物）。贝极星已于 2026-08-18 修复（原误设 Common）
- ✅ **人物独有内容注册已补齐**（2026-08-18，见 `doc/Bella.md` 4.6）：古老牙齿（勇敢牛牛→疯牛）、欧洛巴斯之触（贝极星→闪耀贝极星）、尘封魔典（已有贝极星的眼泪）、美味饼干（`VanillaRelicVisualOverrides`，占位图）、海玻璃（`SEA_GLASS.ZHIJIANG_CHARACTER_BELLA_CHARACTER.title`）、色彩哲学家（`BellaCardPool` 实现 `IModColorfulPhilosophersCardPool` + `events.json`）、先古对话（`ancients.json` 补齐）

### 卡牌框架代码

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Zhijiang.ZhijiangCode.Characters.Bella;

namespace Zhijiang.ZhijiangCode.Cards.Bella;

[RegisterCard(typeof(BellaCardPool))]
public sealed class {CardName} : ModCardTemplate
{
    private const int BaseEnergyCost = {N};
    private const CardType CardKind = CardType.{Type};
    private const CardRarity CardRarityValue = CardRarity.{Rarity};
    private const TargetType CardTarget = TargetType.{Target};
    private const bool ShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Zhijiang{Skill|Attack|Power}.png");

    public {CardName}() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // TODO: 卡牌效果
    }

    protected override void OnUpgrade()
    {
        // TODO: 升级效果
    }
}
```

### 卡牌清单 / 数值 / 进度（指针）

> ⚠️ 卡表**不在本文件维护**（避免与文档两处数据打架）。查这些内容一律去 `doc/Bella.md`：
> - 卡牌阴阳属性表（含稀有度 / 类型 / 备注，如「多人模式专属」）→ **4.0 节**
> - 逐卡数值与升级效果 → **4.1～4.5 节**（按 Basic / Common / Uncommon / Rare / Ancient 分表）
> - 卡牌打出音效（哪些卡有声音、音频文件名与音量、Ogg Vorbis 格式要求、设置里的音效总开关）→ **4.7 节**
> - 卡池目标数量与完成进度 → 上述各节标题下的「目标数量」说明
> - 初始卡组、初始遗物 → **1.4 / 1.5 节**；专属遗物、专属药水 → **2.x / 3.x 节**
> - 与原版对齐的统计基线 → `doc/原版角色卡牌统计.md`、`doc/原版角色遗物统计.md`、`doc/原版角色专属药水统计.md`
>
> 改卡流程见本文档「新增卡牌标准化流程」：改代码 → 改 `doc/Bella.md` 对应表 → 中英本地化同步；**不要回来改 AGENTS.md 的卡表**。
> 仍需牢记的硬约束：初始卡组 10 张 = 打击×4 + 防御×4 + 勇敢牛牛×1 + 不怕困难×1（5 阳 5 阴，开局白拉）。

## 心之壁（第二费用）机制定稿

- 注册位置：`ZhijiangCode/SecondResource/HeartWall.cs`（`HeartWall.Register()` 由 Entry 初始化时调用），RitsuLib `SecondaryResource`，仅贝拉角色显示（`AlwaysShowInCombatUiForCharacter<BellaCharacter>`）。
- **定稿行为**（2026-10 改版：**心之壁→敏捷已删除**，只作纯第二费用）：`defaultAmount: 5`（**每场战斗开局 5**）、`baseMaxAmount: null`（**无上限**）、`turnStartPolicy: None`（**回合开始不自动变化**，战斗中只增不减）、`persistencePolicy: Combat`（**每场战斗重置为 5**、战斗结束清零）、下限 0。
- RitsuLib `SecondaryResourceDefinition` 共 11 个构造参数（后 5 个为图标/本地化）；`baseMaxAmount: null` = 无上限概念，另有 `hardMaxAmount` 硬上限与 `minAmount` 硬下限。枚举语义（v0.4.57）：`turnStartPolicy` = None/ResetToMax/AddMaxToCurrent/Clear（注意 ResetToMax 与 AddMaxToCurrent 不同）；`persistencePolicy` = None/Combat/Run（Combat = 每场战斗重置）。
- ⚠️ **别用 `turnStartPolicy: ResetToMax` 做每回合重置**（2026-10 试过又回退）：① `ResetToMax` 走 `SecondaryResourceCmd.Reset(toMax: true)`，目标 = `GetMax()`，无上限时 `GetMax()` 返回 `null`、回退到 `DefaultAmount`，所以它只能把资源重置成默认值、同时又把上限卡死在该值；② 重置时机在 `Hook.AfterSideTurnStart` 的 Postfix（`await original` 之后），**能力/遗物 `AfterSideTurnStart` 里加的量会被同一次重置抹掉**（「对抗路情侣」「情感障碍」升级收益直接失效），③ 重置产生的负 `Delta` 还会被「喔拉姐」当成"失去心之壁"结算伤害 ④ 跨回合攒不到 150 费「20号循环」的钱。
- 战斗 UI 计数器：能量计数器旁 `(+60, -100)`，90×90，字号 28，白色正数，格式为纯数字（不带 /max）；图标 `Zhijiang/images/characters/Bella/HeartWall_text.png`（**48×48**）/ `HeartWall_big.png`（**256×256**）（已有专属图标，不再复用能量图标）。
- 卡牌消耗显示：能量图标下方 `(0, +80)`，48×48 图标 + 字号 24（`NSecondaryResourceCardCostUi`）。
- 卡牌侧 API：完整资源 ID 是 `Zhijiang.HeartWall`，代码里用 `HeartWall.HeartWallId` 静态属性；增减 `SecondaryResourceCmd.Get/Gain(base.Owner, HeartWall.HeartWallId, n, this)`；卡牌费用在构造函数里 `this.SecondaryCosts().Set(HeartWall.HeartWallId, N)`（升级用 Set 覆盖，支付/CanPlay 校验/卡面费用显示由 RitsuLib 自动处理）；耗空全部心之壁：`Get` 读当前值 → `SecondaryResourceCmd.Lose` 全扣（参考 `TearOfBellaris`）
- 本地化里心之壁图标用 Godot BBCode 直接渲染，**必须带尺寸**：`[img=24x24]res://Zhijiang/images/characters/Bella/HeartWall_text.png[/img]`。⚠️ 源图是 48×48 的，而 `[img]` 不带尺寸时按**原始像素**渲染（实测 4.5.1：`[img]`→48px、`[img=24x24]`→24px、`[img=top]`→只是垂直对齐不改尺寸），不带尺寸会把卡面/遗物/药水描述的行高撑坏（共 67 处引用）。⚠️ RitsuLib 的 `{secondaryResource:secondaryResourceIcons(...)}` formatter 在本地化管线里不生效（原样显示占位符），统一用 `[img=…]`。
- 本地化（`static_hover_tips.json`）：key `ZHIJIANG_SECONDARY_RESOURCE_HEART_WALL.title/.description`（第二资源 key 格式 `ZHIJIANG_SECONDARY_RESOURCE_{LocalId大写}`）；zhs「心之壁 / 唉，有心之壁了」，eng「A.T. Field / An impenetrable barrier of the heart.」（A.T. Field = EVA 绝对恐怖领域梗）。

## 卡牌拖尾特效（Trail VFX）配置（新角色复用指南）

- 回合结束弃牌/奖励拿牌时卡牌飞行的发光拖尾由 `CharacterModel.TrailPath` 指定场景（原版按角色 id 拼 `vfx/card_trail_{id}`，完整路径 `res://scenes/vfx/card_trail_{id}.tscn`，见 `SceneHelper.GetScenePath`）。
- Mod 角色未提供专属场景时回退占位角色的拖尾（贝拉 = ironclad）；RitsuLib 补丁 `CharacterTrailPathPatch` / `CharacterTrailStyleOverridePatch` 支持覆盖，无需自建资源。
- **给新角色指定颜色（两步）**：① 角色类加应援色常量（如 `SupportColor`）；② `CharacterAssetProfile.Vfx` 配 `TrailStyle`（`CharacterTrailStyle`，全字段可选），按节点路径染色：
  - `Trails/OuterTrail`、`Trails/InnerTrail`（Line2D：`Modulate` **相乘**染色 + `Width` 宽度）
  - `Sprites/BigSparks`、`Sprites/LittleSparks`（CPUParticles2D：`Color`，会与 color ramp **相乘**）
  - `Sprites/Sprite2D2`、`Sprites/Sprite2D3`（Sprite2D：`Modulate` + `Scale`）
  ```csharp
  Vfx: new CharacterVfxAssetSet(
      TrailStyle: new CharacterTrailStyle(
          OuterTrailModulate: SupportColor,
          InnerTrailModulate: SupportColor,
          BigSparksColor: SupportColor,
          LittleSparksColor: SupportColor,
          PrimarySpriteModulate: SupportColor,
          SecondarySpriteModulate: SupportColor))
  ```
- **颜色原理（实测结论）**：所有原版拖尾的缎带渐变最亮段都是纯白 (1,1,1)、剪影精灵是白底 → 相乘染色后**亮部即精确等于所设颜色，无需反算**；小火花 ramp 是暖白 → 染后≈目标色；大火花 ramp 是各角色专属彩色渐变（随粒子寿命变化），单值相乘无法全程精确，染后呈「目标色×原ramp」的暗色调。选底建议：目标色与哪个原版场景同色系就回退哪个（或设 `Vfx.TrailPath` 指向它），观感最协调。要 100% 精确需自建专属拖尾场景（节点结构：`Trails/OuterTrail`、`Trails/InnerTrail`、`Sprites/BigSparks`、`Sprites/LittleSparks`、`Sprites/Sprite2D2`、`Sprites/Sprite2D3`）。
- **原版 5 个拖尾场景底色**（外缎带 modulate / 大火花 ramp 主色，供选底参考）：
  | 场景 | 外缎带 | 大火花 ramp |
  |------|------|------|
  | ironclad | 红橙 (1, 0.169, 0) | 红→橙→暗红 |
  | silent | 绿 (0, 0.668, 0.118) | 黄绿→暗绿 |
  | defect | 蓝 (~0, 0.604, 0.79) | 蓝青→暗青 |
  | regent | 橙棕 (0.624, 0.276, 0) | 金黄→棕 |
  | necrobinder | 粉红 (1, 0.126, 0.288) | 粉红→紫红 |
- **查看原版场景内容的方法**：用本机 Godot headless 加载游戏 pck 直接读 `res://` 文本（无需反编译）：临时工程放一个 `SceneTree` 脚本，`ProjectSettings.load_resource_pack("<游戏目录>/SlayTheSpire2.pck", true)` 后 `FileAccess.open("res://scenes/vfx/card_trail_xxx.tscn", FileAccess.READ)` 导出文本即可。
- 贝拉已配置：全部染成 `SupportColor`（0.8588, 0.4902, 0.4549 = #DB7D74，应援色，与 `MapDrawingColor` 同值），底为 ironclad 拖尾（红橙系，与粉色同暖色系），实测观感好。
- ⚠️ **洗牌拖尾例外（2026-08-17 首次尝试；2026-08-29 确认根因并修复）**：弃牌堆→抽牌堆的洗牌飞行特效（`NCardFlyShuffleVfx`）调用 `NCardTrailVfx.Create` 时跟随节点是洗牌特效本身、不是 `NCard`，RitsuLib 的 `CharacterTrailStyleOverridePatch`（靠 `card is NCard` 反查角色）不会给它染 TrailStyle，会显示占位角色的原色（贝拉显示战士红）。本项目补丁 `ZhijiangCode/Patches/BellaShuffleTrailStylePatch.cs`（注册于 `Entry.cs`）在 `NCardTrailVfx.Create` 后拦截洗牌特效，读私有字段 `_targetPile` 与当前战斗各玩家牌堆实例做引用相等反查贝拉玩家，染成与 TrailStyle 完全相同的 `SupportColor`。⚠️ 根因：此前 `Entry.cs` 只调用了 `RegisterPatch` 而漏掉 `PatchAll()`，补丁实际从未应用到游戏；修复后需在 `Entry.cs` 对洗牌拖尾补丁调用 `PatchAll()` 才会生效。以后新角色若也要洗牌拖尾染色，需在该补丁中加对应角色分支（或把染色逻辑抽成共享方法）。

## Mod 设置页（2026-10 引入）

首个设置项是「卡牌自定义音效」开关（玩法说明见 `doc/Bella.md` 4.7）。框架与约定：

- 代码全在 `ZhijiangCode/Settings/`：`ZhijiangSettings.cs`（数据模型 + `ModSettingsValueBinding` 读取入口 `ZhijiangSettingsAccess`）、`ZhijiangSettingsPage.cs`（`Register()`：`ModDataStore.Register<T>` 注册持久化 + `RitsuLibFramework.RegisterModSettings` 注册 UI）；`Entry.Initialize()` 里调用 `ZhijiangSettingsPage.Register()`。
- 持久化：RitsuLib `ModDataStore`，`SaveScope.Global` → mod_data 下的 `settings.json`（本机所有存档共用）；UI 侧自动 AutoSave，点一下即存盘。
- **文案必须走原版已有文本表，mod 不能新建表名**：`LocManager` 按原版 `res://localization/{lang}/*.json` 的文件名枚举，再把各 mod 的 `res://{ModId}/localization/{lang}/{同名文件}` 合并进去。设置页统一用 `settings_ui` 表 → `Zhijiang/localization/{zhs,eng}/settings_ui.json`，key 前缀 `ZHIJIANG_SETTINGS_`，中英同步（`ModSettingsText.LocString("settings_ui", key, fallback)`）。
- 页面入口：主菜单更新日志下方的 RitsuLib 设置入口 / 暂停菜单的模组设置 → 模组「枝江」→「设置」→ 分组「音频」。页面默认在主菜单、跑局暂停、战斗暂停都可见。
- 开关值在**使用点实时读取**（如 `BellaAudio.PlayCardSfx` 开头），所以改完立即生效、不用重启；加音效的卡牌照旧只写 `BellaAudio.PlayCardSfx(...)`，不用自己判设置。

## 开发环境注意

- 本机用 PowerShell 提交 git：中文提交信息用 `git commit -F <UTF-8文件>`（或 `--amend -F`）避免 PS 传参 GBK 乱码；验证仓库实际存储内容用 `git log --output=<文件>` 绕过终端编码
- **游戏日志在 `%APPDATA%\SlayTheSpire2\logs\godot.log`**（同目录还有按启动时间命名的历史日志；`console_history.log` 记录控制台命令）。⚠️ **DevConsole 里不报的异常全在这里**——卡牌/遗物的 UI 初始化异常会被 `TaskHelper.LogTaskExceptions` 吞成一条 `[ERROR]`，游戏内表现为「卡加不进手牌 / 一张卡卡在屏幕左上角」这类怪现象。排查用：`Select-String -Path $log -Pattern '\[ERROR\]|Exception' | Select-Object -Last 30`；`[WARN] [RitsuLib] [Assets] Missing resource path ... Falling back to the base asset` 只是缺图回退，无害
- **构建保持 0 警告**（2026-10 清理完成，基线 `0 警告 0 错误`）：项目开了 `Nullable=enable`，游戏程序集把 `Player.PlayerCombatState`、`CardModel.Pile`、`CardModel.CombatState`、`Creature.CombatState` 标成可空，直接解引用会出 CS8602/CS8604（都是注解噪声，战斗内不会真为空）。正确写法有二：① 清点「本场战斗中仍在循环里的牌」一律用扩展方法 `owner.CardsInCycle()`（`ZhijiangCode/Cards/CombatCardQuery.cs`，已内置判空 + 排除消耗堆），**不要**再手写 `PlayerCombatState!.AllCards.Where(c => c.Pile.Type != PileType.Exhaust)`；② 单点判空用 `is not { } x` 模式或 `!` 断言 + 一行注释说明为何必非空。另：基类 `Task` 虚方法若方法体全同步**不要加 `async`**（CS1998），直接 `return Task.CompletedTask;`。切勿用 `<NoWarn>` 全局屏蔽 CS8602/CS8604（等于关掉整个可空性检查）；现有 `NoWarn` 里的 `MSB3270;CS0436` 是 publicizer 必需的，保留
- `.trae/` 与 `.agents/` 已在 .gitignore，不提交；根目录 `AGENTS.md` 会随仓库提交、供所有协作者/工具共享

## 阴阳机制（核心玩法）设计定稿

> 设计决策已确认（2026-08-12），2026-08-15 重构为「贝极星馈赠与代价」（废除逐卡差值修正），详细设计见 `doc/Bella.md` 1.2 节。编码时以此为准。

### 机制规则

- **卡牌属性**：所有贝拉卡牌带「阳」或「阴」标签（用 RitsuLib CardKeyword 实现，见下）。灵魂/诅咒/战斗临时牌为**中立**，无标签、不计数、不受影响。
- **颜色对应**：白拉 = 阳（白），黑拉 = 阴（黑）。
- **攻防独立**：卡牌的阴阳属性与它偏攻击还是偏防守**互相独立**、不绑定。阴阳只看卡牌属性本身。（「阳=心之壁·守护、阴=充能球·激进」只是历史提案，已作废，2026-08-15 确认）
- **状态判定**：统计贝拉拥有的全部卡牌（含各牌堆）。
  - 阳 > 阴 → **白拉**；阴 > 阳 → **黑拉**；相等 → **白拉**（此时 d=0，馈赠/代价均为基数 1）。
  - ⚠️ **消耗堆（ExhaustPile）不参与计数**：战斗中消耗掉的阴阳牌会被移除出状态统计。
  - ⚠️ **打出堆（PlayPile）中的能力牌不参与计数**：能力牌打出后停留在打出堆、本场战斗不再回到循环，视同离场（`GetOwnedCards` 对 Play 堆只计入非能力牌）。打出堆中的非能力牌只是"正在打出"的瞬态，仍参与计数，避免打牌瞬间状态反复翻转。注意 `CardPile.Type`（牌堆类型）与 `CardModel.Type`（卡牌类型）是两回事。
- **状态效果 = 初始遗物「贝极星」的馈赠与代价**（2026-08-15 定稿：阴阳不再逐张修正卡牌数值，改由力量/敏捷属性系统自动作用于卡牌）：
  - **白拉**（阳 ≥ 阴）：馈赠——每打出 1 张技能牌获得 `1+d÷3` 格挡（固定值、不受敏捷）；代价——战斗开始时失去 `1+d÷3` 力量（整场战斗，压低攻击牌）。
  - **黑拉**（阴 > 阳）：馈赠——每打出 1 张攻击牌对随机一名敌人造成 `1+d÷3` 伤害（纯固定伤害、不受力量、不触发反击）；代价——战斗开始时失去 `1+d÷3` 敏捷（整场战斗，压低防御牌）。
  - `d = |阳牌数 − 阴牌数|`，无上限（鼓励极限偏科）；d=0 时白拉、馈赠/代价均为基数 1。
  - **闪耀贝极星**：馈赠与贝极星数值完全一致，**只有馈赠、没有代价**。
  - 实现：馈赠 `BellarisBlockPower` / `BellarisHeiLaAttackPower`（`AfterCardPlayed` 实时判状态与数值，两遗物共用）；代价控制器 `BellarisYinYangDebuffPower`（仅贝极星施加，内部施加原版 `StrengthPower`/`DexterityPower` 负层数、带图标可见）。
- **初始遗物 = 角色专属阴阳馈赠与代价**（2026-10：原「心之壁 ÷ 15 → 临时敏捷」的通用能力已删除，心之壁不再自动转敏捷，`ModStarterRelicTemplate` 与 `BellarisHeartWallPower` 一并移除；初始遗物直接继承 `BellaStarterRelicTemplate : ModRelicTemplate`）：
  - 阴阳馈赠与代价（见上）。⚠️ 方向是**白拉防御、黑拉进攻**（不是进攻/防守的常规直觉），文档 1.2.2/1.5/2.1 已同步。
  - 实现模式：进入战斗（`AfterRoomEntered` 判 `CombatRoom`）时施加 Power——`BellarisBlockPower`（内部判白拉才生效）与 `BellarisHeiLaAttackPower`（内部判黑拉才生效），各自按状态判断、天然互斥；贝极星额外施加 `BellarisYinYangDebuffPower`（代价控制器）。"每打出卡牌"没有 PowerModel 钩子，用 `AbstractModel.AfterCardPlayed`（参考 `BellarisBlockPower`）
- **反差牌**：与当前状态阴阳相反的牌（白拉时的阴牌、黑拉时的阳牌），无标签牌恒不是。计数机制（供后续卡牌复用）：`BellaYinYangService` 订阅 `CardPlayingEvent` 在每张牌**第一段**打出瞬间按当时状态累加每玩家计数（`IsFirstInSeries` 判定，Replay 多段只算一张），`SideTurnStartedEvent`（玩家侧）清零，`CombatStartingEvent`/`CombatEndedEvent` 清理；卡牌 OnPlay 读 `GetContrastPlaysThisTurn` 时注意自身已在打出瞬间被计入，若自身是反差牌需扣 1（参考 `MoreLoverMorePowerful`）。
- **判定翻转（了转反）**：`IsBaiLa/IsHeiLa` 结果与 `IsInverted`（检测隐藏 Power `TurnOverPower`，由「了转反」施加、战斗结束自动清除）取异或；状态图标与贝极星代价经牌堆变动事件实时同步翻转，反差牌判定也随之取反。了转反本身是无标签中立牌、不参与计数。
- **状态展示**：战斗内用可见 Power（白拉 Power / 黑拉 Power）挂在角色下方，状态翻转时动态替换；战斗外用初始遗物 hover tip 动态文本显示当前状态。
- 角色基础数值：`StartingHp = 75`、`StartingGold = 99`、`Gender = Feminine`、占位角色 `PlaceholderCharacterId = "ironclad"`（缺字段时从占位角色回退）

### 已实现卡牌的阴阳属性（指针）

> 原表已删除——与 `doc/Bella.md` **4.0 节**完全重复，且每加一张卡都要两处同步。
> **新增卡牌时请直接在 `doc/Bella.md` 4.0 节登记阴阳属性**（含稀有度 / 类型 / 备注列）。
> 代码侧不变：各卡 `CanonicalKeywords` 挂载 `BellaYinYangService.YangKeywordId / YinKeywordId.GetModCardKeyword()`。

### 卡牌数值定稿（指针）

> 原表已删除——与 `doc/Bella.md` **4.1～4.5 节**重复。数值以代码为准，改完同步 `doc/Bella.md` 对应分节的表。
> 历史结论保留：早期数值（打击 6、勇敢牛牛 4×2+2、冰山美人 2 球等）已下调，为阴阳效果留空间；
> 打击 / 防御已对齐原版（打击 6→9、防御 5→8，各升级 +3，与 `DefendIronclad` / `StrikeIronclad` 一致）。

### 关键词句号补丁（⚠️ 补丁已写但实测无效，用户决定搁置）

- RitsuLib `GetCardText` 会在金色关键词后拼接 `card_keywords.PERIOD`（句号），原版"消耗"等关键词沿用此样式。
- 已写补丁 `ZhijiangCode/Patches/ModKeywordPeriodRemovalPatch.cs`（`IPatchMethod`，Postfix patch `ModKeywordRegistry.GetCardText`，对 `ZHIJIANG_KEYWORD_YANG/YIN` 去句号），但**实测句号仍然存在**，用户拍板"算了、这个不影响"（2026-08-13）。2026-08-29 复盘确认：该补丁与洗牌拖尾补丁一样从未被应用（只 `RegisterPatch` 未 `PatchAll`），因此"实测无效"并非注入点错误；若以后真要修，只需对它调用 `PatchAll()` 即可先验证，若仍无效再考虑改 `ModKeywordCardDescriptionInjector`。按用户约定仍保持搁置，未应用。
- 注册方式：`RitsuLibFramework.CreatePatcher(ModId, "KeywordPeriodRemoval").RegisterPatch<ModKeywordPeriodRemovalPatch>()`，需 `using STS2RitsuLib.Patching.Core`。

### 贝极星馈赠与代价（定稿）

- **公式**：馈赠/代价 = `1 + |d|÷3`，d = |阳牌数−阴牌数|。白拉：技能牌 → 格挡 / 代价 −力量；黑拉：攻击牌 → 随机敌人伤害 / 代价 −敏捷。
- ⚠️ 幅度必须取 `Math.Abs(d) / 3`——曾因直接用带符号 `d/3` 且 `<= 0` 提前 return，导致黑拉（d<0）时全部差值修正失效（已修复）；现统一走 `BellaYinYangService.ComputeMagnitude`（内部已 Abs）。
- **实现**：馈赠 `BellarisBlockPower` / `BellarisHeiLaAttackPower`（`AfterCardPlayed` 实时判状态与数值，伤害/格挡均为 `ValueProp.Unpowered` 固定值）；代价控制器 `BellarisYinYangDebuffPower.Sync`（施加原版力量/敏捷负层数，撤销旧代价再施加新代价，带图标可见）。
- **施加**：遗物 `AfterRoomEntered`（判 `CombatRoom`）施加三个 Power（闪耀贝极星只施加前两个，无代价）。
- **状态同步**：`BellaYinYangService.RegisterCombatStateSync()` 订阅 `CombatStartingEvent`（初始施加白拉/黑拉标记 + 同步代价）与 `CardMovedBetweenPilesEvent`（任何牌堆变动——消耗、打出能力牌、生成牌等——改变阴阳计数时重算状态、替换状态标记、切换代价）。馈赠每次出牌实时判状态，无需同步。
- **随机敌人**：`player.RunState.Rng.CombatTargets.NextItem(CombatState.HittableEnemies)`（同原版 `JuggernautPower`/`Tingsha` 模式）；黑拉伤害用 `CreatureCmd.Damage(..., ValueProp.Unpowered, ...)`。
- ⚠️ `ModifyCardPlayCount` 是"整张卡重复打出"（Burst/Echo 类），**不是**多段攻击段数。"下一张攻击牌额外打出 N 次"的正确用法见 `LoopIn20Power`：`ModifyCardPlayCount` 返回 `playCount + N`，`AfterModifyingCardPlayCount` 里 `PowerCmd.Decrement` 消耗 1 层

### 状态展示（已实现）

- 战斗内：`BaiLaPower` / `HeiLaPower`（`ZhijiangCode/Powers/`，可见 Power，`StackType.Single`，`IsVisibleInternal=true`），战斗开始时由 `BellaYinYangService.RegisterCombatStateSync()` 的 `CombatStartingEvent` 订阅按当前状态施加，状态翻转时由 `CardMovedBetweenPilesEvent` 订阅动态替换（此前"快照不更新"的遗留问题已随本次改造解决）。
- 战斗外：贝极星/闪耀贝极星的 `AdditionalHoverTips` 动态追加当前状态 Power 的 hover tip（`base.Owner` 即 Player，每次悬停重算）。
- 本地化：`powers.json`（zhs/eng）的 `ZHIJIANG_POWER_BAI_LA_POWER` / `ZHIJIANG_POWER_HEI_LA_POWER`（文案已按新机制更新）。
- ⚠️ 状态标记图标暂用贝拉能量图标占位，后续替换专属图标。
- 阴阳机制源文件清单：`Keywords/BellaYinYangKeywords.cs`；`Characters/Bella/BellaYinYangService.cs`；`Relics/Bella/{Bellaris.cs, KiraBellaris.cs, BellaStarterRelicTemplate.cs, BellarisBlockPower.cs, BellarisHeiLaAttackPower.cs, BellarisYinYangDebuffPower.cs}`；`Powers/{BaiLaPower.cs, HeiLaPower.cs}`；`Patches/ModKeywordPeriodRemovalPatch.cs`

### 本轮开发范围（静态框架）

- ✅ 卡牌标签（阳/阴 CardKeyword）+ 状态判定（白拉/黑拉）
- ✅ 初始遗物「贝极星」的阴阳馈赠与代价（白拉技能格挡/黑拉随机攻击 + 力量/敏捷代价），闪耀贝极星同馈赠无代价。2026-08-15 由"逐卡差值修正"重构而来，旧版 `IBellaYinYangCorrectionCard`/`BellaYinYangCorrectionPower`/`BellarisStrengthPower` 已删除；更早的固定加成数值（+2/+3/+4/+6 等）均已废弃，勿再参考
- ✅ 状态展示（战斗内 Power + 战斗外遗物文本）+ 状态翻转动态同步（标记替换 + 代价切换）
- ✅ 切换牌「阴阳逆转」已实现：「了转反」（稀有技能，消耗；判定翻转）。
- ⏸ 动态机制（临时状态「趋光·堕影」/立场宣言/双极牌「太极劲」）、转化牌「拨乱反正」、反其道牌「混沌之心」、升级翻转属性、药水「阴阳灵液」、事件「阴阳师」→ **后续版本**
- ⚠️ 纪元系统不适用（`RequiresEpochAndTimeline = false`），文档「五、纪元系统」标记为暂不适用

### 代码实现方案

1. **关键词注册**：用 `[RegisterOwnedCardKeyword]` attribute 注册 `yang` / `yin` 两个 CardKeyword
   - `CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription`（显示在卡牌描述**上方**，区别于"消耗/固有"的 AfterCardDescription 显示在下方）
   - 本地化 key：`ZHIJIANG_KEYWORD_YANG.title/.description`、`ZHIJIANG_KEYWORD_YIN.*`，写入 `card_keywords` 本地化表（zhs/eng）
   - 卡牌上挂载：覆盖 `CardModel.CanonicalKeywords` 返回 `"ZHIJIANG_KEYWORD_YANG".GetModCardKeyword()`，或旧式 `RegisteredKeywordIds`（已过时，用新的）
   - **覆盖 `CanonicalKeywords` 必须合并既有关键词**：若该卡已有 `CardKeyword.Exhaust`（消耗），要把阴阳关键词与它一起返回（参考 `ASoulIsComing`/`LoopIn20`/`TearOfBellaris`），否则会挤掉"消耗"
   - 运行时判定：字符串重载 `card.HasModKeyword("...")` 已过时（编译警告），实际代码用 minted 值比较 `card.Keywords.Contains("ZHIJIANG_KEYWORD_YANG".GetModCardKeyword())`（见 `BellaYinYangService.ComputeDiff`）
   - 相关源码：`STS2-RitsuLib/src/Keywords/`（ModKeywordRegistry、ModKeywordDefinition、ModKeywordCardDescriptionPlacement）、`STS2-RitsuLib/src/Interop/AutoRegistration/RegistrationAttributes.cs`（RegisterOwnedCardKeyword）
2. **状态计算**：写 `BellaYinYangService`（静态工具类）：遍历玩家卡组统计阳/阴数量算 d、判状态。**实时计算**，不缓存变量（状态是派生状态，卡组几十张遍历开销可忽略，避免更新时机不同步问题）。
3. **状态效果层**：遗物 `AfterRoomEntered`（`CombatRoom`）施加 `BellarisBlockPower` + `BellarisHeiLaAttackPower`（馈赠，出牌时实时判状态与差值）+ `BellarisYinYangDebuffPower`（代价，仅贝极星；`Sync` 撤销旧代价、施加新代价，内部施加原版力量/敏捷负层数）。
4. **状态展示与同步**：两个可见 Power（`BaiLaPower` / `HeiLaPower`）挂角色下方，参考 `EvilBellaPower`（`IsVisibleInternal = true`）；`RegisterCombatStateSync()` 订阅 `CombatStartingEvent` 初始同步 + `CardMovedBetweenPilesEvent` 翻转同步；遗物 hover tip 动态文本显示当前状态。
5. **随机目标**：`player.RunState.Rng.CombatTargets.NextItem(CombatState.HittableEnemies)`（同原版 `JuggernautPower`/`Tingsha` 模式）；黑拉伤害用 `CreatureCmd.Damage(..., ValueProp.Unpowered, ...)`。
6. **初始卡组配比**：10 张起始牌配 5 阳 5 阴（开局白拉、可自由转向）。

### 遗留事项

- ✅ 状态名「黑拉」与稀有能力牌的重名冲突已解决（2026-08-15）：卡牌更名为「黑贝拉sama」（zhs 卡牌与可见能力标题同步改名），代码类名/文件名保持 `EvilBella` 不变；eng 标题仍为 Evil Bella（英文无重名冲突）。
- 卡牌阴阳具体分配（打击/勇敢牛牛等各属阳还是阴）编码时按 5 阳 5 阴配比分配。
- ⚠️ 代价撤销采用"补回差值"实现：若战斗中其他效果把力量/敏捷整体清空（非按数值抵消），后续翻转时补回可能多算。当前卡池与原版主流效果无此类场景，若未来引入"移除全部力量"类效果需复查。