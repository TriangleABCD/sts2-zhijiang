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
using STS2RitsuLib.Keywords;
using Zhijiang.ZhijiangCode.Characters.Bella;

namespace Zhijiang.ZhijiangCode.Cards.Bella;

// 贝1：1 费攻击牌，造成 3→4 点伤害，并将抽牌堆中随机 2→3 张当前耗能为 1 的牌加入手牌。
// 检索逻辑参考原版 Anointed（抽牌堆 → 手牌随机选取），耗能过滤参考 AllForOne。
[RegisterCard(typeof(BellaCardPool))]
public sealed class BellaIsOne : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    // 检索耗能恰为该值的牌（本卡 = 1）。
    private const int FilterEnergyCost = 1;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Common;
    private const TargetType CardTarget = TargetType.AnyEnemy;
    private const bool ShowInCardLibrary = true;

    // 卡图待补：占位路径指向尚不存在的资源（构建会有 RITSU013 警告，方便日后找出缺卡图的牌）。
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/bella_is_one.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(3, ValueProp.Move),
        new DynamicVar("Cards", 2m)
    ];

    // 阴阳属性：贝1为阳牌。
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        BellaYinYangService.YangKeywordId.GetModCardKeyword()
    ];

    public BellaIsOne() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        // 只检索手牌空位数量的牌（手牌满时超出部分会被引擎转送弃牌堆，见 CardPileCmd.Add）。
        int count = Math.Min(DynamicVars["Cards"].IntValue,
            CardPile.MaxCardsInHand - PileType.Hand.GetPile(base.Owner).Cards.Count);
        if (count <= 0)
            return;

        List<CardModel> cards = PileType.Draw.GetPile(base.Owner).Cards
            .Where(IsTargetCost)
            .TakeRandom(count, base.Owner.RunState.Rng.CombatCardSelection)
            .ToList();
        if (cards.Count == 0)
            return;

        await CardPileCmd.Add(cards, PileType.Hand);
    }

    // 仅统计可打出的牌型（攻击/技能/能力），且当前耗能恰为目标值（X 费用除外）。
    private static bool IsTargetCost(CardModel card)
    {
        if (card.Type is not (CardType.Attack or CardType.Skill or CardType.Power))
            return false;
        if (card.EnergyCost.CostsX)
            return false;
        return card.EnergyCost.GetWithModifiers(CostModifiers.All) == FilterEnergyCost;
    }

    protected override void OnUpgrade()
    {
        // 伤害 3 → 4，检索 2 → 3 张。
        DynamicVars.Damage.UpgradeValueBy(1m);
        DynamicVars["Cards"].UpgradeValueBy(1m);
    }
}
