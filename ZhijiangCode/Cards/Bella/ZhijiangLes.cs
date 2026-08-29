using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Keywords;
using Zhijiang.ZhijiangCode.Characters.Bella;
using Zhijiang.ZhijiangCode.Powers;

namespace Zhijiang.ZhijiangCode.Cards.Bella;

// 枝江小百合：1 费能力牌，每打出 5→3 张反差牌（与当前黑白拉状态阴阳相反的牌）获得 1 点敏捷。
// 反差牌判定复用 BellaYinYangService.IsContrastCard，Replay 多段只算第一段。
[RegisterCard(typeof(BellaCardPool))]
public sealed class ZhijiangLes : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Power;
    private const CardRarity CardRarityValue = CardRarity.Uncommon;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;

    // 卡图待补：占位路径指向尚不存在的资源（构建会有 RITSU013 警告，方便日后找出缺卡图的牌）。
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/zhijiang_les.png");

    // 每 N 张反差牌获得 1 点敏捷（5→3）。
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("ContrastThreshold", 5m)
    ];

    // 敏捷悬浮提示。
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<DexterityPower>()
    ];

    // 阴阳属性：枝江小百合为阴牌。
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        BellaYinYangService.YinKeywordId.GetModCardKeyword()
    ];

    public ZhijiangLes() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "PowerUp", base.Owner.Character.PowerUpAnimDelay);
        decimal threshold = DynamicVars["ContrastThreshold"].BaseValue;
        ZhijiangLesPower? power = await PowerCmd.Apply<ZhijiangLesPower>(choiceContext, base.Owner.Creature,
            threshold, base.Owner.Creature, this);
        power?.SetThreshold(threshold);
    }

    protected override void OnUpgrade()
    {
        // 阈值 5 → 3。
        DynamicVars["ContrastThreshold"].UpgradeValueBy(-2m);
    }
}
