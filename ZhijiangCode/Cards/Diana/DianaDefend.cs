using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Zhijiang.ZhijiangCode.Characters.Diana;
using Zhijiang.ZhijiangCode.Keywords;

namespace Zhijiang.ZhijiangCode.Cards.Diana;

// 嘉然的基础防御，数值与原版/贝拉对齐。
[RegisterCard(typeof(DianaCardPool))]
[RegisterCharacterStarterCard(typeof(DianaCharacter), 4, Order = 1)]
public sealed class DianaDefend : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Basic;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;

    public override bool GainsBlock => true;

    // TODO(美术)：卡图路径已写好，图片文件待补（缺失时 RitsuLib 回退占位图）。
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Diana/diana_defend.png");

    protected override HashSet<CardTag> CanonicalTags => new() { CardTag.Defend };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(5m, ValueProp.Move)
    ];

    // 扑克牌面：方块 4（花色与点数是两个独立关键词）。
    // 特意与打击的 ♠3 错开点数：同点判定只看点数，若基础牌都堆在 3 点上，
    // 起手必然三条/炸弹，同点族的收益曲线就压不住。
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        DianaPokerKeywords.Diamond,
        DianaPokerKeywords.Rank4
    ];

    public DianaDefend() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m);
    }
}
