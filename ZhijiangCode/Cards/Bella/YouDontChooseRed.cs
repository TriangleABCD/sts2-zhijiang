using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Keywords;
using Zhijiang.ZhijiangCode.Characters.Bella;

namespace Zhijiang.ZhijiangCode.Cards.Bella;

// 你不选...红色：稀有牌（中立 / 技能）。将抽牌堆、弃牌堆、手牌中所有 0 费牌加入手牌并升级。
[RegisterCard(typeof(BellaCardPool))]
public sealed class YouDontChooseRed : ModCardTemplate
{
    private const int BaseEnergyCost = 2;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Rare;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;

    // 卡图占位：暂用通用技能牌卡图。
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/you_dont_choose_red.png");

    // 消耗。
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    public YouDontChooseRed() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 收集抽牌堆 + 弃牌堆中的 0 费牌，加入手牌。
        List<CardModel> draw = PileType.Draw.GetPile(base.Owner).Cards.Where(IsZeroCost).ToList();
        List<CardModel> discard = PileType.Discard.GetPile(base.Owner).Cards.Where(IsZeroCost).ToList();

        List<CardModel> toHand = draw.Concat(discard).ToList();
        IReadOnlyList<CardPileAddResult> results = toHand.Count > 0
            ? await CardPileCmd.Add(toHand, PileType.Hand)
            : [];
        if (LocalContext.IsMe(base.Owner))
            CardCmd.PreviewCardPileAdd(results);

        // 升级所有目标（含原本已在手牌的 0 费牌）。
        List<CardModel> toUpgrade = PileType.Hand.GetPile(base.Owner).Cards.Where(IsZeroCost).ToList();
        if (toUpgrade.Count > 0)
            CardCmd.Upgrade(toUpgrade, CardPreviewStyle.HorizontalLayout);
    }

    // 仅统计可打出的牌型（攻击/技能/能力），且当前耗能为 0（X 费用除外）。
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
        // 升级：移除「消耗」。
        RemoveKeyword(CardKeyword.Exhaust);
    }
}