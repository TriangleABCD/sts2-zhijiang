using Godot;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Cards;
using STS2RitsuLib;
using STS2RitsuLib.Combat.SecondaryResources;

using Zhijiang.ZhijiangCode.Characters.Diana;

namespace Zhijiang.ZhijiangCode.SecondResource;

/// <summary>
/// 嘉然的「心之壁」：规则与贝拉的完全相同（开局 5、无上限、回合开始不变化、战斗结束清零），
/// 只有图标用嘉然自己的资源。
///
/// 之所以单独注册一个次级资源，是因为 RitsuLib 的 <see cref="SecondaryResourceDefinition" />
/// 图标路径在注册期固定、没有按角色切图的钩子；注册成独立 id（Zhijiang.DianaHeartWall）后，
/// 嘉然以后换专属图标只改本文件，不会动到贝拉的 HeartWall。
/// </summary>
public static class DianaHeartWall
{
    public static SecondaryResourceDefinition Definition { get; private set; } = null!;
    public static string DianaHeartWallId { get; private set; } = string.Empty;

    public static void Register()
    {
        var registry = RitsuLibFramework.GetSecondaryResourceRegistry(Entry.ModId);

        Definition = registry.Register("DianaHeartWall", new SecondaryResourceDefinition(
            defaultAmount: 5,
            baseMaxAmount: null,
            turnStartPolicy: SecondaryResourceTurnStartPolicy.None,
            persistencePolicy: SecondaryResourcePersistencePolicy.Combat,
            smallIconPath: $"{Entry.ResPath}/images/characters/Diana/Diana_HeartWall_text.png",
            largeIconPath: $"{Entry.ResPath}/images/characters/Diana/Diana_HeartWall_big.png"
        ));

        DianaHeartWallId = Definition.Id;

        // 战斗界面计数器：位置与贝拉的心之壁一致。
        registry.RegisterCombatUi(
            "diana_heart_wall_combat_counter",
            parent =>
            {
                var row = NSecondaryResourceCounter.Create(Definition, new SecondaryResourceCounterStyle
                {
                    CounterSize = new Vector2(90, 90),
                    IconSize = new Vector2(90, 90),
                    FontSize = 28,
                    AmountLabelOffset = new Vector2(5, 15),
                    PositiveColor = Colors.White,
                    FormatAmount = (amount, max) => amount.ToString(),
                    IconStyle = SecondaryResourceIconStyle.Default with
                    {
                        Size = new Vector2(90, 90),
                        HoverTip = SecondaryResourceHoverTipStyle.Default,
                    },
                });
                var energyCounter = parent.GetNode<Control>("%EnergyCounterContainer");
                row.Position = energyCounter.Position + new Vector2(60, -100);
                return row;
            },
            ctx => ctx.Node.Bind(ctx.Player)
        );

        // 卡牌上的次级资源消耗角标。
        registry.RegisterCardUi(
            "diana_heart_wall_card_ui",
            parent =>
            {
                var ui = NSecondaryResourceCardCostUi.Create(DianaHeartWallId, new SecondaryResourceCardCostUiStyle
                {
                    IconSize = new Vector2(48, 48),
                    FontSize = 24,
                    LabelOffset = new Vector2(3, 8),
                });
                var energyIcon = parent.GetNode<TextureRect>("%EnergyIcon");
                ui.Position = energyIcon.Position + new Vector2(0, 80);
                return ui;
            },
            ctx => ctx.Node.Refresh(ctx)
        );

        registry.AlwaysShowInCombatUiForCharacter<DianaCharacter>(Definition.LocalId);
    }
}
