using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Keywords;
using Zhijiang.ZhijiangCode.Audio;
using Zhijiang.ZhijiangCode.Characters.Bella;
using Zhijiang.ZhijiangCode.Powers;

namespace Zhijiang.ZhijiangCode.Cards.Bella;

// 对抗路情侣：罕见牌（阳 / 能力）。每回合开始时获得心之壁。
[RegisterCard(typeof(BellaCardPool))]
public sealed class CoupleOnTheClashLane : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Power;
    private const CardRarity CardRarityValue = CardRarity.Uncommon;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;

    // 卡图待补：占位路径指向尚不存在的资源（构建会有 RITSU013 警告，方便日后找出缺卡图的牌）。
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/couple_on_the_clash_lane.png");

    // 每回合开始时获得的心之壁（5 → 10）。
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("HeartWallGain", 5m)
    ];

    // 阴阳属性：对抗路情侣为阳牌。
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        BellaYinYangService.YangKeywordId.GetModCardKeyword()
    ];

    public CoupleOnTheClashLane() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 打出音效：Zhijiang/audios/couple_on_the_clash_lane.ogg。
        BellaAudio.PlayCardSfx("couple_on_the_clash_lane.ogg");

        await CreatureCmd.TriggerAnim(base.Owner.Creature, "PowerUp", base.Owner.Character.PowerUpAnimDelay);
        int heartWallGain = DynamicVars["HeartWallGain"].IntValue;
        await PowerCmd.Apply<CoupleOnTheClashLanePower>(choiceContext, base.Owner.Creature,
            heartWallGain, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // 每回合心之壁 5 → 10。
        DynamicVars["HeartWallGain"].UpgradeValueBy(5m);
    }
}
