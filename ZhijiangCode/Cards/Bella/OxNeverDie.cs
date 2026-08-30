using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Keywords;
using Zhijiang.ZhijiangCode.Cards.Status;
using Zhijiang.ZhijiangCode.Characters.Bella;

namespace Zhijiang.ZhijiangCode.Cards.Bella;

// 牛不灭（Ox Never Die）：1 费普通攻击牌（阳）。
// 造成 13→16 点伤害，弃牌堆加入 1 张「牛批」（状态牌，阴）。
[RegisterCard(typeof(BellaCardPool))]
public sealed class OxNeverDie : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Common;
    private const TargetType CardTarget = TargetType.AnyEnemy;
    private const bool ShowInCardLibrary = true;
    private const int NpCount = 1;

    // 卡图占位：指向尚不存在的资源（构建会有 RITSU013 警告，方便日后找出缺卡图的牌）。
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/ox_never_die.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(13, ValueProp.Move)
    ];

    // 「牛批」状态牌悬浮提示。
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromCard<Np>()
    ];

    // 阴阳属性：牛不灭为阳牌。
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        BellaYinYangService.YangKeywordId.GetModCardKeyword()
    ];

    public OxNeverDie() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        // 弃牌堆中加入 2 张「牛批」（仅自己，联机不广播）。
        IReadOnlyList<CardPileAddResult> discardResults = await CardPileCmd.AddGeneratedCardsToCombat(
            Np.Create(base.Owner, NpCount, base.CombatState!), PileType.Discard, base.Owner);

        // 仅刷新本地玩家的 UI。
        if (LocalContext.IsMe(base.Owner))
            CardCmd.PreviewCardPileAdd(discardResults);
    }

    protected override void OnUpgrade()
    {
        // 伤害 13 → 16。
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}
