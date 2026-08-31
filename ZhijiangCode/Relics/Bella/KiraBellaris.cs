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
// 与贝极星拥有相同的阴阳馈赠（白拉技能格挡 / 黑拉攻击伤害），但没有阴阳代价（不失去力量/敏捷）。
[RegisterRelic(typeof(BellaRelicPool))]
public sealed class KiraBellaris : BellaStarterRelicTemplate
{
    // 升级遗物沿用初始遗物稀有度，保证后续仍被识别为初始遗物。
    public override RelicRarity Rarity => RelicRarity.Starter;

    // 初始遗物不可在商店出现（防御性限制；Starter 稀有度本就不会被商店抽取）。
    public override bool IsAllowedInShops => false;

    // 遗物的数值。DexterityPower 用于在本地化悬浮提示中展示能力图标。
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<DexterityPower>(1m)
    ];

    // 敏捷与格挡悬浮提示，以及动态的当前阴阳状态提示。
    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get
        {
            var tips = new List<IHoverTip>
            {
                HoverTipFactory.FromPower<DexterityPower>(),
                HoverTipFactory.Static(StaticHoverTip.Block)
            };

            // 动态追加当前阴阳状态（白拉/黑拉）。
            // 仅可变的遗物实例才读取 Owner；规范（canonical）实例会被
            // TouchOfOrobas 等流程查询 HoverTips，此时没有所属玩家。
            if (IsMutable && base.Owner is { } player)
            {
                tips.Add(BellaYinYangService.IsBaiLa(player)
                    ? HoverTipFactory.FromPower<BaiLaPower>()
                    : HoverTipFactory.FromPower<HeiLaPower>());
            }

            return tips;
        }
    }

    // 图片资源。暂复用贝极星素材，后续可替换为专属资源。
    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/relics/KiraBellaris_85x85.png",
        IconOutlinePath: $"{Entry.ResPath}/images/relics/KiraBellaris_85x85.png",
        BigIconPath: $"{Entry.ResPath}/images/relics/KiraBellaris_256x256.png");

    // ---- 心之壁 → 敏捷（共用逻辑，来自 ModStarterRelicTemplate） ----
    protected override Task ApplyHeartWallDexterity(PlayerChoiceContext choiceContext, Creature creature, int amount)
    {
        return PowerCmd.Apply<BellarisHeartWallPower>(choiceContext, creature, amount, creature, null);
    }

    // ---- 闪耀贝极星独有效果：阴阳姿态的馈赠（无代价） ----
    // 白拉时每打出 1 张技能牌获得 1+|d|÷3 格挡；黑拉时每打出 1 张攻击牌对随机敌人造成 1+|d|÷3 伤害。
    // 数值与贝极星一致，但不施加力量/敏捷代价。
    public override async Task AfterRoomEntered(AbstractRoom room)
    {
        if (room is CombatRoom)
        {
            Flash();
            // 白拉技能格挡。
            await PowerCmd.Apply<BellarisBlockPower>(
                new ThrowingPlayerChoiceContext(), base.Owner.Creature, 1, base.Owner.Creature, null);
            // 黑拉攻击伤害。
            await PowerCmd.Apply<BellarisHeiLaAttackPower>(
                new ThrowingPlayerChoiceContext(), base.Owner.Creature, 1, base.Owner.Creature, null);
        }
    }
}
