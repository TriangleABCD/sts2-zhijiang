using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Keywords;
using Zhijiang.ZhijiangCode.Characters.Bella;
using Zhijiang.ZhijiangCode.Powers;

namespace Zhijiang.ZhijiangCode.Cards.Bella;

// 小土豆雷：1 费攻击牌。打出后开始 3 回合倒计时，倒计时结束时对当前血量最多的敌人
// 造成 17→20 点伤害（固定伤害，不受力量修正）。参考原版 TheBomb / TheBombPower。
// 目标类型为 Self：爆炸时自动选取目标，不锁定打出时的敌人（若该敌人在倒计时期间死亡则换目标）。
[RegisterCard(typeof(BellaCardPool))]
public sealed class PotatoMine : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Common;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;

    // 卡图待补：占位路径指向尚不存在的资源（构建会有 RITSU013 警告，方便日后找出缺卡图的牌）。
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/potato_mine.png");

    // 倒计时回合数与爆炸伤害（同原版 TheBomb 用普通 DynamicVar，卡面预览不掺力量加成）。
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Turns", 3m),
        new DynamicVar("BombDamage", 17m)
    ];

    // 阴阳属性：小土豆雷为阴牌。
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        BellaYinYangService.YinKeywordId.GetModCardKeyword()
    ];

    public PotatoMine() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        PotatoMinePower? power = await PowerCmd.Apply<PotatoMinePower>(choiceContext, base.Owner.Creature,
            DynamicVars["Turns"].BaseValue, base.Owner.Creature, this);
        power?.SetDamage(DynamicVars["BombDamage"].BaseValue);
    }

    protected override void OnUpgrade()
    {
        // 爆炸伤害 17 → 20。
        DynamicVars["BombDamage"].UpgradeValueBy(3m);
    }
}
