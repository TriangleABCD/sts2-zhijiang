using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Zhijiang.ZhijiangCode.Characters.Bella;
using Zhijiang.ZhijiangCode.Powers;

namespace Zhijiang.ZhijiangCode.Cards.Bella;

// 队长的权力是无限的：稀有牌（中立 / 能力）。每回合开始时，你可以丢弃一张牌；若弃的是技能牌，抽 1→2 张牌。
[RegisterCard(typeof(BellaCardPool))]
public sealed class CaptainsPowerIsInfinite : ModCardTemplate
{
    private const int BaseEnergyCost = 2;
    private const CardType CardKind = CardType.Power;
    private const CardRarity CardRarityValue = CardRarity.Rare;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;

    // 卡图占位：指向尚不存在的资源（构建会有 RITSU013 警告，方便日后找出缺卡图的牌）。
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/captains_power_is_infinite.png");

    // 抽牌数（1 → 升级后 2）。
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Draw", 1m)
    ];

    // 中立牌：不挂阳/阴关键词。
    public override IEnumerable<CardKeyword> CanonicalKeywords => [];

    public CaptainsPowerIsInfinite() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int draw = DynamicVars["Draw"].IntValue;
        await PowerCmd.Apply<CaptainsPowerIsInfinitePower>(choiceContext, base.Owner.Creature, draw, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // 抽牌 1 → 2。
        DynamicVars["Draw"].UpgradeValueBy(1m);
    }
}