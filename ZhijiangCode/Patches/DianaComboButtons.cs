using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using Zhijiang.ZhijiangCode.Cards.Diana;

namespace Zhijiang.ZhijiangCode.Patches;

/// <summary>
/// 战斗界面左下角的两个组合按钮：「同点」「顺子」。
/// 点一次高亮本族全部组合（再点一次取消），两模式互斥 —— 由 <see cref="DianaComboHighlight.Toggle" /> 负责。
///
/// 位置先给一个大致值（左下角往上 132px、往右 48px），后续按实际观感微调 <see cref="PanelOffset" />。
/// 挂在战斗 UI 根节点（全屏 Control）上，避免依赖手牌节点自己的锚点。
/// </summary>
internal static class DianaComboButtons
{
    private const string RootName = "DianaComboButtons";
    private const int FontSize = 22;

    // 距屏幕左下角的偏移：x 向右、y 向上（负值）。
    private static readonly Vector2 PanelOffset = new(48, -132);
    private static readonly Vector2 ButtonSize = new(104, 40);

    private static readonly List<(Button Button, DianaComboView View)> Buttons = [];

    public static void Ensure()
    {
        if (NCombatRoom.Instance?.Ui is not Control ui)
            return;

        if (ui.GetNodeOrNull<Control>(RootName) is { } existing)
        {
            SyncAppearance();
            return;
        }

        var row = new HBoxContainer
        {
            Name = RootName,
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        row.SetAnchorsPreset(Control.LayoutPreset.BottomLeft);
        row.Position = PanelOffset;
        row.AddThemeConstantOverride("separation", 8);
        ui.AddChild(row);

        Buttons.Clear();
        Buttons.Add((CreateButton(row, "同点", DianaComboView.SameRank), DianaComboView.SameRank));
        Buttons.Add((CreateButton(row, "顺子", DianaComboView.Straight), DianaComboView.Straight));

        SyncAppearance();
    }

    private static Button CreateButton(Node parent, string text, DianaComboView view)
    {
        var button = new Button
        {
            Text = text,
            CustomMinimumSize = ButtonSize,
            FocusMode = Control.FocusModeEnum.None,
        };
        button.AddThemeFontSizeOverride("font_size", FontSize);
        button.Pressed += () =>
        {
            DianaComboHighlight.Toggle(view);
            RecomputeAndRefreshBadges();
            SyncAppearance();
        };
        parent.AddChild(button);
        return button;
    }

    /// <summary>当前激活的按钮显示为纯白、未激活的压暗。</summary>
    private static void SyncAppearance()
    {
        foreach ((Button button, DianaComboView view) in Buttons)
        {
            if (!GodotObject.IsInstanceValid(button))
                continue;

            bool active = DianaComboHighlight.View == view;
            button.Modulate = active ? Colors.White : new Color(0.72f, 0.72f, 0.72f);
        }
    }

    /// <summary>按钮改变显示模式后立即重算并刷新手牌角标（不等下一次手牌变化）。</summary>
    public static void RecomputeAndRefreshBadges()
    {
        DianaComboHighlight.Recompute();

        if (NPlayerHand.Instance is not { } hand)
            return;

        foreach (NHandCardHolder holder in hand.ActiveHolders)
            DianaComboBadge.Apply(holder.CardNode, holder.CardModel);
    }
}
