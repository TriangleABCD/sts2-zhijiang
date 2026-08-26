using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Zhijiang.ZhijiangCode.Characters.Bella;
using STS2RitsuLib.Keywords;

namespace Zhijiang.ZhijiangCode.Cards.Bella;

// 长路归航：稀有牌（中立 / 技能）。消耗。将所有 0 费牌加入手牌并升级；将手牌中所有 1 费牌费用减至 0（本回合）。
[RegisterCard(typeof(BellaCardPool))]
public sealed class LongRoadHome : ModCardTemplate
{
    private const int BaseEnergyCost = 3;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Rare;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;

    // 卡图占位：指向尚不存在的资源（构建会有 RITSU013 警告，方便日后找出缺卡图的牌）。
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/long_road_home.png");

    // 消耗。
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    public LongRoadHome() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        List<CardModel> toHandle = new();

        // 抽牌堆 + 弃牌堆中的 0 费牌加入手牌。
        foreach (CardPile pile in new[] { PileType.Draw.GetPile(base.Owner), PileType.Discard.GetPile(base.Owner) })
        {
            toHandle.AddRange(pile.Cards.Where(IsZeroCost));
        }
        if (toHandle.Count > 0)
        {
            await CardPileCmd.Add(toHandle, PileType.Hand);
        }

        // 手牌中所有 0 费牌升级。
        List<CardModel> zeroCostInHand = PileType.Hand.GetPile(base.Owner).Cards.Where(IsZeroCost).ToList();
        if (zeroCostInHand.Count > 0)
            CardCmd.Upgrade(zeroCostInHand, CardPreviewStyle.HorizontalLayout);

        // 手牌中所有 1 费牌本回合费用减至 0。
        foreach (CardModel card in PileType.Hand.GetPile(base.Owner).Cards)
        {
            if (card.Type is not (CardType.Attack or CardType.Skill or CardType.Power))
                continue;
            if (card.EnergyCost.CostsX)
                continue;
            if (card.EnergyCost.GetWithModifiers(CostModifiers.All) == 1)
                card.EnergyCost.SetThisTurnOrUntilPlayed(0);
        }
    }

    private static bool IsZeroCost(CardModel card)
    {
        if (card.Type is not (CardType.Attack or CardType.Skill or CardType.Power))
            return false;
        if (card.EnergyCost.CostsX)
            return false;
        return card.EnergyCost.GetWithModifiers(CostModifiers.All) == 0;
    }

    protected override void OnUpgrade()
    {
        // 升级：移除消耗。
        RemoveKeyword(CardKeyword.Exhaust);
    }
}