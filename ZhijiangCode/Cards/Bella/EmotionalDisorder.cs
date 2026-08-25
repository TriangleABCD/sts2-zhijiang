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

// 情感障碍：稀有牌（阳 / 能力）。白拉时每回合开始获得（心之壁 ÷ 10）格挡；升级后每回合额外获得 1 心之壁。
[RegisterCard(typeof(BellaCardPool))]
public sealed class EmotionalDisorder : ModCardTemplate
{
    private const int BaseEnergyCost = 2;
    private const CardType CardKind = CardType.Power;
    private const CardRarity CardRarityValue = CardRarity.Rare;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;

    // 卡图占位：指向尚不存在的资源（构建会有 RITSU013 警告，方便日后找出缺卡图的牌）。
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/emotional_disorder.png");

    // 升级后每回合额外获得 1 心之壁（0 → 1）。
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("HeartWallGain", 0m)
    ];

    // 阴阳属性：情感障碍为阳牌。
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        BellaYinYangService.YangKeywordId.GetModCardKeyword()
    ];

    public EmotionalDisorder() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int heartWallGain = DynamicVars["HeartWallGain"].IntValue;
        EmotionalDisorderPower? power = await PowerCmd.Apply<EmotionalDisorderPower>(
            choiceContext, base.Owner.Creature, 1, base.Owner.Creature, this);
        power?.SetHeartWallGain(heartWallGain);
    }

    protected override void OnUpgrade()
    {
        // 升级：每回合额外 1 心之壁。
        DynamicVars["HeartWallGain"].UpgradeValueBy(1m);
    }
}
