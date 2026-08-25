using System.Collections.Generic;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Keywords;
using Zhijiang.ZhijiangCode.Characters.Bella;

namespace Zhijiang.ZhijiangCode.Cards.Status;

// 高雅（Elegant）：状态牌，无法打出，只能占据手牌/牌堆位置（与「牛批」对称）。
// 注册到原版 StatusCardPool（无色状态池），不绑定到任何角色。
// 阴阳属性：阳——带标签参与阴阳计数，是「牛批」（阴）的对称状态牌。
[RegisterCard(typeof(StatusCardPool))]
public sealed class Elegant : ModCardTemplate
{
    // 状态牌不可升级。
    public override int MaxUpgradeLevel => 0;

    // 卡图占位：指向尚不存在的资源（构建会有 RITSU013 警告，方便日后找出缺卡图的牌）。
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/elegant.png");

    // 状态牌：无法打出 + 阳。
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Unplayable,
        BellaYinYangService.YangKeywordId.GetModCardKeyword()
    ];

    public Elegant() : base(-1, CardType.Status, CardRarity.Status, TargetType.None, showInCardLibrary: false)
    {
    }

    /// <summary>
    /// 生成 <paramref name="amount" /> 张「高雅」，归 <paramref name="owner" /> 所有。
    /// </summary>
    public static IEnumerable<Elegant> Create(Player owner, int amount, ICombatState combatState)
    {
        List<Elegant> cards = new();
        for (int i = 0; i < amount; i++)
            cards.Add(combatState.CreateCard<Elegant>(owner));
        return cards;
    }
}
