using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;

namespace Zhijiang.ZhijiangCode.Characters.Diana;

// 嘉然（Diana）。当前是"常规角色"骨架：没有第二套玩法机制，卡池里只有打击/防御。
// 美术暂时复用贝拉资源的复制件（Zhijiang/images/characters/Diana/*），卡图路径已写好、文件待补。
[RegisterCharacter]
public sealed class DianaCharacter : ModCharacterTemplate<DianaCardPool, DianaRelicPool, DianaPotionPool>
{
    // 主题色（#E799B0 嘉然粉）：卡面/名字色、遗物与药水实验室描边都用它。
    public static readonly Color ThemeColor = new(0.9059f, 0.6f, 0.6902f);
    // 应援色：与主题色同值（贝拉是两个独立颜色，嘉然目前统一用一个），用于地图画线、卡牌拖尾、洗牌拖尾。
    public static readonly Color SupportColor = new(0.9059f, 0.6f, 0.6902f);

    private const string SceneRoot = $"{Entry.ResPath}/scenes/characters";
    private const string ImageRoot = $"{Entry.ResPath}/images/characters";

    private const string CharacterIconScenePath = $"{SceneRoot}/Diana/Diana_character_icon.tscn";
    private const string CharacterSelectBgScenePath = $"{SceneRoot}/Diana/Diana_character_select_bg.tscn";
    private const string EnergyCounterScenePath = $"{SceneRoot}/Diana/Diana_energy_counter.tscn";
    private const string MerchantScenePath = $"{SceneRoot}/Diana/Diana_merchant.tscn";
    private const string RestSiteScenePath = $"{SceneRoot}/Diana/Diana_rest_site.tscn";

    // 角色名称颜色。
    public override Color NameColor => ThemeColor;
    // 能量数字描边色：主题色的深色版 #3D1726，保证粉色能量盘上的数字可读。
    public override Color EnergyLabelOutlineColor => new(0.24f, 0.09f, 0.15f);
    // 地图绘制颜色。
    public override Color MapDrawingColor => SupportColor;

    public override CharacterGender Gender => CharacterGender.Feminine;

    public override int StartingHp => 92;
    public override int StartingGold => 99;

    // 只写需要替换的类别，未写的字段会被 RitsuLib 从占位角色配置里补齐。
    public override CharacterAssetProfile AssetProfile => new(
        Scenes: new CharacterSceneAssetSet(
            // TODO(美术)：VisualsPath（战斗人物模型）暂缺，先回退占位角色（ironclad）。
            // 补图时把 Bella_character.tscn 那份逐帧场景复制成 Diana_character.tscn 并填回这里。
            EnergyCounterPath: EnergyCounterScenePath,
            MerchantAnimPath: MerchantScenePath,
            RestSiteAnimPath: RestSiteScenePath),
        Ui: new CharacterUiAssetSet(
            IconTexturePath: $"{ImageRoot}/Diana/Diana_character_icon.png",
            IconPath: CharacterIconScenePath,
            IconOutlineTexturePath: $"{ImageRoot}/Diana/Diana_character_icon_outline.png",
            CharacterSelectBgPath: CharacterSelectBgScenePath,
            CharacterSelectIconPath: $"{ImageRoot}/Diana/Diana_character_select.png",
            CharacterSelectLockedIconPath: $"{ImageRoot}/Diana/Diana_character_select_locked.png",
            MapMarkerPath: $"{ImageRoot}/Diana/Diana_map_icon.png"),
        Vfx: new CharacterVfxAssetSet(
            // 拖尾沿用占位角色的场景，颜色相乘染成嘉然当前应援色。
            TrailStyle: new CharacterTrailStyle(
                OuterTrailModulate: SupportColor,
                InnerTrailModulate: SupportColor,
                BigSparksColor: SupportColor,
                LittleSparksColor: SupportColor,
                PrimarySpriteModulate: SupportColor,
                SecondarySpriteModulate: SupportColor)),
        // 多人模式出拳/石头/布/剪刀的手臂贴图。
        Multiplayer: new CharacterMultiplayerAssetSet(
            ArmPointingTexturePath: $"{ImageRoot}/Diana/multiplayer_hand_diana_point.png",
            ArmRockTexturePath: $"{ImageRoot}/Diana/multiplayer_hand_diana_rock.png",
            ArmPaperTexturePath: $"{ImageRoot}/Diana/multiplayer_hand_diana_paper.png",
            ArmScissorsTexturePath: $"{ImageRoot}/Diana/multiplayer_hand_diana_scissors.png"));

    public override string? PlaceholderCharacterId => "ironclad";
    public override bool RequiresEpochAndTimeline => false;
    public override float AttackAnimDelay => 0f;
    public override float CastAnimDelay => 0f;

    // 建筑师（Architect）战里这个人物的攻击特效列表，暂与贝拉一致。
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
