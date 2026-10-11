using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Keywords;
using Zhijiang.ZhijiangCode.Characters.Bella;

namespace Zhijiang.ZhijiangCode.Cards.Bella;

// 贝极熊：罕见牌（阳 / 技能）。获得格挡；若本回合没打出过攻击牌，额外获得格挡。
[RegisterCard(typeof(BellaCardPool))]
public sealed class BellarisBear : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Uncommon;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;

    // 卡图占位：指向尚不存在的资源（构建会有 RITSU013 警告，方便日后找出缺卡图的牌）。
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/bellaris_bear.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(11, ValueProp.Move),
        new DynamicVar("BonusBlock", 5m)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.Static(StaticHoverTip.Block)
    ];

    // 阴阳属性：贝极熊为阳牌。
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        BellaYinYangService.YangKeywordId.GetModCardKeyword()
    ];

    public BellarisBear() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int block = DynamicVars.Block.IntValue;
        if (BellaYinYangService.GetAttackPlaysThisTurn(base.Owner) == 0)
            block += DynamicVars["BonusBlock"].IntValue;

        await CreatureCmd.GainBlock(base.Owner.Creature, block, ValueProp.Move, cardPlay);
    }

    protected override void OnUpgrade()
    {
        // 格挡 11 → 16，额外格挡 5 → 7（触发条件不随升级变化）。
        DynamicVars.Block.UpgradeValueBy(5m);
        DynamicVars["BonusBlock"].UpgradeValueBy(2m);
    }
}
