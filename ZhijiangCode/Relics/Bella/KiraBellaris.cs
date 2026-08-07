using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Zhijiang.ZhijiangCode.Characters.Bella;

namespace Zhijiang.ZhijiangCode.Relics;

// 闪耀贝极星：贝极星经"先古之民"（奥罗巴斯）事件替换后的升级版。
// 升级后攻击牌伤害加成从 +1 提升到 +3（升级牌额外 +2，共 +5）。
[RegisterRelic(typeof(BellaRelicPool))]
public sealed class KiraBellaris : ModStarterRelicTemplate
{
    // 升级遗物沿用初始遗物稀有度，保证后续仍被识别为初始遗物。
    public override RelicRarity Rarity => RelicRarity.Starter;

    // 遗物的数值。DexterityPower 和 StrengthPower 用于在本地化悬浮提示中展示能力图标。
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<DexterityPower>(1m),
        new PowerVar<StrengthPower>(2m)
    ];

    // 敏捷和力量悬浮提示。
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<DexterityPower>(),
        HoverTipFactory.FromPower<StrengthPower>()
    ];

    // 暂复用贝极星图片，后续可替换为专属资源。
    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/relics/KiraBellaris_85x85.png",
        IconOutlinePath: $"{Entry.ResPath}/images/relics/KiraBellaris_85x85.png",
        BigIconPath: $"{Entry.ResPath}/images/relics/KiraBellaris_256x256.png");

    // ---- 心之壁 → 敏捷（共用逻辑，来自 ModStarterRelicTemplate） ----
    protected override Task ApplyHeartWallDexterity(PlayerChoiceContext choiceContext, Creature creature, int amount)
    {
        return PowerCmd.Apply<BellarisHeartWallPower>(choiceContext, creature, amount, creature, null);
    }

    // ---- 闪耀贝极星独有效果：攻击牌伤害 +3（基础）/ +5（升级后） ----
    // 每场战斗开始时施加 3 层加成能力（层数即基础加成）。
    public override async Task AfterRoomEntered(AbstractRoom room)
    {
        if (room is CombatRoom)
        {
            Flash();
            await PowerCmd.Apply<BellarisStrengthPower>(
                new ThrowingPlayerChoiceContext(), base.Owner.Creature, 3, base.Owner.Creature, null);
        }
    }
}
