using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Keywords;
using Zhijiang.ZhijiangCode.Characters.Bella;

namespace Zhijiang.ZhijiangCode.Cards.Bella;

// 分奴：罕见牌（阴 / 技能）。消耗你手中所有阴牌，每消耗一张抽 1→2 张牌。
[RegisterCard(typeof(BellaCardPool))]
public sealed class Tryhard : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Uncommon;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;

    // 卡图占位：暂用通用技能牌卡图。
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/tryhard.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("CardsPerYin", 1m)
    ];

    // 阴阳属性：分奴为阴牌。
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        BellaYinYangService.YinKeywordId.GetModCardKeyword()
    ];

    public Tryhard() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 收集手中所有阴牌。
        List<CardModel> yinCards = PileType.Hand.GetPile(base.Owner).Cards
            .Where(c => c.Keywords.Contains(BellaYinYangService.YinKeywordId.GetModCardKeyword()))
            .ToList();
        if (yinCards.Count == 0)
            return;

        // 全部消耗。
        foreach (CardModel card in yinCards)
            await CardCmd.Exhaust(choiceContext, card);

        // 每消耗一张抽 1→2 张。
        int draw = yinCards.Count * DynamicVars["CardsPerYin"].IntValue;
        await CardPileCmd.Draw(choiceContext, draw, base.Owner);
    }

    protected override void OnUpgrade()
    {
        // 每张阴牌抽牌 1 → 2。
        DynamicVars["CardsPerYin"].UpgradeValueBy(1m);
    }
}