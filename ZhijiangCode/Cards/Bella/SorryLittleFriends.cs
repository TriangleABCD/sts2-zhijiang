using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using Zhijiang.ZhijiangCode.Characters.Bella;
using STS2RitsuLib.Scaffolding.Content;
using Zhijiang.ZhijiangCode.SecondResource;

namespace Zhijiang.ZhijiangCode.Cards.Bella;

// 小伙伴对不起：普通牌（中立 / 技能）。获得 5 点心之壁并抽牌。
[RegisterCard(typeof(BellaCardPool))]
public sealed class SorryLittleFriends : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Common;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;

    // 卡图占位：指向尚不存在的资源（构建会有 RITSU013 警告，方便日后找出缺卡图的牌）。
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/sorry_little_friends.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("HeartWallGain", 7m),
        new CardsVar(1)
    ];

    // 中立牌：不挂阳/阴关键词。
    public override IEnumerable<CardKeyword> CanonicalKeywords => [];

    public SorryLittleFriends() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 获得 5 点心之壁。
        await SecondaryResourceCmd.Gain(base.Owner, HeartWall.HeartWallId,
            DynamicVars["HeartWallGain"].IntValue, this);

        // 抽 1→2 张牌。
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, base.Owner);
    }

    protected override void OnUpgrade()
    {
        // 抽牌 1 → 2（心之壁获取保持 5）。
        DynamicVars.Cards.UpgradeValueBy(1m);
    }
}