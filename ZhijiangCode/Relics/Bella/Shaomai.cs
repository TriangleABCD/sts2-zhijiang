using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Zhijiang.ZhijiangCode.Characters.Bella;

namespace Zhijiang.ZhijiangCode.Relics;

/// <summary>
/// 烧麦：拾起时获得 17 点最大生命（一次性遗物）。
/// </summary>
[RegisterRelic(typeof(BellaRelicPool))]
public sealed class Shaomai : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public override bool HasUponPickupEffect => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new MaxHpVar(17m)
    ];

    // 占位图标：复用贝极星素材，后续替换为专属图标。
    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/relics/shaomai_85x85.png",
        IconOutlinePath: $"{Entry.ResPath}/images/relics/shaomai_85x85.png",
        BigIconPath: $"{Entry.ResPath}/images/relics/shaomai_256x256.png");

    public override async Task AfterObtained()
    {
        await CreatureCmd.GainMaxHp(base.Owner.Creature, base.DynamicVars.MaxHp.BaseValue);
    }
}
