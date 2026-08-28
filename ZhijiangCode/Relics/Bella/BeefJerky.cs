using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Zhijiang.ZhijiangCode.Characters.Bella;

namespace Zhijiang.ZhijiangCode.Relics;

/// <summary>
/// 牛肉干：每场战斗的卡牌奖励可额外选择 1 次（每战斗重置）。
/// </summary>
[RegisterRelic(typeof(BellaRelicPool))]
public sealed class BeefJerky : ModRelicTemplate
{
    private int _timesUsed;

    public override RelicRarity Rarity => RelicRarity.Shop;

    [SavedProperty]
    public int TimesUsed
    {
        get => _timesUsed;
        set
        {
            AssertMutable();
            _timesUsed = value;
        }
    }

    // 占位图标：复用贝极星素材，后续替换为专属图标。
    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/relics/beaf_jerky_85x85.png",
        IconOutlinePath: $"{Entry.ResPath}/images/relics/beaf_jerky_85x85.png",
        BigIconPath: $"{Entry.ResPath}/images/relics/beaf_jerky_256x256.png");

    public override Task BeforeCombatStart()
    {
        TimesUsed = 0;
        return Task.CompletedTask;
    }

    public override bool ShouldAllowSelectingMoreCardRewards(Player player, CardReward cardReward)
    {
        if (player != base.Owner)
            return false;
        if (TimesUsed >= 1)
            return false;
        TimesUsed++;
        return true;
    }
}
