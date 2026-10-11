using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Zhijiang.ZhijiangCode.Characters.Bella;
using Zhijiang.ZhijiangCode.SecondResource;

namespace Zhijiang.ZhijiangCode.Relics;

/// <summary>
/// 晃悠悠：每打出一张反差牌，获得 5 心之壁。
/// 反差牌的口径与 <see cref="BellaYinYangService.IsContrastCard"/> 一致（含「了转反」翻转）。
/// </summary>
[RegisterRelic(typeof(BellaRelicPool))]
public sealed class SwayingHairpin : ModRelicTemplate
{
    private int _lastContrastPlays;

    public override RelicRarity Rarity => RelicRarity.Common;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("HeartWallGain", 5m)
    ];

    // 占位图标：复用贝极星素材，后续替换为专属图标。
    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/relics/swaying_hairpin_85x85.png",
        IconOutlinePath: $"{Entry.ResPath}/images/relics/swaying_hairpin_85x85.png",
        BigIconPath: $"{Entry.ResPath}/images/relics/swaying_hairpin_256x256.png");

    public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == base.Owner.Creature.Side)
            _lastContrastPlays = 0;
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // Replay 多段重复打出只算第一段。
        if (!cardPlay.IsFirstInSeries)
            return;
        if (cardPlay.Card.Owner != base.Owner)
            return;

        // 反差牌计数在打出瞬间已累加（本张牌若是反差牌就已计入）。
        // 不能在此处重新算 IsContrastCard——若本次打的是能力牌（打出后不再参与阴阳计数），
        // 状态可能在 AfterCardPlayed 时已经翻转，导致同一张牌重新判定为“不是反差牌”。
        // 计数增长即表示刚打出的这张是反差牌；若某次钩子漏触发，奖励只会顺延到下一张牌、总数不变。
        int contrastPlays = BellaYinYangService.GetContrastPlaysThisTurn(base.Owner);
        if (contrastPlays <= _lastContrastPlays)
        {
            _lastContrastPlays = contrastPlays;
            return;
        }

        _lastContrastPlays = contrastPlays;
        Flash();
        await SecondaryResourceCmd.Gain(
            base.Owner, HeartWall.HeartWallId, DynamicVars["HeartWallGain"].IntValue, this);
    }
}
