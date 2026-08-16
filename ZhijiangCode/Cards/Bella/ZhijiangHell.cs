using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Keywords;
using Zhijiang.ZhijiangCode.Characters.Bella;
using Zhijiang.ZhijiangCode.SecondResource;

namespace Zhijiang.ZhijiangCode.Cards.Bella;

// 枝江地狱：0 费获得 1 能量，代价是消耗 12→8 心之壁。
[RegisterCard(typeof(BellaCardPool))]
public sealed class ZhijiangHell : ModCardTemplate
{
    private const int BaseEnergyCost = 0;
    private const int BaseHeartWallCost = 12;
    private const int UpgradedHeartWallCost = 8;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Uncommon;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;

    // 卡图待补：占位路径指向尚不存在的资源（构建会有 RITSU013 警告，方便日后找出缺卡图的牌）。
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/zhijiang_hell.png");

    // 能量增益（恒为 1，升级只降心之壁费用）。
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar(1)
    ];

    // 能量图标悬浮提示（同原版能量牌）。
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        base.EnergyHoverTip
    ];

    // 阴阳属性：枝江地狱为阳牌。
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        BellaYinYangService.YangKeywordId.GetModCardKeyword()
    ];

    public ZhijiangHell() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
        // 第二费用：消耗 12 心之壁。
        this.SecondaryCosts().Set(HeartWall.HeartWallId, BaseHeartWallCost);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 获得 1 点能量。
        await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, base.Owner);
    }

    protected override void OnUpgrade()
    {
        // 心之壁费用 12 → 8。
        this.SecondaryCosts().Set(HeartWall.HeartWallId, UpgradedHeartWallCost);
    }
}
