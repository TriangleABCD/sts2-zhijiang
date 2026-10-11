using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Keywords;
using Zhijiang.ZhijiangCode.Characters.Bella;
using Zhijiang.ZhijiangCode.Powers;

namespace Zhijiang.ZhijiangCode.Cards.Bella;

// 贝拉博士（Doctor Bella）：罕见牌（阴 / 攻击）。造成伤害；若处于白拉，则本场战斗判定翻转为黑拉。
[RegisterCard(typeof(BellaCardPool))]
public sealed class DoctorBella : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Uncommon;
    private const TargetType CardTarget = TargetType.AnyEnemy;
    private const bool ShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/doctor_bella.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(7, ValueProp.Move)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        BellaYinYangService.YinKeywordId.GetModCardKeyword()
    ];

    public DoctorBella() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) return;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        // 若当前处于白拉，则变为黑拉：翻转本场战斗的判定（已是黑拉则无效果）。
        // 翻转由隐藏 Power TurnOverPower 承载（与「了转反」同一套机制，战斗结束自动清除）；
        // 若此前已被「了转反」翻转过，则撤销该标记回到未翻转，最终同样是黑拉。
        if (!BellaYinYangService.IsBaiLa(base.Owner))
            return;

        if (BellaYinYangService.IsInverted(base.Owner))
            await PowerCmd.Remove<TurnOverPower>(base.Owner.Creature);
        else
            await PowerCmd.Apply<TurnOverPower>(choiceContext, base.Owner.Creature, 1, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // 伤害 7 → 10。
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}
