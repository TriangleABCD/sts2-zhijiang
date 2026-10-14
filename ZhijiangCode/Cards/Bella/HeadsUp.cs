using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Keywords;
using Zhijiang.ZhijiangCode.Audio;
using Zhijiang.ZhijiangCode.Characters.Bella;
using Zhijiang.ZhijiangCode.SecondResource;

namespace Zhijiang.ZhijiangCode.Cards.Bella;

// 抬头！：罕见牌（阳 / 技能）。自己获得 3→4 点力量（不再影响队友）。耗 25 心之壁。
[RegisterCard(typeof(BellaCardPool))]
public sealed class HeadsUp : ModCardTemplate
{
    private const int BaseEnergyCost = 0;
    private const int HeartWallCost = 25;

    // 每打出一次，本场战斗内这张牌的心之壁费用增加的量（同原版「模组改造」的 EnergyCost.AddThisCombat(1)）。
    private const int HeartWallCostIncreasePerPlay = 10;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Uncommon;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;

    // 卡图占位：指向尚不存在的资源（构建会有 RITSU013 警告，方便日后找出缺卡图的牌）。
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/heads_up.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("StrengthAmount", 3m)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<StrengthPower>()
    ];

    // 阴阳属性：抬头！为阳牌。
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        BellaYinYangService.YangKeywordId.GetModCardKeyword()
    ];

    public HeadsUp() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
        // 第二费用：消耗 25 心之壁。
        this.SecondaryCosts().Set(HeartWall.HeartWallId, HeartWallCost);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 打出音效：Zhijiang/audios/heads_up.ogg。
        BellaAudio.PlayCardSfx("heads_up.ogg");

        // 只给自己加力量（原为所有队友）。
        await PowerCmd.Apply<StrengthPower>(choiceContext, base.Owner.Creature,
            DynamicVars["StrengthAmount"].IntValue, base.Owner.Creature, this);

        // 每打出一次，这张牌的心之壁费用 +10；用 ThisCombat 层，战斗结束随卡牌对象一起清除。
        int nextCost = this.SecondaryCosts().Get(HeartWall.HeartWallId).Amount + HeartWallCostIncreasePerPlay;
        this.SecondaryCosts().Set(HeartWall.HeartWallId, new SecondaryResourceCost(nextCost),
            SecondaryResourceCostDuration.ThisCombat);
    }

    protected override void OnUpgrade()
    {
        // 力量 3 → 4。
        DynamicVars["StrengthAmount"].UpgradeValueBy(1m);
    }
}
