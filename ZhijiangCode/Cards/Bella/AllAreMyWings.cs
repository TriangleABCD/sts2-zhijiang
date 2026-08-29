using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Zhijiang.ZhijiangCode.Powers;
using Zhijiang.ZhijiangCode.Characters.Bella;

namespace Zhijiang.ZhijiangCode.Cards.Bella;

// 都是我的翅膀：罕见牌（中立 / 能力）。若玩家本回合打出的阳牌数等于阴牌数，
// 下回合获得 1→2 点能量。
[RegisterCard(typeof(BellaCardPool))]
public sealed class AllAreMyWings : ModCardTemplate
{
    private const int BaseEnergyCost = 2;
    private const CardType CardKind = CardType.Power;
    private const CardRarity CardRarityValue = CardRarity.Uncommon;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;

    // 卡图占位：暂用通用技能牌卡图。
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/all_are_my_wings.png");

    // 奖励能量数（1 → 升级后 2）。
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar(1)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        base.EnergyHoverTip
    ];

    // 中立牌：不挂阳/阴关键词。
    public override IEnumerable<CardKeyword> CanonicalKeywords => [];

    public AllAreMyWings() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "PowerUp", base.Owner.Character.PowerUpAnimDelay);
        int reward = DynamicVars.Energy.IntValue;
        await PowerCmd.Apply<AllAreMyWingsPower>(choiceContext, base.Owner.Creature, reward, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // 奖励 3 → 4。
        DynamicVars.Energy.UpgradeValueBy(1m);
    }
}