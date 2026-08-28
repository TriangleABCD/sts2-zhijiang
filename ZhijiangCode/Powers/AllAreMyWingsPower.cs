using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Combat.Ui.ExtraCornerAmountLabels;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;
using Zhijiang.ZhijiangCode.Characters.Bella;

namespace Zhijiang.ZhijiangCode.Powers;

/// <summary>
/// 都是我的翅膀能力：若玩家本回合打出的阳牌数等于阴牌数，下回合获得 {Amount} 点能量。
/// 图标上有两个额外角标：右上角=本回合阳牌数，右下角=本回合阴牌数（纯数字）。
/// 主角标隐藏（StackType.Single 不渲染主计数），不再显示平衡差或奖励能量。
/// Amount 仍保存奖励能量 1/2，用于 hover tip 与回合结束结算。
/// </summary>
[RegisterPower]
public sealed class AllAreMyWingsPower : ModPowerTemplate, IPowerExtraIconAmountLabelSpecsProvider
{
    private int _pendingReward;
    private int _yangThisTurn;
    private int _yinThisTurn;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    protected override bool IsVisibleInternal => true;

    /// <summary>
    /// 额外角标：右上角显示本回合阳牌数、右下角显示本回合阴牌数（纯数字）。
    /// 跟随 DisplayAmountChanged 刷新（打牌/回合开始/回合结束都会触发 InvokeDisplayAmountChanged）。
    /// </summary>
    public IReadOnlyList<ExtraIconAmountLabelSpec> GetPowerExtraIconAmountLabelSpecs()
    {
        return
        [
            // 右上角=阳（白色），右下角=阴（暗红色），用富文本标签上色。
            ExtraIconAmountLabelSpec.RichText(
                ExtraIconAmountLabelCorner.TopRight,
                $"[color=#ffffff]{_yangThisTurn}[/color]"),
            ExtraIconAmountLabelSpec.RichText(
                ExtraIconAmountLabelCorner.BottomRight,
                $"[color=#8b0000]{_yinThisTurn}[/color]"),
        ];
    }

    // 专属能力图标：all_are_my_wings_power_64x64.png / all_are_my_wings_power_256x256.png（待补成品图）。
    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/characters/Bella/all_are_my_wings_power_64x64.png",
        BigIconPath: $"{Entry.ResPath}/images/characters/Bella/all_are_my_wings_power_256x256.png");

    /// <summary>
    /// 每次自己打出卡牌后更新本回合阳/阴计数并刷新图标角标。
    /// Replay 类多段重复打出只算第一段，与 BellaYinYangService 的口径一致。
    /// </summary>
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner?.Creature != base.Owner)
            return;
        if (!cardPlay.IsFirstInSeries)
            return;

        var keywords = cardPlay.Card.Keywords;
        if (keywords.Contains(BellaYinYangService.YangKeywordId.GetModCardKeyword()))
        {
            _yangThisTurn++;
            InvokeDisplayAmountChanged();
        }
        else if (keywords.Contains(BellaYinYangService.YinKeywordId.GetModCardKeyword()))
        {
            _yinThisTurn++;
            InvokeDisplayAmountChanged();
        }
    }

    public override Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        // 仅拥有者回合结束时结算：本回合阴阳平衡则记下奖励。
        if (side == Owner.Side && _yangThisTurn == _yinThisTurn)
        {
            _pendingReward = Amount;
        }
        return Task.CompletedTask;
    }

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != Owner.Side)
            return;
        if (base.Owner.Player is not { } player)
            return;

        // 上回合若平衡，现在发放能量。
        if (_pendingReward > 0)
        {
            await PlayerCmd.GainEnergy(_pendingReward, player);
            _pendingReward = 0;
        }

        // 新回合清零计数与角标。
        _yangThisTurn = 0;
        _yinThisTurn = 0;
        InvokeDisplayAmountChanged();
    }
}