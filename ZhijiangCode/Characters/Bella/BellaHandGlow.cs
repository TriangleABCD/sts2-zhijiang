using Godot;
using MegaCrit.Sts2.Core.Nodes.Cards;
using STS2RitsuLib.Scaffolding.Cards.HandOutline;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Keywords;

namespace Zhijiang.ZhijiangCode.Characters.Bella;

/// <summary>
/// 贝拉手牌泛光：
/// 战斗出牌阶段，手牌中与当前黑白拉状态同向的牌泛金色光（白拉=阳牌、黑拉=阴牌），
/// 反差牌泛红色光（白拉=阴牌、黑拉=阳牌）；无阴阳标签的中立牌不发光。
/// 使用 RitsuLib <see cref="ModCardHandOutlineRegistry"/> 的逐帧刷新规则，状态翻转时自动变色。
/// </summary>
public static class BellaHandGlow
{
    public static void Register()
    {
        // 注册在 ModCardTemplate 基类上，覆盖本 mod 全部卡牌（均为贝拉卡）。
        ModCardHandOutlineRegistry.Register<ModCardTemplate>(
            ModCardHandOutlineRules.Switch<ModCardTemplate>(
                card =>
                {
                    // 只作用于贝拉玩家，避免影响其他角色/其他 mod 的 RitsuLib 模板卡。
                    var player = card.Owner;
                    if (player is not { Character: BellaCharacter })
                        return null;

                    bool isYang = card.Keywords.Contains(
                        BellaYinYangService.YangKeywordId.GetModCardKeyword());
                    bool isYin = card.Keywords.Contains(
                        BellaYinYangService.YinKeywordId.GetModCardKeyword());
                    if (!isYang && !isYin)
                        return null;

                    // 与阴阳状态判定完全同口径：反差牌红，其余（同向）牌金。
                    return BellaYinYangService.IsContrastCard(player, card)
                        ? NCardHighlight.red
                        : NCardHighlight.gold;
                },
                priority: 0,
                visibleWhenUnplayable: true,
                refreshEveryFrame: true));
    }
}
