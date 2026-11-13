using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Relics;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using Zhijiang.ZhijiangCode.Characters.Diana;
using Zhijiang.ZhijiangCode.Keywords;
using Zhijiang.ZhijiangCode.Relics.Diana;

namespace Zhijiang.ZhijiangCode.Ui;

/// <summary>
/// 「王牌花色」四选一界面：战斗第一回合抽完牌后弹出。
///
/// 动效：4 张卡从嘉心糖遗物图标的位置依次飞出展开（带残影拖尾）；选中后缩小飞回遗物位置，其余三张淡出。
/// 卡面图：Zhijiang/images/ui/suit_choice/{spade,heart,diamond,club}.png（848×1264，只要 1 态）。
/// </summary>
public static class DianaSuitChoicePanel
{
    private const string LocTable = "relics";
    private const string LocPrefix = "ZHIJIANG_RELIC_JIA_XIN_TANG";

    private const float CardWidth = 300f;
    private const float CardHeight = 447f;
    private const float CardGap = 28f;

    // 节奏：慢一点，飞入/飞回都看得清。
    private const float IntroDuration = 0.6f;
    private const float IntroStagger = 0.12f;
    private const float ExitDuration = 0.6f;
    private const float BackdropFade = 0.45f;

    // 残影拖尾：每隔 GhostInterval 秒留一张半透明残影，GhostLife 秒内淡出。
    private const float GhostInterval = 0.035f;
    private const float GhostLife = 0.3f;
    private const float GhostAlpha = 0.26f;

    private static readonly (PokerSuit Suit, string File, string LocSuffix)[] Options =
    [
        (PokerSuit.Spade, "spade", "suit_spade"),
        (PokerSuit.Heart, "heart", "suit_heart"),
        (PokerSuit.Diamond, "diamond", "suit_diamond"),
        (PokerSuit.Club, "club", "suit_club"),
    ];

    /// <summary>弹出选择界面并等待玩家点选；无法弹出时返回 null。</summary>
    public static async Task<PokerSuit?> ShowAsync(Player player)
    {
        if (NCombatRoom.Instance is not { } room)
            return null;

        View view = Build(room, player);
        PokerSuit? chosen = await view.Result.Task;

        if (chosen is { } suit)
            DianaTrumpSuit.Set(suit);

        if (GodotObject.IsInstanceValid(view.Overlay))
            view.Overlay.QueueFree();

        return chosen;
    }

    private sealed class CardEntry
    {
        public PokerSuit Suit;
        public Control Card = null!;
        public TextureButton Button = null!;
        public Texture2D? Texture;
    }

    private sealed class View
    {
        public Control Overlay = null!;
        /// <summary>残影层：夹在黑幕和卡片之间，保证拖尾可见又不遮挡卡片。</summary>
        public Control GhostLayer = null!;
        public readonly List<CardEntry> Cards = [];
        public Vector2 AnchorLocal;
        public TaskCompletionSource<PokerSuit?> Result = new();
        public bool Picked;
    }

    private static View Build(Node room, Player player)
    {
        var view = new View { Overlay = new Control { Name = "DianaSuitChoice", MouseFilter = Control.MouseFilterEnum.Stop } };
        view.Overlay.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        room.AddChild(view.Overlay);

        var backdrop = new ColorRect
        {
            Color = new Color(0f, 0f, 0f, 0f),
            MouseFilter = Control.MouseFilterEnum.Stop,
        };
        backdrop.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        view.Overlay.AddChild(backdrop);
        view.Overlay.CreateTween().TweenProperty(backdrop, "color:a", 0.74f, BackdropFade);

        view.GhostLayer = new Control { Name = "DianaSuitGhosts", MouseFilter = Control.MouseFilterEnum.Ignore };
        view.GhostLayer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        view.Overlay.AddChild(view.GhostLayer);

        var hint = new Label
        {
            Text = ToBbcode(Localized($"{LocPrefix}.choose_hint")),
            HorizontalAlignment = HorizontalAlignment.Center,
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        hint.SetAnchorsPreset(Control.LayoutPreset.TopWide);
        hint.OffsetTop = 90f;
        hint.OffsetBottom = 146f;
        hint.AddThemeFontSizeOverride("font_size", 30);
        hint.AddThemeColorOverride("font_color", new Color(1f, 0.95f, 0.85f));
        hint.AddThemeColorOverride("font_outline_color", new Color(0f, 0f, 0f));
        hint.AddThemeConstantOverride("outline_size", 8);
        hint.Modulate = new Color(1f, 1f, 1f, 0f);
        view.Overlay.AddChild(hint);
        view.Overlay.CreateTween().TweenProperty(hint, "modulate:a", 1f, BackdropFade);

        Vector2 viewport = view.Overlay.GetViewportRect().Size;
        view.AnchorLocal = ResolveRelicAnchor(view.Overlay, player, viewport);

        float totalWidth = Options.Length * CardWidth + (Options.Length - 1) * CardGap;
        float startX = (viewport.X - totalWidth) * 0.5f;
        float slotY = (viewport.Y - CardHeight) * 0.5f;

        for (var i = 0; i < Options.Length; i++)
        {
            (PokerSuit suit, string file, string locSuffix) = Options[i];
            var slot = new Vector2(startX + i * (CardWidth + CardGap), slotY);
            CardEntry entry = BuildCard(suit, file, locSuffix, view);

            entry.Card.Position = view.AnchorLocal - new Vector2(CardWidth * 0.5f, CardHeight * 0.5f);
            entry.Card.Scale = new Vector2(0.18f, 0.18f);
            entry.Card.Modulate = new Color(1f, 1f, 1f, 0f);
            entry.Button.Disabled = true;

            view.Overlay.AddChild(entry.Card);
            view.Cards.Add(entry);

            float delay = i * IntroStagger;
            Tween tween = entry.Card.CreateTween().SetParallel(true);
            tween.TweenProperty(entry.Card, "position", slot, IntroDuration)
                .SetDelay(delay)
                .SetTrans(Tween.TransitionType.Back)
                .SetEase(Tween.EaseType.Out);
            tween.TweenProperty(entry.Card, "scale", Vector2.One, IntroDuration)
                .SetDelay(delay)
                .SetTrans(Tween.TransitionType.Back)
                .SetEase(Tween.EaseType.Out);
            tween.TweenProperty(entry.Card, "modulate:a", 1f, IntroDuration * 0.5f)
                .SetDelay(delay);

            _ = EmitGhosts(view, entry, delay, IntroDuration);
        }

        EnableCardsLater(view, IntroDuration + (Options.Length - 1) * IntroStagger);
        return view;
    }

    private static async void EnableCardsLater(View view, float delay)
    {
        await view.Overlay.ToSignal(view.Overlay.GetTree().CreateTimer(delay), SceneTreeTimer.SignalName.Timeout);
        if (!GodotObject.IsInstanceValid(view.Overlay))
            return;

        foreach (CardEntry entry in view.Cards)
            if (GodotObject.IsInstanceValid(entry.Button))
                entry.Button.Disabled = false;
    }

    private static CardEntry BuildCard(PokerSuit suit, string file, string locSuffix, View view)
    {
        var card = new Control
        {
            CustomMinimumSize = new Vector2(CardWidth, CardHeight),
            PivotOffset = new Vector2(CardWidth * 0.5f, CardHeight * 0.5f),
            MouseFilter = Control.MouseFilterEnum.Pass,
        };

        var texture = ResourceLoader.Load<Texture2D>($"{Entry.ResPath}/images/ui/suit_choice/{file}.png");

        var button = new TextureButton
        {
            TextureNormal = texture,
            IgnoreTextureSize = true,
            StretchMode = TextureButton.StretchModeEnum.Scale,
            MouseFilter = Control.MouseFilterEnum.Stop,
        };
        button.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        card.AddChild(button);

        var text = new RichTextLabel
        {
            BbcodeEnabled = true,
            ScrollActive = false,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            Text = BuildCardText(locSuffix),
        };
        text.Position = new Vector2(CardWidth * 0.08f, CardHeight * 0.545f);
        text.Size = new Vector2(CardWidth * 0.84f, CardHeight * 0.34f);
        text.AddThemeFontSizeOverride("normal_font_size", 17);
        card.AddChild(text);

        var entry = new CardEntry { Suit = suit, Card = card, Button = button, Texture = texture };

        button.MouseEntered += () =>
        {
            if (!view.Picked)
                TweenScale(card, 1.04f);
        };
        button.MouseExited += () =>
        {
            if (!view.Picked)
                TweenScale(card, 1f);
        };
        button.ButtonDown += () =>
        {
            if (!view.Picked)
                TweenScale(card, 0.97f);
        };
        button.Pressed += () => Pick(view, suit);

        return entry;
    }

    private static async void Pick(View view, PokerSuit suit)
    {
        if (view.Picked)
            return;

        view.Picked = true;

        foreach (CardEntry entry in view.Cards)
            if (GodotObject.IsInstanceValid(entry.Button))
                entry.Button.Disabled = true;

        foreach (CardEntry entry in view.Cards)
        {
            if (!GodotObject.IsInstanceValid(entry.Card))
                continue;

            Tween tween = entry.Card.CreateTween().SetParallel(true);

            if (entry.Suit == suit)
            {
                tween.TweenProperty(entry.Card, "position",
                        view.AnchorLocal - new Vector2(CardWidth * 0.5f, CardHeight * 0.5f), ExitDuration)
                    .SetTrans(Tween.TransitionType.Back)
                    .SetEase(Tween.EaseType.In);
                tween.TweenProperty(entry.Card, "scale", new Vector2(0.18f, 0.18f), ExitDuration);
                tween.TweenProperty(entry.Card, "modulate:a", 0f, ExitDuration);
                _ = EmitGhosts(view, entry, 0f, ExitDuration);
            }
            else
            {
                tween.TweenProperty(entry.Card, "modulate:a", 0f, ExitDuration * 0.6f);
                tween.TweenProperty(entry.Card, "scale", new Vector2(0.9f, 0.9f), ExitDuration * 0.6f);
            }
        }

        // 用计时器而不是 Tween.Finished 收尾：即使动画被打断，回合流程也不会卡住。
        if (GodotObject.IsInstanceValid(view.Overlay))
            await view.Overlay.ToSignal(
                view.Overlay.GetTree().CreateTimer(ExitDuration + 0.05f), SceneTreeTimer.SignalName.Timeout);

        view.Result.TrySetResult(suit);
    }

    /// <summary>
    /// 残影拖尾：在 duration 秒内每隔 GhostInterval 留一张半透明残影，残影自己淡出后释放。
    /// 不用原版的 NCardTrailVfx：它会接管被跟随节点的 modulate，和我们的淡入淡出打架。
    /// </summary>
    private static async Task EmitGhosts(View view, CardEntry entry, float delay, float duration)
    {
        if (delay > 0f)
        {
            if (!GodotObject.IsInstanceValid(view.Overlay))
                return;

            await view.Overlay.ToSignal(
                view.Overlay.GetTree().CreateTimer(delay), SceneTreeTimer.SignalName.Timeout);
        }

        var elapsed = 0f;
        while (elapsed < duration)
        {
            if (!GodotObject.IsInstanceValid(view.Overlay) || !GodotObject.IsInstanceValid(entry.Card))
                return;

            SpawnGhost(view, entry.Card, entry.Texture);

            await view.Overlay.ToSignal(
                view.Overlay.GetTree().CreateTimer(GhostInterval), SceneTreeTimer.SignalName.Timeout);
            elapsed += GhostInterval;
        }
    }

    private static void SpawnGhost(View view, Control card, Texture2D? texture)
    {
        if (texture is null || !GodotObject.IsInstanceValid(view.GhostLayer))
            return;

        var ghost = new TextureRect
        {
            Texture = texture,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            Position = card.Position,
            Size = new Vector2(CardWidth, CardHeight),
            PivotOffset = new Vector2(CardWidth * 0.5f, CardHeight * 0.5f),
            Scale = card.Scale,
            Modulate = new Color(1f, 1f, 1f, GhostAlpha),
        };
        view.GhostLayer.AddChild(ghost);

        Tween tween = ghost.CreateTween().SetParallel(true);
        tween.TweenProperty(ghost, "modulate:a", 0f, GhostLife);
        tween.TweenProperty(ghost, "scale", card.Scale * 1.06f, GhostLife);
        tween.TweenCallback(Callable.From(ghost.QueueFree)).SetDelay(GhostLife);
    }

    /// <summary>嘉心糖遗物图标的屏幕位置（找不到时退回屏幕顶部中间）。</summary>
    private static Vector2 ResolveRelicAnchor(Control overlay, Player player, Vector2 viewport)
    {
        Vector2 fallback = new(viewport.X * 0.5f, 80f);

        if (NRun.Instance is not { } run)
            return fallback;

        NRelicInventory? inventory = run.GlobalUi?.RelicInventory;
        if (inventory is null)
            return fallback;

        foreach (NRelicInventoryHolder holder in inventory.RelicNodes)
        {
            if (!GodotObject.IsInstanceValid(holder))
                continue;

            RelicModel? model = holder.Relic?.Model;
            if (model is not DianaStarterRelicTemplate)
                continue;

            if (!player.Relics.Contains(model))
                continue;

            return holder.GlobalPosition + holder.Size * 0.5f - overlay.GlobalPosition;
        }

        return fallback;
    }

    private static string BuildCardText(string locSuffix)
    {
        string title = Localized($"{LocPrefix}.{locSuffix}.title");

        var description = new LocString(LocTable, $"{LocPrefix}.{locSuffix}.description");
        description.Add("Count", DianaTrumpSuit.Threshold);

        return $"[font_size=26]{ToBbcode($"[gold]{title}[/gold]")}[/font_size]\n\n{ToBbcode(description.GetFormattedText())}";
    }

    private static string Localized(string key)
    {
        return new LocString(LocTable, key).GetFormattedText();
    }

    /// <summary>
    /// 把本地化文本里的游戏自定义标签（[gold] 等）转成 Godot 原生 BBCode。
    /// 游戏的 gold 是自定义 RichTextEffect，只有它自己的标签控件认；
    /// 我们自建的 RichTextLabel 没装那些效果，直接换成等效颜色标签。
    /// </summary>
    private static string ToBbcode(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        return text
            .Replace("[gold]", $"[color=#{StsColors.gold.ToHtml(false)}]")
            .Replace("[/gold]", "[/color]")
            .Replace("[pink]", $"[color=#{StsColors.pink.ToHtml(false)}]")
            .Replace("[/pink]", "[/color]");
    }

    private static void TweenScale(Control node, float target)
    {
        if (!GodotObject.IsInstanceValid(node))
            return;

        node.CreateTween()
            .TweenProperty(node, "scale", new Vector2(target, target), 0.1f)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.Out);
    }
}
