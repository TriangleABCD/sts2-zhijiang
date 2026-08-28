using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Zhijiang.ZhijiangCode.Characters.Bella;

namespace Zhijiang.ZhijiangCode.Relics;

/// <summary>
/// 露西亚痛机：白拉/黑拉状态翻转时，抽 1 张牌并获得 2 点固定格挡。
/// 通过 <see cref="IBellaStateFlipListener"/> 接收翻转通知（首次进入战斗不触发）。
/// </summary>
[RegisterRelic(typeof(BellaRelicPool))]
public sealed class LuciaPC : ModRelicTemplate, IBellaStateFlipListener
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(1),
        new BlockVar(2m, ValueProp.Unpowered)
    ];

    // 占位图标：复用贝极星素材，后续替换为专属图标。
    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/relics/lucia_pc_85x85.png",
        IconOutlinePath: $"{Entry.ResPath}/images/relics/lucia_pc_85x85.png",
        BigIconPath: $"{Entry.ResPath}/images/relics/lucia_pc_256x256.png");

    public async Task OnBellaStateFlipped(Player player)
    {
        if (player != base.Owner)
            return;

        Flash();
        PlayerChoiceContext ctx = new ThrowingPlayerChoiceContext();
        await CardPileCmd.Draw(ctx, DynamicVars.Cards.IntValue, base.Owner);
        await CreatureCmd.GainBlock(
            base.Owner.Creature, DynamicVars.Block, null, fast: true);
    }
}
