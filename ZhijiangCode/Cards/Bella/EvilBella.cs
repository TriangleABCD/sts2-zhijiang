using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Zhijiang.ZhijiangCode.Characters.Bella;
using Zhijiang.ZhijiangCode.Powers;

namespace Zhijiang.ZhijiangCode.Cards.Bella;

[RegisterCard(typeof(BellaCardPool))]
public sealed class EvilBella : ModCardTemplate
{
    private const int BaseEnergyCost = 2;
    private const CardType CardKind = CardType.Power;
    private const CardRarity CardRarityValue = CardRarity.Rare;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/evil_bella.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("OrbCount", 1m)
    ];

    public EvilBella() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int orbCount = DynamicVars["OrbCount"].IntValue;
        await PowerCmd.Apply<EvilBellaPower>(choiceContext, base.Owner.Creature,
            orbCount, base.Owner.Creature, this);

        // 升级后额外获得 2 个充能球栏位（未升级时依赖 OrbCmd.Channel 自动给 1 栏位）。
        if (IsUpgraded)
        {
            await OrbCmd.AddSlots(base.Owner, 2);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["OrbCount"].UpgradeValueBy(1m);
    }
}
