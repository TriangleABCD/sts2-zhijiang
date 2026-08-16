using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Keywords;
using Zhijiang.ZhijiangCode.Characters.Bella;
using Zhijiang.ZhijiangCode.SecondResource;

namespace Zhijiang.ZhijiangCode.Cards.Bella;

// A.T. 立场：获得格挡，数值等于当前心之壁除以除数（5→4）向下取整。
// 类名 AtField（Slugify → AT_FIELD，与分析器/本地化 key 一致）。
[RegisterCard(typeof(BellaCardPool))]
public sealed class AtField : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Common;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;

    // 卡图待补：占位路径指向尚不存在的资源（构建会有 RITSU013 警告，方便日后找出缺卡图的牌）。
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/at_field.png");

    // 除数（5 → 升级后 4）：格挡 = 当前心之壁 ÷ 除数（向下取整）。
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Divisor", 5m)
    ];

    // 阴阳属性：A.T. 立场为阳牌。
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        BellaYinYangService.YangKeywordId.GetModCardKeyword()
    ];

    public AtField() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int heartWall = SecondaryResourceCmd.Get(base.Owner, HeartWall.HeartWallId);
        int block = heartWall / DynamicVars["Divisor"].IntValue;
        if (block > 0)
        {
            await CreatureCmd.GainBlock(base.Owner.Creature, block, ValueProp.Move, cardPlay);
        }
    }

    protected override void OnUpgrade()
    {
        // 除数 5 → 4。
        DynamicVars["Divisor"].UpgradeValueBy(-1m);
    }
}
