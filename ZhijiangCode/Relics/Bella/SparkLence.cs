using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;
using Zhijiang.ZhijiangCode.Characters.Bella;

namespace Zhijiang.ZhijiangCode.Relics;

/// <summary>
/// 神光棒：拾起时从牌组选择最多 4 张非中立（含阳/阴标签）卡牌，反转其阴阳属性。
/// </summary>
[RegisterRelic(typeof(BellaRelicPool))]
public sealed class SparkLence : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public override bool HasUponPickupEffect => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("MaxSelect", 4m)
    ];

    // 占位图标：复用贝极星素材，后续替换为专属图标。
    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/relics/spark_lence_85x85.png",
        IconOutlinePath: $"{Entry.ResPath}/images/relics/spark_lence_85x85.png",
        BigIconPath: $"{Entry.ResPath}/images/relics/spark_lence_256x256.png");

    public override async Task AfterObtained()
    {
        var player = base.Owner;
        var yang = BellaYinYangService.YangKeywordId.GetModCardKeyword();
        var yin = BellaYinYangService.YinKeywordId.GetModCardKeyword();

        var prefs = new CardSelectorPrefs(SelectionScreenPrompt, 0, DynamicVars["MaxSelect"].IntValue)
        {
            Cancelable = true
        };

        var selected = await CardSelectCmd.FromDeckGeneric(
            player,
            prefs,
            card => card.Keywords.Contains(yang) || card.Keywords.Contains(yin));

        foreach (var card in selected)
        {
            bool hasYang = card.Keywords.Contains(yang);
            bool hasYin = card.Keywords.Contains(yin);
            if (hasYang && !hasYin)
            {
                card.RemoveKeyword(yang);
                card.AddKeyword(yin);
            }
            else if (hasYin && !hasYang)
            {
                card.RemoveKeyword(yin);
                card.AddKeyword(yang);
            }
            CardCmd.Preview(card);
        }

        BellaYinYangService.NotifyRelicCountChanged(player);
    }
}
