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
using Zhijiang.ZhijiangCode.Powers;

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
        new PowerVar<StrengthPower>(4m)
    ];

    // 敏捷、格挡和力量悬浮提示，以及动态的当前阴阳状态提示。
    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get
        {
            var tips = new List<IHoverTip>
            {
                HoverTipFactory.FromPower<DexterityPower>(),
                HoverTipFactory.Static(StaticHoverTip.Block),
                HoverTipFactory.FromPower<StrengthPower>()
            };

            // 动态追加当前阴阳状态（白拉/黑拉）。
            if (base.Owner is { } player)
            {
                tips.Add(BellaYinYangService.IsBaiLa(player)
                    ? HoverTipFactory.FromPower<BaiLaPower>()
                    : HoverTipFactory.FromPower<HeiLaPower>());
            }

            return tips;
        }
    }

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

    // ---- 闪耀贝极星独有效果：阴阳双面加成 ----
    // 黑拉（阴多于阳）：攻击牌伤害 +4；白拉（阳多于阴）：每打出 1 张技能牌获得 6 格挡。
    // 每场战斗开始时施加，效果由两个能力各自按当前状态判定。
    public override async Task AfterRoomEntered(AbstractRoom room)
    {
        if (room is CombatRoom)
        {
            Flash();
            // 黑拉攻击加成：层数即加成值（+4）。
            await PowerCmd.Apply<BellarisStrengthPower>(
                new ThrowingPlayerChoiceContext(), base.Owner.Creature, 4, base.Owner.Creature, null);
            // 白拉技能格挡：层数即格挡值（+6）。
            await PowerCmd.Apply<BellarisBlockPower>(
                new ThrowingPlayerChoiceContext(), base.Owner.Creature, 6, base.Owner.Creature, null);
        }
    }
}
