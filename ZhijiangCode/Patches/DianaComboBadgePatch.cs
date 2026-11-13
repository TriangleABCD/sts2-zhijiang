using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib.Patching.Models;
using Zhijiang.ZhijiangCode.Cards.Diana;

namespace Zhijiang.ZhijiangCode.Patches;

/// <summary>
/// 在战斗手牌卡面上画组合角标（① ② ③ …），挂在卡牌自己的 %OverlayContainer 上。
///
/// 卡面布局（card.tscn 实测）：能量宝石在左上、星标在其下方、附魔在左侧中段、
/// 标题横幅在顶部、类型牌在底部中央、描述占下半部 —— 右上角是空的，所以角标放右上。
/// 只有 NHandCardHolder（手牌）才画，牌堆视图/奖励界面不会出现。
/// </summary>
internal static class DianaComboBadge
{
    private const string BadgeNodeName = "DianaComboBadge";
    private static readonly Vector2 BadgePosition = new(88, -234);
    private static readonly Vector2 BadgeSize = new(56, 44);

    public static void Apply(NCard? card, CardModel? model)
    {
        if (card is null || model is null || !GodotObject.IsInstanceValid(card))
            return;

        if (card.GetNodeOrNull<Control>("%OverlayContainer") is not { } overlay)
            return;

        Label? badge = overlay.GetNodeOrNull<Label>(BadgeNodeName);
        bool show = DianaComboHighlight.TryGet(model, out DianaComboHighlight.Entry entry);

        if (!show)
        {
            if (badge is not null)
                badge.Visible = false;
            return;
        }

        if (badge is null)
        {
            badge = new Label
            {
                Name = BadgeNodeName,
                MouseFilter = Control.MouseFilterEnum.Ignore,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            badge.AddThemeFontSizeOverride("font_size", 30);
            badge.AddThemeColorOverride("font_color", new Color(1f, 1f, 1f));
            badge.AddThemeColorOverride("font_outline_color", new Color(0f, 0f, 0f));
            badge.AddThemeConstantOverride("outline_size", 8);
            overlay.AddChild(badge);
        }

        badge.Text = GroupGlyph(entry.GroupIndex);
        badge.Position = BadgePosition;
        badge.Size = BadgeSize;
        badge.Visible = true;
    }

    /// <summary>1–20 → ①②③…；超出范围退回普通数字。</summary>
    private static string GroupGlyph(int index)
    {
        return index is >= 1 and <= 20
            ? char.ConvertFromUtf32(0x2460 + index - 1)
            : index.ToString();
    }
}

/// <summary>手牌卡牌重绘时同步组合角标。</summary>
public sealed class DianaComboBadgePatch : IPatchMethod
{
    public static string PatchId => "zhijiang_diana_combo_badge";

    public static string Description => "Draw Diana's poker combo group badge on hand cards";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets()
    {
        return [new(typeof(NHandCardHolder), nameof(NHandCardHolder.UpdateCard), [])];
    }

    public static void Postfix(NHandCardHolder __instance)
    {
        DianaComboBadge.Apply(__instance.CardNode, __instance.CardModel);
    }
}

/// <summary>
/// 手牌布局刷新（每次加牌/移除/悬停都会走到这里）时重算组合，再刷新所有手牌角标。
/// 这是"手牌一变化就重算"的唯一触发点，不额外挂事件。
/// </summary>
public sealed class DianaComboHandRefreshPatch : IPatchMethod
{
    public static string PatchId => "zhijiang_diana_combo_hand_refresh";

    public static string Description => "Recompute Diana's poker combos whenever the hand layout refreshes";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets()
    {
        return [new(typeof(NPlayerHand), "RefreshLayout", [])];
    }

    public static void Postfix(NPlayerHand __instance)
    {
        DianaComboHighlight.Recompute();

        foreach (NHandCardHolder holder in __instance.ActiveHolders)
            DianaComboBadge.Apply(holder.CardNode, holder.CardModel);

        // 战斗里第一次刷新手牌时把两个组合按钮建出来（幂等）。
        DianaComboButtons.Ensure();
    }
}
