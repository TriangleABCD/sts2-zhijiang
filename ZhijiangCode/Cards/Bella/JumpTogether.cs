using System;
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
using Zhijiang.ZhijiangCode.Audio;
using Zhijiang.ZhijiangCode.Characters.Bella;

namespace Zhijiang.ZhijiangCode.Cards.Bella;

// 一起跳起来：罕见牌（阳 / 攻击）。造成伤害；若本回合打出过反差牌，额外攻击 1→2 次。
[RegisterCard(typeof(BellaCardPool))]
public sealed class JumpTogether : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Uncommon;
    private const TargetType CardTarget = TargetType.AnyEnemy;
    private const bool ShowInCardLibrary = true;

    // 卡图占位：指向尚不存在的资源（构建会有 RITSU013 警告，方便日后找出缺卡图的牌）。
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/jump_together.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(7, ValueProp.Move),
        new DynamicVar("ExtraHits", 1m),
        // 战斗内实时显示当前实际攻击段数（基础 1 段；本回合打过反差牌则 +ExtraHits）。
        new CalculationBaseVar(1m),
        new CalculationExtraVar(1m),
        new CalculatedVar("CalculatedHits").WithMultiplier((card, _) => ExtraHits(card))
    ];

    // 阴阳属性：一起跳起来为阳牌。
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        BellaYinYangService.YangKeywordId.GetModCardKeyword()
    ];

    public JumpTogether() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 打出音效：Zhijiang/audios/jump_together.ogg。
        BellaAudio.PlayCardSfx("jump_together.ogg");

        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        // 与卡面 {CalculatedHits} 同源，保证预览与实际一致。
        int hitCount = Math.Max(1, (int)((CalculatedVar)DynamicVars["CalculatedHits"]).Calculate(cardPlay.Target));

        for (int i = 0; i < hitCount; i++)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this)
                .Targeting(cardPlay.Target)
                .Execute(choiceContext);
        }
    }

    /// <summary>额外攻击段数：本回合此前打出过反差牌时为 ExtraHits，否则为 0。</summary>
    private static decimal ExtraHits(CardModel card)
    {
        int contrastPlays = BellaYinYangService.GetContrastPlaysThisTurn(card.Owner);
        if (BellaYinYangService.IsContrastCard(card.Owner, card))
            contrastPlays--;

        return contrastPlays > 0 ? card.DynamicVars["ExtraHits"].IntValue : 0;
    }

    protected override void OnUpgrade()
    {
        // 伤害 5 → 7，额外攻击 1 → 2 次。
        DynamicVars.Damage.UpgradeValueBy(2m);
        DynamicVars["ExtraHits"].UpgradeValueBy(1m);
    }
}
