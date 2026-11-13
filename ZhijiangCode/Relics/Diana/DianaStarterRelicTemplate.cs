using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Scaffolding.Content;
using Zhijiang.ZhijiangCode.Characters.Diana;

namespace Zhijiang.ZhijiangCode.Relics.Diana;

/// <summary>
/// 嘉心糖系列初始遗物共用基类：进入战斗时重置王牌花色、写入本遗物的触发门槛，
/// 并施加 <see cref="DianaTrumpSuitPower" /> 负责每回合开始的结算。
/// 升级版只在 <see cref="SuitThreshold" /> 上不同（3 → 2）。
/// </summary>
public abstract class DianaStarterRelicTemplate : ModRelicTemplate
{
    // 初始遗物必须是 Starter，否则会被商店/奖励池当作普通遗物抽出来。
    public override RelicRarity Rarity => RelicRarity.Starter;

    public override bool IsAllowedInShops => false;

    /// <summary>手牌里每 N 张同花色牌触发一次。</summary>
    protected abstract int SuitThreshold { get; }

    // TODO(美术)：升级版暂无专属图标，先沿用基础版。
    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/relics/JiaXinTang_85x85.png",
        IconOutlinePath: $"{Entry.ResPath}/images/relics/JiaXinTang_85x85.png",
        BigIconPath: $"{Entry.ResPath}/images/relics/JiaXinTang_256x256.png");

    public override async Task AfterRoomEntered(AbstractRoom room)
    {
        if (room is not CombatRoom)
            return;

        Flash();

        DianaTrumpSuit.Reset();
        DianaTrumpSuit.SetThreshold(SuitThreshold);

        await PowerCmd.Apply<DianaTrumpSuitPower>(
            new ThrowingPlayerChoiceContext(), base.Owner.Creature, 1, base.Owner.Creature, null);
    }
}
