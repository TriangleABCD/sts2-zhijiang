using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Keywords;
using Zhijiang.ZhijiangCode.Characters.Bella;
using Zhijiang.ZhijiangCode.SecondResource;

namespace Zhijiang.ZhijiangCode.Cards.Bella;

// 嘿嘿！：将你的能量翻倍（升级耗能 1→0），消耗 10 心之壁。
[RegisterCard(typeof(BellaCardPool))]
public sealed class Hihi : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Common;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;
    private const int HeartWallCost = 10;

    // 卡图待补：占位路径指向尚不存在的资源（构建会有 RITSU013 警告，方便日后找出缺卡图的牌）。
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/hihi.png");

    // 能量图标悬浮提示（同原版 DoubleEnergy）。
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        base.EnergyHoverTip
    ];

    // 消耗 + 阴阳属性：嘿嘿！为阳牌。
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust,
        BellaYinYangService.YangKeywordId.GetModCardKeyword()
    ];

    public Hihi() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
        // 第二费用：消耗 10 心之壁（显示在能量图标下方，不写进描述文本）。
        this.SecondaryCosts().Set(HeartWall.HeartWallId, HeartWallCost);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 将能量翻倍：获得与当前能量相等的能量（同原版 DoubleEnergy）。
        await PlayerCmd.GainEnergy(base.Owner.PlayerCombatState.Energy, base.Owner);
    }

    protected override void OnUpgrade()
    {
        // 耗能 1 → 0。
        base.EnergyCost.UpgradeBy(-1);
    }
}
