using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Zhijiang.ZhijiangCode.Characters.Bella;
using Zhijiang.ZhijiangCode.Powers;

namespace Zhijiang.ZhijiangCode.Cards.Bella;

// 9000同接擤鼻涕：罕见牌（中立 / 技能）。获得 1→2 能量；本回合结束时失去 5 心之壁。
[RegisterCard(typeof(BellaCardPool))]
public sealed class NoseBlowWhen9000Concurrent : ModCardTemplate
{
    private const int BaseEnergyCost = 0;
    private const int HeartWallLoss = 5;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Uncommon;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;

    // 卡图占位：指向尚不存在的资源（构建会有 RITSU013 警告，方便日后找出缺卡图的牌）。
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/nose_blow_when_9000_concurrent.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar(1)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        base.EnergyHoverTip
    ];

    // 中立牌：不挂阳/阴关键词。
    public override IEnumerable<CardKeyword> CanonicalKeywords => [];

    public NoseBlowWhen9000Concurrent() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 获得 1→2 能量。
        await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, base.Owner);

        // 本回合结束时失去 5 心之壁。
        await PowerCmd.Apply<ConcurrentNoseBlowPower>(choiceContext, base.Owner.Creature, HeartWallLoss, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // 升级：能量 1 → 2（心之壁损失保持 5）。
        DynamicVars.Energy.UpgradeValueBy(1m);
    }
}