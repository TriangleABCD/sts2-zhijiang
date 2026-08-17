using System.Linq;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using STS2RitsuLib.Patching.Models;
using Zhijiang.ZhijiangCode.Characters.Bella;

namespace Zhijiang.ZhijiangCode.Patches;

/// <summary>
/// 贝拉洗牌拖尾染色补丁。
/// 抽牌堆耗尽时弃牌堆洗回抽牌堆的飞行特效（NCardFlyShuffleVfx）内部调用
/// NCardTrailVfx.Create(this, trailPath) 时，跟随节点是洗牌特效本身而不是 NCard，
/// 因此 RitsuLib 的 CharacterTrailStyleOverridePatch（只处理 NCard，从卡片反查角色）
/// 不会给它染色，贝拉的洗牌拖尾一直显示占位角色（ironclad 战士）的原始红色。
/// 本补丁在 NCardTrailVfx.Create 之后拦截：若跟随节点是洗牌特效，通过其目标牌堆
/// （_targetPile，即洗牌玩家的牌堆实例）在当前战斗中反查贝拉玩家，
/// 按贝拉应援色（SupportColor #DB7D74）染色，与"手牌进弃牌堆"的拖尾完全一致。
/// </summary>
public sealed class BellaShuffleTrailStylePatch : IPatchMethod
{
    private static readonly AccessTools.FieldRef<NCardFlyShuffleVfx, CardPile> TargetPileRef =
        AccessTools.FieldRefAccess<NCardFlyShuffleVfx, CardPile>("_targetPile");

    public static string PatchId => "zhijiang_bella_shuffle_trail_style";

    public static string Description => "Color Bella's discard-to-draw shuffle trail like her card trail";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets()
    {
        return [new(typeof(NCardTrailVfx), nameof(NCardTrailVfx.Create), [typeof(Control), typeof(string)])];
    }

    public static void Postfix(Control card, ref NCardTrailVfx? __result)
    {
        // 只处理洗牌特效（手牌进弃牌堆等普通飞行拖尾由 RitsuLib 的补丁负责）。
        if (__result == null || card is not NCardFlyShuffleVfx shuffleVfx)
            return;

        CardPile? targetPile = TargetPileRef(shuffleVfx);
        CombatState? state = CombatManager.Instance.DebugOnlyGetState();
        if (targetPile == null || state == null)
            return;

        // 洗牌特效的目标牌堆是洗牌玩家拥有的牌堆实例（正常洗牌即抽牌堆），
        // 用引用相等反查出这个牌堆属于哪个贝拉玩家。
        foreach (var player in state.Players)
        {
            if (player.Character is not BellaCharacter)
                continue;
            if (player.PlayerCombatState is not { } combatState)
                continue;
            if (!combatState.AllPiles.Any(pile => ReferenceEquals(pile, targetPile)))
                continue;

            ApplyBellaStyle(__result);
            return;
        }
    }

    /// <summary>
    /// 与 BellaCharacter.TrailStyle 完全一致的染色方案：
    /// 缎带/剪影精灵/大小火花全部染成 SupportColor（#DB7D74），不覆盖宽度与缩放。
    /// </summary>
    private static void ApplyBellaStyle(NCardTrailVfx trail)
    {
        Color color = BellaCharacter.SupportColor;
        ApplyLineStyle(trail, "Trails/OuterTrail", color, null);
        ApplyLineStyle(trail, "Trails/InnerTrail", color, null);
        ApplyParticleColor(trail, "Sprites/BigSparks", color);
        ApplyParticleColor(trail, "Sprites/LittleSparks", color);
        ApplySpriteStyle(trail, "Sprites/Sprite2D2", color, null);
        ApplySpriteStyle(trail, "Sprites/Sprite2D3", color, null);
    }

    private static void ApplyLineStyle(Node root, string nodePath, Color? modulate, float? width)
    {
        if (root.GetNodeOrNull<Line2D>(nodePath) is not { } line)
            return;

        if (modulate.HasValue)
            line.Modulate = modulate.Value;

        if (width.HasValue)
            line.Width = width.Value;
    }

    private static void ApplyParticleColor(Node root, string nodePath, Color? color)
    {
        if (!color.HasValue)
            return;

        if (root.GetNodeOrNull<CpuParticles2D>(nodePath) is { } particles)
            particles.Color = color.Value;
    }

    private static void ApplySpriteStyle(Node root, string nodePath, Color? modulate, Vector2? scale)
    {
        if (root.GetNodeOrNull<Sprite2D>(nodePath) is not { } sprite)
            return;

        if (modulate.HasValue)
            sprite.Modulate = modulate.Value;

        if (scale.HasValue)
            sprite.Scale = scale.Value;
    }
}
