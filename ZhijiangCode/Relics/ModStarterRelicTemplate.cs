using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Scaffolding.Content;
using Zhijiang.ZhijiangCode.SecondResource;

namespace Zhijiang.ZhijiangCode.Relics;

/// <summary>
/// 本 mod 所有角色初始遗物的共用基类。
/// 提供心之壁 → 敏捷的通用逻辑。
/// 子类通过覆写 ApplyHeartWallDexterity 指定各角色专属的临时敏捷能力类型。
/// </summary>
public abstract class ModStarterRelicTemplate : ModRelicTemplate
{
    // 每回合开始时，根据心之壁数值获得临时敏捷（仅本回合有效）。
    // 公式：敏捷 = 心之壁 ÷ 15（整除）。
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        Creature creature = base.Owner.Creature;
        int heartWallAmount = SecondaryResourceCmd.Get(player, HeartWall.HeartWallId);
        int dexterityAmount = heartWallAmount / 15;
        if (dexterityAmount > 0)
        {
            Flash();
            await ApplyHeartWallDexterity(choiceContext, creature, dexterityAmount);
        }
    }

    // 子类覆写此方法来施加各角色专属的临时敏捷能力。
    protected abstract Task ApplyHeartWallDexterity(PlayerChoiceContext choiceContext, Creature creature, int amount);
}
