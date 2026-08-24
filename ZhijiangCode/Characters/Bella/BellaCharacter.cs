using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Scaffolding.Godot;

namespace Zhijiang.ZhijiangCode.Characters.Bella;

[RegisterCharacter]
public sealed class BellaCharacter : ModCharacterTemplate<BellaCardPool, BellaRelicPool, BellaPotionPool>
{
    public static readonly Color ThemeColor = new(0.42f, 0.65f, 0.72f);
    // 应援色（#DB7D74）。
    public static readonly Color SupportColor = new(0.8588f, 0.4902f, 0.4549f);

    private const string SceneRoot = $"{Entry.ResPath}/scenes/characters";
    private const string ImageRoot = $"{Entry.ResPath}/images/characters";
    private const string CharacterScenePath = $"{SceneRoot}/Bella/Bella_character.tscn";
    private const string EnergyCounterScenePath = $"{SceneRoot}/Bella/Bella_energy_counter.tscn";
    private const string MerchantScenePath = $"{SceneRoot}/Bella/Bella_merchant.tscn";
    private const string RestSiteScenePath = $"{SceneRoot}/Bella/Bella_rest_site.tscn";
    private const string CharacterSelectBgScenePath = $"{SceneRoot}/Bella/Bella_character_select_bg.tscn";

    // 角色名称颜色。
    public override Color NameColor => ThemeColor;
    // 能量图标轮廓颜色。
    public override Color EnergyLabelOutlineColor => new(0.08f, 0.18f, 0.24f);
    // 地图绘制颜色（应援色 #DB7D74）。
    public override Color MapDrawingColor => SupportColor;

    // 人物性别（男女中立）。
    public override CharacterGender Gender => CharacterGender.Feminine;

    // 初始血量和金币。
    public override int StartingHp => 75;
    public override int StartingGold => 99;

    // CharacterAssetProfile 按类别拆分。你只写需要替换的部分，其他字段会保留回退。
    // AssetProfile 只指定模板自带的静态占位资源；没有复制的音频、拖尾、转场等资源继续从占位角色回退。
    public override CharacterAssetProfile AssetProfile => new(
        Scenes: new CharacterSceneAssetSet(
            // 人物模型 tscn 路径。
            VisualsPath: CharacterScenePath,
            // 能量表盘 tscn 路径。
            EnergyCounterPath: EnergyCounterScenePath,
            // 商店人物场景。
            MerchantAnimPath: MerchantScenePath,
            // 篝火休息场景。
            RestSiteAnimPath: RestSiteScenePath),
        Ui: new CharacterUiAssetSet(
            // 人物头像路径。
            IconTexturePath: $"{ImageRoot}/Bella/Bella_character_icon.png",
            // 左上角头像
            IconPath: $"{SceneRoot}/Bella/Bella_character_icon.tscn",
            // 人物头像轮廓。
            IconOutlineTexturePath: $"{ImageRoot}/Bella/Bella_character_icon_outline.png",
            // 人物选择背景。
            CharacterSelectBgPath: CharacterSelectBgScenePath,
            // 人物选择图标。
            CharacterSelectIconPath: $"{ImageRoot}/Bella/Bella_character_select.png",
            // 人物选择图标-锁定状态。
            CharacterSelectLockedIconPath: $"{ImageRoot}/Bella/Bella_character_select_locked.png",
            // 地图上的角色标记图标、表情轮盘上的角色头像。
            MapMarkerPath: $"{ImageRoot}/Bella/Bella_map_icon.png"),
        // 卡牌拖尾特效：未提供专属拖尾场景时沿用占位角色（ironclad）的拖尾，
        // 颜色通过 TrailStyle 染成贝拉应援色（SupportColor #DB7D74）。
        // 原版缎带渐变亮段与剪影精灵贴图均为白色，相乘染色后亮部即精确的 #DB7D74；
        // 大火花自带红橙 color ramp，相乘后呈暗红火花（与粉色拖尾同一暖色系）。
        Vfx: new CharacterVfxAssetSet(
            TrailStyle: new CharacterTrailStyle(
                OuterTrailModulate: SupportColor,
                InnerTrailModulate: SupportColor,
                BigSparksColor: SupportColor,
                LittleSparksColor: SupportColor,
                PrimarySpriteModulate: SupportColor,
                SecondarySpriteModulate: SupportColor)),
            Multiplayer: new CharacterMultiplayerAssetSet(
                ArmPointingTexturePath: $"{ImageRoot}/Bella/multiplayer_hand_bella_point.png",
                ArmRockTexturePath: $"{ImageRoot}/Bella/multiplayer_hand_bella_rock.png",
                ArmPaperTexturePath: $"{ImageRoot}/Bella/multiplayer_hand_bella_paper.png",
                ArmScissorsTexturePath: $"{ImageRoot}/Bella/multiplayer_hand_bella_scissors.png"),
            // 美味饼干：按人物显示专属图标（占位路径，后续补图）。
            VanillaRelicVisualOverrides:
            [
                new(
                    CharacterOwnedVanillaRelicModelId.YummyCookie,
                    new RelicAssetProfile(IconPath: $"{Entry.ResPath}/images/relics/Bella_yummy_cookie_override.png"))
            ]
        );

    // 某个字段没写时，RitsuLib 会从占位角色配置里补齐。
    public override string? PlaceholderCharacterId => "ironclad";
    // 如果你的人物不需要时间线小故事，加上这句。
    public override bool RequiresEpochAndTimeline => false;
    // 攻击和施法动画延迟，以对齐动画。静态占位资源不需要延迟。
    public override float AttackAnimDelay => 0f;
    public override float CastAnimDelay => 0f;

    // 让 RitsuLib 把普通 Godot 场景转换成游戏需要的 NCreatureVisuals。
    // 自动转换人物场景，让你不需要手动挂脚本。复制即可。
    protected override NCreatureVisuals? TryCreateCreatureVisuals()
    {
        return RitsuGodotNodeFactories.CreateFromScenePath<NCreatureVisuals>(
            CharacterScenePath);
    }

    // 攻击建筑师的攻击特效列表。
    public override List<string> GetArchitectAttackVfx()
    {
        return
        [
            "vfx/vfx_attack_blunt",
            "vfx/vfx_heavy_blunt",
            "vfx/vfx_attack_slash",
            "vfx/vfx_bloody_impact",
            "vfx/vfx_rock_shatter"
        ];
    }
}
