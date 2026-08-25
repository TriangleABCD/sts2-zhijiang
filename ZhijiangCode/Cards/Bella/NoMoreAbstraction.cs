using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Keywords;
using Zhijiang.ZhijiangCode.Cards.Status;
using Zhijiang.ZhijiangCode.Characters.Bella;
using Zhijiang.ZhijiangCode.SecondResource;

namespace Zhijiang.ZhijiangCode.Cards.Bella;

// 我再也不玩抽象了：稀有牌（阳 / 技能）。消耗所有「高雅」，每消耗 3 张获得 1 点能量并抽 1 张牌。
[RegisterCard(typeof(BellaCardPool))]
public sealed class NoMoreAbstraction : ModCardTemplate
{
    private const int BaseEnergyCost = 3;
    private const int HeartWallCost = 25;
    private const int ElegantPerReward = 3;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Rare;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;

    // 卡图占位：指向尚不存在的资源（构建会有 RITSU013 警告，方便日后找出缺卡图的牌）。
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/no_more_abstraction.png");

    // 动态显示：消耗高雅数量换算出的能量/抽牌数。
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(0m),
        new CalculationExtraVar(1m),
        new CalculatedVar("CalculatedEnergy")
            .WithMultiplier((CardModel card, Creature? _) => GetRewardCount(card.Owner)),
        new CalculatedVar("CalculatedDraw")
            .WithMultiplier((CardModel card, Creature? _) => GetRewardCount(card.Owner))
    ];

    // 「高雅」状态牌悬浮提示。
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromCard<Elegant>()
    ];

    // 消耗 + 阳。
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust,
        BellaYinYangService.YangKeywordId.GetModCardKeyword()
    ];

    public NoMoreAbstraction() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
        // 第二费用：消耗 25 心之壁。
        this.SecondaryCosts().Set(HeartWall.HeartWallId, HeartWallCost);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 当前持有的「高雅」（各牌堆，不含已消耗堆）。
        List<CardModel> elegants = GetElegantCards(base.Owner).ToList();

        // 消耗所有高雅。
        foreach (CardModel elegant in elegants)
            await CardCmd.Exhaust(choiceContext, elegant);

        // 每 3 张：1 点能量 + 抽 1 张牌。
        int rewards = GetRewardCount(elegants.Count);
        if (rewards > 0)
        {
            await PlayerCmd.GainEnergy(rewards, base.Owner);
            await CardPileCmd.Draw(choiceContext, rewards, base.Owner);
        }
    }

    private static int GetRewardCount(int elegantCount) => elegantCount / ElegantPerReward;

    private static int GetRewardCount(Player owner) => GetRewardCount(GetElegantCards(owner).Count());

    private static IEnumerable<CardModel> GetElegantCards(Player owner)
        => owner.PlayerCombatState!.AllCards.Where(c => c is Elegant && c.Pile.Type != PileType.Exhaust);

    protected override void OnUpgrade()
    {
        // 升级：移除消耗（能量/抽牌、费用不变）。
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
