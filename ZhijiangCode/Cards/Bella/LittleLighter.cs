using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Keywords;
using Zhijiang.ZhijiangCode.Characters.Bella;

namespace Zhijiang.ZhijiangCode.Cards.Bella;

// 小打火机：稀有牌（阴 / 攻击）。造成 1 + 累计加成 伤害；消耗所有状态牌，每张使本场后续伤害 +PerStatus。
[RegisterCard(typeof(BellaCardPool))]
public sealed class LittleLighter : ModCardTemplate
{
    private const int BaseEnergyCost = 2;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Rare;
    private const TargetType CardTarget = TargetType.AnyEnemy;
    private const bool ShowInCardLibrary = true;

    // 本场战斗累计的伤害加成（按本张卡实例计）。
    private int _bonus;

    // 卡图占位：指向尚不存在的资源（构建会有 RITSU013 警告，方便日后找出缺卡图的牌）。
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/little_lighter.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(3, ValueProp.Move),
        new DynamicVar("PerStatus", 1m),
        new CalculationBaseVar(0m),
        new CalculationExtraVar(1m),
        new CalculatedVar("CurrentDamage").WithMultiplier(
            (CardModel card, Creature? _) => 1 + ((LittleLighter)card)._bonus)
    ];

    // 阴阳属性：小打火机为阴牌。
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        BellaYinYangService.YinKeywordId.GetModCardKeyword()
    ];

    public LittleLighter() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) return;

        // 造成 1 + 累计加成 伤害。
        await DamageCmd.Attack(1 + _bonus)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        // 消耗所有状态牌（不含已消耗堆）。
        List<CardModel> statuses = base.Owner.PlayerCombatState!.AllCards
            .Where(c => c.Type == CardType.Status && c.Pile.Type != PileType.Exhaust)
            .ToList();
        foreach (CardModel status in statuses)
            await CardCmd.Exhaust(choiceContext, status);

        // 每张使本场后续伤害 +PerStatus。
        _bonus += statuses.Count * DynamicVars["PerStatus"].IntValue;
    }

    protected override void OnUpgrade()
    {
        // 每张状态牌加成 1 → 2。
        DynamicVars["PerStatus"].UpgradeValueBy(1m);
    }
}
