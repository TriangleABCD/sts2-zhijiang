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

// RegisterRelic 会把遗物注册进指定遗物池。
// RegisterCharacterStarterRelic 会把它作为 BellaCharacter 的初始遗物。
[RegisterRelic(typeof(BellaRelicPool))]
[RegisterCharacterStarterRelic(typeof(BellaCharacter))]
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

    // 敏捷和力量悬浮提示。
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<DexterityPower>(),
        HoverTipFactory.FromPower<StrengthPower>()
    ];

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

    // ---- 贝极星独有效果：攻击牌伤害 +2（基础）/ +5（升级后） ----
    // 每场战斗开始时施加。
    public override async Task AfterRoomEntered(AbstractRoom room)
    {
        if (room is CombatRoom)
        {
            Flash();
            await PowerCmd.Apply<BellarisStrengthPower>(
                new ThrowingPlayerChoiceContext(), base.Owner.Creature, 1, base.Owner.Creature, null);
        }
    }
}
