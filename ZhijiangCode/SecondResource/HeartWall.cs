using Godot;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Cards;
using STS2RitsuLib;
using STS2RitsuLib.Combat.SecondaryResources;

using Zhijiang.ZhijiangCode.Characters.Bella;

namespace Zhijiang.ZhijiangCode.SecondResource;
public static class HeartWall
{
	public static SecondaryResourceDefinition HeartWallDefinition { get; private set; } = null!;
    public static string HeartWallId { get; private set; } = string.Empty;

	public static void Register()
	{
		var registry = RitsuLibFramework.GetSecondaryResourceRegistry(Entry.ModId);

		// 注册次级资源
		HeartWallDefinition = registry.Register("HeartWall", new SecondaryResourceDefinition(
			defaultAmount: 10,
			baseMaxAmount: null,
			turnStartPolicy: SecondaryResourceTurnStartPolicy.None,
			persistencePolicy: SecondaryResourcePersistencePolicy.Combat,
			smallIconPath: $"{Entry.ResPath}/images/characters/energy_text.png",
			largeIconPath: $"{Entry.ResPath}/images/characters/energy_big.png"
		));

		HeartWallId = HeartWallDefinition.Id;

		// 注册战斗界面计数器
		registry.RegisterCombatUi(
			"heart_wall_combat_counter",
			parent =>
			{
				var row = NSecondaryResourceCounter.Create(HeartWallDefinition, new SecondaryResourceCounterStyle
				{
					FontSize = 32,
					PositiveColor = Colors.Cyan,
					FormatAmount = (amount, max) => amount.ToString(),
					IconStyle = SecondaryResourceIconStyle.Default with
					{
						Size = new Vector2(80, 80),
						HoverTip = SecondaryResourceHoverTipStyle.Default,
					},
				});
				// 自由指定位置。例如这里我们找到能量计数器的位置，放在它旁边
				var energyCounter = parent.GetNode<Control>("%EnergyCounterContainer");
				row.Position = energyCounter.Position + new Vector2(120, -120);
				return row;
			},
			ctx => ctx.Node.Bind(ctx.Player)
		);

		// 注册卡牌上显示次级资源消耗
		registry.RegisterCardUi(
    		"heart_wall_card_ui",
    		parent =>
    		{
        		var ui = NSecondaryResourceCardCostUi.Create(HeartWallId, new SecondaryResourceCardCostUiStyle
        		{
            		IconSize = new Vector2(48, 48),
            		FontSize = 24,
        		});
        		// 自由指定位置。例如这里我们找到能量图标的位置，放在它旁边
        		var energyIcon = parent.GetNode<TextureRect>("%EnergyIcon");
        		ui.Position = energyIcon.Position + new Vector2(0, 80);
        		return ui;
    		},
    		ctx => ctx.Node.Refresh(ctx)
		);

		registry.AlwaysShowInCombatUiForCharacter<BellaCharacter>(HeartWallDefinition.LocalId);
	}

}