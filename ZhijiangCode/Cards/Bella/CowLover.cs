using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Keywords;
using Zhijiang.ZhijiangCode.Characters.Bella;
using Zhijiang.ZhijiangCode.Powers;

namespace Zhijiang.ZhijiangCode.Cards.Bella;

// 牛牛民：罕见牌（阴 / 能力）。每当你消耗一张牛批，获得心之壁。
[RegisterCard(typeof(BellaCardPool))]
public sealed class CowLover : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Power;
    private const CardRarity CardRarityValue = CardRarity.Uncommon;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;

    // 卡图占位：指向尚不存在的资源（构建会有 RITSU013 警告，方便日后找出缺卡图的牌）。
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/cow_lover.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("HeartWallGain", 1m)
    ];

    // 阴阳属性：牛牛民为阴牌。
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        BellaYinYangService.YinKeywordId.GetModCardKeyword()
    ];

    public CowLover() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int gain = DynamicVars["HeartWallGain"].IntValue;
        await PowerCmd.Apply<CowLoverPower>(choiceContext, base.Owner.Creature, gain, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // 心之壁 1 → 2。
        DynamicVars["HeartWallGain"].UpgradeValueBy(1m);
    }
}
