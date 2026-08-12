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

// RegisterRelic 会把遗物注册进指定遗物池。
// RegisterCharacterStarterRelic 会把它作为 BellaCharacter 的初始遗物。
// RegisterTouchOfOrobasRefinement 让"先古之民"（奥罗巴斯）事件把贝极星替换为闪耀贝极星。
[RegisterRelic(typeof(BellaRelicPool))]
[RegisterCharacterStarterRelic(typeof(BellaCharacter))]
[RegisterTouchOfOrobasRefinement(typeof(KiraBellaris))]
public sealed class Bellaris : ModStarterRelicTemplate
{
    // 稀有度。
    public override RelicRarity Rarity => RelicRarity.Common;

    // 遗物的数值。DexterityPower 和 StrengthPower 用于在本地化悬浮提示中展示能力图标。
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<DexterityPower>(1m),
        new PowerVar<StrengthPower>(2m)
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

    // 图片资源统一放在 AssetProfile 里配置。
    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/relics/Bellaris_85x85.png",
        IconOutlinePath: $"{Entry.ResPath}/images/relics/Bellaris_85x85.png",
        BigIconPath: $"{Entry.ResPath}/images/relics/Bellaris_256x256.png");

    // ---- 心之壁 → 敏捷（共用逻辑，来自 ModStarterRelicTemplate） ----
    protected override Task ApplyHeartWallDexterity(PlayerChoiceContext choiceContext, Creature creature, int amount)
    {
        return PowerCmd.Apply<BellarisHeartWallPower>(choiceContext, creature, amount, creature, null);
    }

    // ---- 贝极星独有效果：阴阳双面加成 ----
    // 黑拉（阴多于阳）：攻击牌伤害 +2；白拉（阳多于阴）：每打出 1 张技能牌获得 3 格挡。
    // 每场战斗开始时施加，效果由两个能力各自按当前状态判定。
    public override async Task AfterRoomEntered(AbstractRoom room)
    {
        if (room is CombatRoom)
        {
            Flash();
            // 黑拉攻击加成：层数即加成值（+2）。
            await PowerCmd.Apply<BellarisStrengthPower>(
                new ThrowingPlayerChoiceContext(), base.Owner.Creature, 2, base.Owner.Creature, null);
            // 白拉技能格挡：层数即格挡值（+3）。
            await PowerCmd.Apply<BellarisBlockPower>(
                new ThrowingPlayerChoiceContext(), base.Owner.Creature, 3, base.Owner.Creature, null);
        }
    }
}
