using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Keywords;
using Zhijiang.ZhijiangCode.Characters.Bella;

namespace Zhijiang.ZhijiangCode.Cards.Bella;

// 末路狂花：普通牌（阴 / 攻击）。造成伤害；若手牌中没有阳牌，则在本场战斗中升级手牌中的所有牌。
[RegisterCard(typeof(BellaCardPool))]
public sealed class FlowerInDeath : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Common;
    private const TargetType CardTarget = TargetType.AnyEnemy;
    private const bool ShowInCardLibrary = true;

    // 卡图占位：暂用通用攻击牌卡图。
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/flower_in_death.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(9, ValueProp.Move)
    ];

    // 阴阳属性：末路狂花为阴牌。
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        BellaYinYangService.YinKeywordId.GetModCardKeyword()
    ];

    public FlowerInDeath() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        // 若手牌中没有阳牌，则升级手牌中的所有牌。
        // 战斗中打出的牌是牌组卡牌的克隆体（Player.PopulateCombatState 里 state.CloneCard），
        // 因此升级只持续到本场战斗结束、战斗结束自动还原，无需手动回滚——同原版药水「锻造祝福」。
        bool hasYangInHand = PileType.Hand.GetPile(base.Owner).Cards
            .Any(c => c.Keywords.Contains(BellaYinYangService.YangKeywordId.GetModCardKeyword()));
        if (hasYangInHand)
            return;

        foreach (CardModel card in PileType.Hand.GetPile(base.Owner).Cards.ToList())
        {
            if (card.IsUpgradable)
                CardCmd.Upgrade(card);
        }
    }

    protected override void OnUpgrade()
    {
        // 伤害 9 → 13。
        DynamicVars.Damage.UpgradeValueBy(4m);
    }
}
