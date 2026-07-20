using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Zhijiang.ZhijiangCode.Characters.Bella;
using Zhijiang.ZhijiangCode.SecondResource;

namespace Zhijiang.ZhijiangCode.Cards.Bella;

[RegisterCard(typeof(BellaCardPool))]
[RegisterCharacterStarterCard(typeof(BellaCharacter), 1)]
public sealed class PreventionShot : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Basic;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/Characters/Bella/Bella_energy_text.png");

    // 规范值：格挡、心之壁、力量损失。
    // {Block:diff()} 显示格挡值，{HeartWallGain} 显示心之壁增益，{StrengthLoss} 显示力量损失。
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(10m, ValueProp.Move),
        new DynamicVar("HeartWallGain", 5m),
        new DynamicVar("StrengthLoss", 1m)
    ];

    // 力量悬浮提示。
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<StrengthPower>()
    ];

    public PreventionShot() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 获得 5 点心之壁。
        await SecondaryResourceCmd.Gain(base.Owner, HeartWall.HeartWallId,
            (int)DynamicVars["HeartWallGain"].BaseValue, this);

        // 获得 10 点格挡。
        await CreatureCmd.GainBlock(base.Owner.Creature, DynamicVars.Block, cardPlay);

        // 失去 1 点力量。
        await PowerCmd.Apply<StrengthPower>(choiceContext, base.Owner.Creature,
            -DynamicVars["StrengthLoss"].BaseValue, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m);
        DynamicVars["HeartWallGain"].UpgradeValueBy(5m);
    }
}
