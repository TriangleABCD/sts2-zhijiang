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

// NPK48：稀有攻击牌（阴）。3 费 + 25 心之壁。
// 消耗所有「牛批」（各牌堆，不含已消耗堆），每张对随机敌人造成 7 点伤害。
// 原理参考原版机器人「散射炮」（FlakCannon）。
[RegisterCard(typeof(BellaCardPool))]
public sealed class Npk48 : ModCardTemplate
{
    private const int BaseEnergyCost = 3;
    private const int HeartWallCost = 25;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Rare;
    private const TargetType CardTarget = TargetType.RandomEnemy;
    private const bool ShowInCardLibrary = true;

    // 卡图占位：指向尚不存在的资源（构建会有 RITSU013 警告，方便日后找出缺卡图的牌）。
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/npk48.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(10, ValueProp.Move),
        new CalculationBaseVar(0m),
        new CalculationExtraVar(1m),
        new CalculatedVar("CalculatedHits").WithMultiplier((CardModel card, Creature? _) => GetNpCount(card.Owner))
    ];

    // 「牛批」状态牌悬浮提示。
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromCard<Np>()
    ];

    // 消耗 + 阴。
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust,
        BellaYinYangService.YinKeywordId.GetModCardKeyword()
    ];

    public Npk48() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
        // 第二费用：消耗 25 心之壁。
        this.SecondaryCosts().Set(HeartWall.HeartWallId, HeartWallCost);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 当前持有的「牛批」（各牌堆，不含已消耗堆）。
        int hitCount = GetNpCount(base.Owner);
        List<CardModel> nps = GetNpCards(base.Owner).ToList();

        // 消耗所有牛批。
        foreach (CardModel np in nps)
            await CardCmd.Exhaust(choiceContext, np);

        // 每张牛批对随机敌人造成伤害（随机弹射，可命中同一敌人）。
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .WithHitCount(hitCount)
            .FromCard(this)
            .TargetingRandomOpponents(base.CombatState!)
            .WithHitFx("vfx/vfx_attack_blunt", null, "blunt_attack.mp3")
            .Execute(choiceContext);
    }

    private static int GetNpCount(Player owner) => GetNpCards(owner).Count();

    private static IEnumerable<CardModel> GetNpCards(Player owner)
        => owner.PlayerCombatState!.AllCards.Where(c => c is Np && c.Pile.Type != PileType.Exhaust);

    protected override void OnUpgrade()
    {
        // 升级：移除消耗（伤害、费用不变）。
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
