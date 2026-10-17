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
using Zhijiang.ZhijiangCode.Powers;

namespace Zhijiang.ZhijiangCode.Cards.Bella;

// 日程表：普通牌（阳 / 技能）。获得格挡；本回合每消耗 1 点心之壁，获得 1→2 点格挡（升级后翻倍）。
[RegisterCard(typeof(BellaCardPool))]
public sealed class Schedule : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Common;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;

    // 卡图占位：暂用通用防御牌卡图。
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/schedule.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(11, ValueProp.Move),
        // 本回合每消耗 1 点心之壁换到的格挡数（升级 1 → 2）。
        new DynamicVar("WallToBlock", 1m)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.Static(StaticHoverTip.Block)
    ];

    // 阴阳属性：日程表为阳牌。
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        BellaYinYangService.YangKeywordId.GetModCardKeyword()
    ];

    public Schedule() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 获得 11→13 点格挡。
        await CreatureCmd.GainBlock(base.Owner.Creature, DynamicVars.Block, cardPlay);

        // 挂上「日程表」临时能力：本回合内每消耗 1 点心之壁，获得 {WallToBlock} 点格挡（层数即每次消耗的格挡数）。
        await PowerCmd.Apply<SchedulePower>(choiceContext, base.Owner.Creature,
            DynamicVars["WallToBlock"].IntValue, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // 格挡 11 → 13；每点心之壁换到的格挡 1 → 2。
        DynamicVars.Block.UpgradeValueBy(2m);
        DynamicVars["WallToBlock"].UpgradeValueBy(1m);
    }
}