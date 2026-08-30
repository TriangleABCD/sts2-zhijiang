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
using Zhijiang.ZhijiangCode.Characters.Bella;
using Zhijiang.ZhijiangCode.SecondResource;

namespace Zhijiang.ZhijiangCode.Cards.Bella;

// 9000同接擤鼻涕：罕见牌（中立 / 技能）。0 费，获得 2→3 能量；消耗 5 心之壁。
[RegisterCard(typeof(BellaCardPool))]
public sealed class NoseBlowWhen9000Concurrent : ModCardTemplate
{
    private const int BaseEnergyCost = 0;
    private const int HeartWallCost = 5;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Uncommon;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;

    // 卡图占位：指向尚不存在的资源（构建会有 RITSU013 警告，方便日后找出缺卡图的牌）。
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/nose_blow_when_9000_concurrent.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar(2)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        base.EnergyHoverTip
    ];

    // 中立牌：不挂阳/阴关键词。
    public override IEnumerable<CardKeyword> CanonicalKeywords => [];

    public NoseBlowWhen9000Concurrent() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
        // 第二费用：消耗 5 心之壁。
        this.SecondaryCosts().Set(HeartWall.HeartWallId, HeartWallCost);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 获得 2→3 能量。
        await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, base.Owner);
    }

    protected override void OnUpgrade()
    {
        // 升级：能量 2 → 3（心之壁消耗保持 5）。
        DynamicVars.Energy.UpgradeValueBy(1m);
    }
}