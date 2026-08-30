using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Keywords;
using Zhijiang.ZhijiangCode.Cards.Status;
using Zhijiang.ZhijiangCode.Characters.Bella;

namespace Zhijiang.ZhijiangCode.Cards.Bella;

// 月兔回旋于空中：普通牌（阴 / 技能）。获得格挡，并将 2 张「高雅」加入你的抽牌堆。
[RegisterCard(typeof(BellaCardPool))]
public sealed class MoonRabbitSpinningInTheAir : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const int ElegantCount = 1;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Common;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;

    // 卡图占位：指向尚不存在的资源（构建会有 RITSU013 警告，方便日后找出缺卡图的牌）。
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/moon_rabbit_spinning_in_the_air.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(17, ValueProp.Move),
        new DynamicVar("DexterityGain", 1m)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.Static(StaticHoverTip.Block),
        HoverTipFactory.FromPower<DexterityPower>()
    ];

    // 阴阳属性：月兔回旋于空中为阴牌（与阳牌牛不灭对称）。
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        BellaYinYangService.YinKeywordId.GetModCardKeyword()
    ];

    public MoonRabbitSpinningInTheAir() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 获得 17→21 点格挡。
        await CreatureCmd.GainBlock(base.Owner.Creature, DynamicVars.Block, cardPlay);

        // 获得 1→2 点敏捷。
        await PowerCmd.Apply<DexterityPower>(choiceContext, base.Owner.Creature, DynamicVars["DexterityGain"].IntValue, base.Owner.Creature, this);

        // 将 1 张「高雅」加入抽牌堆（仅自己，联机不广播）。
        IReadOnlyList<CardPileAddResult> drawResults = await CardPileCmd.AddGeneratedCardsToCombat(
            Elegant.Create(base.Owner, ElegantCount, base.CombatState!),
            PileType.Draw, base.Owner, CardPilePosition.Random);

        // 仅刷新本地玩家的 UI。
        if (LocalContext.IsMe(base.Owner))
            CardCmd.PreviewCardPileAdd(drawResults);
    }

    protected override void OnUpgrade()
    {
        // 格挡 17 → 21，敏捷 1 → 2。
        DynamicVars.Block.UpgradeValueBy(4m);
        DynamicVars["DexterityGain"].UpgradeValueBy(1m);
    }
}
