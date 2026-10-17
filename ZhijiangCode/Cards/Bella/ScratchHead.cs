using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Keywords;
using Zhijiang.ZhijiangCode.Characters.Bella;

namespace Zhijiang.ZhijiangCode.Cards.Bella;

// 挠头：罕见牌（阳 / 技能）。获得格挡；若本回合打出过反差牌，随机打出抽牌堆中的一张技能牌。
[RegisterCard(typeof(BellaCardPool))]
public sealed class ScratchHead : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Uncommon;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/scratch_head.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(7, ValueProp.Move),
        // 免费打出的技能牌张数（升级 1 → 2）。
        new DynamicVar("AutoPlayCount", 1m)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.Static(StaticHoverTip.Block)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        BellaYinYangService.YangKeywordId.GetModCardKeyword()
    ];

    public ScratchHead() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(base.Owner.Creature, DynamicVars.Block.IntValue, ValueProp.Move, cardPlay);

        // 本卡打出瞬间已被反差牌计数计入，若自身是反差牌需扣除自身。
        int contrastPlays = BellaYinYangService.GetContrastPlaysThisTurn(base.Owner);
        if (BellaYinYangService.IsContrastCard(base.Owner, this))
            contrastPlays--;

        if (contrastPlays <= 0)
            return;

        // 随机打出抽牌堆中的 {AutoPlayCount} 张技能牌（免费，同原版「横祸」Catastrophe；没有可打出的技能牌则无事发生）。
        // 每次重新取牌堆，避免重复打出同一张（上一张已在 AutoPlay 中离开抽牌堆）。
        for (int i = 0; i < DynamicVars["AutoPlayCount"].IntValue; i++)
        {
            CardModel? skill = PileType.Draw.GetPile(base.Owner).Cards
                .Where(c => c.Type == CardType.Skill && !c.Keywords.Contains(CardKeyword.Unplayable))
                .ToList()
                .StableShuffle(base.Owner.RunState.Rng.Shuffle)
                .FirstOrDefault();

            if (skill == null)
                return;

            await CardCmd.AutoPlay(choiceContext, skill, null);
        }
    }

    protected override void OnUpgrade()
    {
        // 格挡 7 → 10；免费打出的技能牌 1 → 2 张。
        DynamicVars.Block.UpgradeValueBy(3m);
        DynamicVars["AutoPlayCount"].UpgradeValueBy(1m);
    }
}
