using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Zhijiang.ZhijiangCode.Characters.Bella;

namespace Zhijiang.ZhijiangCode.Cards.Bella;

// 一个女人满足不了我：罕见牌（中立 / 技能）。抽牌，将手牌洗回抽牌堆，0 费牌洗回时获得格挡。
[RegisterCard(typeof(BellaCardPool))]
public sealed class OneWomanCantSatisfyMe : ModCardTemplate
{
    private const int BaseEnergyCost = 2;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Uncommon;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;

    // 卡图占位：指向尚不存在的资源（构建会有 RITSU013 警告，方便日后找出缺卡图的牌）。
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/one_woman_cant_satisfy_me.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(3),
        new DynamicVar("ShuffleCount", 3m),
        new DynamicVar("BlockPerZero", 4m)
    ];

    // 中立牌：不挂阳/阴关键词。
    public override IEnumerable<CardKeyword> CanonicalKeywords => [];

    public OneWomanCantSatisfyMe() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 抽 3 张牌。
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, base.Owner);

        // 随机挑最多 ShuffleCount 张手牌洗回抽牌堆。
        List<CardModel> hand = PileType.Hand.GetPile(base.Owner).Cards.ToList();
        int shuffleCount = Math.Min(DynamicVars["ShuffleCount"].IntValue, hand.Count);
        if (shuffleCount > 0)
        {
            List<CardModel> toShuffle = hand.TakeRandom(shuffleCount, base.Owner.RunState.Rng.CombatCardSelection).ToList();
            await CardPileCmd.Add(toShuffle, PileType.Draw, CardPilePosition.Random);

            // 每张 0 费牌获得 2 点格挡。
            int block = toShuffle.Count(IsZeroCost) * DynamicVars["BlockPerZero"].IntValue;
            if (block > 0)
                await CreatureCmd.GainBlock(base.Owner.Creature, block, ValueProp.Move, cardPlay);
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
        // 洗回数量 3 → 5。
        DynamicVars["ShuffleCount"].UpgradeValueBy(2m);
    }
}
