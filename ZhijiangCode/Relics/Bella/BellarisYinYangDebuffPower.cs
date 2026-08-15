using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Zhijiang.ZhijiangCode.Characters.Bella;

namespace Zhijiang.ZhijiangCode.Relics;

/// <summary>
/// 贝极星遗物的阴阳代价控制器（隐藏能力）：
/// 白拉状态失去 1+|d|÷3 点力量；黑拉状态失去 1+|d|÷3 点敏捷。
/// 通过施加原版 StrengthPower / DexterityPower 负层数实现（带图标可见、持续整场战斗）。
/// 由 BellaYinYangService 在状态翻转时调用 <see cref="Sync" /> 撤销旧代价、施加新代价；
/// 战斗结束能力随战斗状态自动清除。
/// 仅贝极星施加；闪耀贝极星不施加本能力（无代价）。
/// </summary>
public sealed class BellarisYinYangDebuffPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    // 控制器本身不显示图标；代价通过原版力量/敏捷能力的图标展示。
    protected override bool IsVisibleInternal => false;

    private enum DebuffKind
    {
        None,
        Strength,
        Dexterity
    }

    // 当前已施加的代价类型与数值，用于状态翻转时精确撤销。
    private DebuffKind _appliedKind = DebuffKind.None;
    private int _appliedAmount;

    /// <summary>
    /// 按当前阴阳状态同步代价。状态或差值无变化时不做任何事；
    /// 变化时先撤销旧代价，再施加新代价。
    /// </summary>
    public async Task Sync(PlayerChoiceContext choiceContext)
    {
        if (base.Owner.Player is not { } player)
            return;

        int magnitude = BellaYinYangService.ComputeMagnitude(player);
        DebuffKind desiredKind = BellaYinYangService.IsBaiLa(player)
            ? DebuffKind.Strength
            : DebuffKind.Dexterity;
        int desiredAmount = magnitude + 1;

        if (_appliedKind == desiredKind && _appliedAmount == desiredAmount)
            return;

        // 撤销旧代价（静默，避免图标闪烁）。
        switch (_appliedKind)
        {
            case DebuffKind.Strength:
                await PowerCmd.Apply<StrengthPower>(
                    choiceContext, base.Owner, _appliedAmount, base.Owner, null, silent: true);
                break;
            case DebuffKind.Dexterity:
                await PowerCmd.Apply<DexterityPower>(
                    choiceContext, base.Owner, _appliedAmount, base.Owner, null, silent: true);
                break;
        }

        // 施加新代价：原版力量/敏捷能力，负值带图标可见。
        if (desiredKind == DebuffKind.Strength)
            await PowerCmd.Apply<StrengthPower>(
                choiceContext, base.Owner, -desiredAmount, base.Owner, null);
        else
            await PowerCmd.Apply<DexterityPower>(
                choiceContext, base.Owner, -desiredAmount, base.Owner, null);

        _appliedKind = desiredKind;
        _appliedAmount = desiredAmount;
    }
}
