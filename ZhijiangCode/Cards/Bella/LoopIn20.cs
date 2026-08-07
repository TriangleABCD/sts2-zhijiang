using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Zhijiang.ZhijiangCode.Characters.Bella;
using Zhijiang.ZhijiangCode.Powers;
using Zhijiang.ZhijiangCode.SecondResource;

namespace Zhijiang.ZhijiangCode.Cards.Bella;

[RegisterCard(typeof(BellaCardPool))]
public sealed class LoopIn20 : ModCardTemplate
{
    private const int BaseEnergyCost = 3;
    private const int BaseHeartWallCost = 150;
    private const int UpgradedHeartWallCost = 120;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Rare;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/loop_in_20.png");

    // 消耗。
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    public LoopIn20() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
        // 消耗 150 点心之壁。
        this.SecondaryCosts().Set(HeartWall.HeartWallId, BaseHeartWallCost);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 施加能力：下一张攻击牌额外打出 20 次。
        await PowerCmd.Apply<LoopIn20Power>(choiceContext, base.Owner.Creature,
            1, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // 耗能 3 → 2。
        base.EnergyCost.UpgradeBy(-1);
        // 心之壁消耗 150 → 120。
        this.SecondaryCosts().Set(HeartWall.HeartWallId, UpgradedHeartWallCost);
    }
}
