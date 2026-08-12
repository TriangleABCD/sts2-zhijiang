using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Orbs;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Orbs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Keywords;
using Zhijiang.ZhijiangCode.Characters.Bella;

namespace Zhijiang.ZhijiangCode.Cards.Bella;

[RegisterCard(typeof(BellaCardPool))]
public sealed class IceBeauty : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Common;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;
    private const int OrbCount = 1;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/ice_beauty.png");

    // 阴阳属性：冰山美人为阳牌。
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        BellaYinYangService.YangKeywordId.GetModCardKeyword()
    ];

    public IceBeauty() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 确保至少 2 个充能球栏位，不足则补足。
        int capacity = base.Owner.GetOrbCapacity();
        if (capacity < OrbCount)
        {
            await OrbCmd.AddSlots(base.Owner, OrbCount - capacity);
        }

        for (int i = 0; i < OrbCount; i++)
        {
            // 栏位已满时先激发最靠前的球，为新球腾出位置。
            if (base.Owner.PlayerCombatState?.OrbQueue is { } queue
                && queue.Orbs.Count >= queue.Capacity && queue.Capacity > 0)
            {
                await OrbCmd.EvokeNext(choiceContext, base.Owner);
            }
            await OrbCmd.Channel<FrostOrb>(choiceContext, base.Owner);
        }
    }

    protected override void OnUpgrade()
    {
        // 耗能 1 → 0。
        base.EnergyCost.UpgradeBy(-1);
    }
}
