using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Keywords;
using Zhijiang.ZhijiangCode.Characters.Bella;
using Zhijiang.ZhijiangCode.Powers;

namespace Zhijiang.ZhijiangCode.Cards.Bella;

// 喔拉姐，可靠的拉姐：稀有牌（阴 / 能力）。每当你失去心之壁，对随机敌人造成损失量 ÷ 5（升级后 ÷ 3）伤害。
[RegisterCard(typeof(BellaCardPool))]
public sealed class ReliableSisterLa : ModCardTemplate
{
    private const int BaseEnergyCost = 3;
    private const CardType CardKind = CardType.Power;
    private const CardRarity CardRarityValue = CardRarity.Rare;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;

    // 卡图占位：暂用通用技能牌卡图。
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/reliable_sister_la.png");

    // 除数（5 → 升级后 3）。
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Divisor", 5m)
    ];

    // 阴阳属性：喔拉姐，可靠的拉姐为阴牌。
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        BellaYinYangService.YinKeywordId.GetModCardKeyword()
    ];

    public ReliableSisterLa() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int divisor = DynamicVars["Divisor"].IntValue;
        await PowerCmd.Apply<ReliableSisterLaPower>(choiceContext, base.Owner.Creature, divisor, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // 除数 5 → 3。
        DynamicVars["Divisor"].UpgradeValueBy(-2m);
    }
}