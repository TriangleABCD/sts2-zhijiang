using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using Zhijiang.ZhijiangCode.Characters.Bella;

namespace Zhijiang.ZhijiangCode.Powers;

/// <summary>
/// 贝拉洗脚水施加的能力：你打出的下一张阴攻击牌造成 3 倍伤害。
/// 只绑定第一张带「阴」关键词的攻击牌；中途打出非阴攻击牌不会消耗该能力。
/// 参考原版 <c>GigantificationPower</c> 的伤害倍率实现。
/// </summary>
[RegisterPower]
public sealed class BellaYinTriplePower : PowerModel
{
    private sealed class Data
    {
        public AttackCommand? commandToModify;
    }

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    // 药水产生的间接效果，不显示图标。
    protected override bool IsVisibleInternal => false;

    protected override object InitInternalData() => new Data();

    public override Task BeforeAttack(AttackCommand command)
    {
        if (command.ModelSource is not CardModel cardModel)
            return Task.CompletedTask;

        if (cardModel.Owner.Creature != base.Owner)
            return Task.CompletedTask;

        if (cardModel.Type != CardType.Attack)
            return Task.CompletedTask;

        if (!command.DamageProps.IsPoweredAttack())
            return Task.CompletedTask;

        // 仅绑定"阴"属性攻击牌。
        if (!cardModel.Keywords.Contains(BellaYinYangService.YinKeywordId.GetModCardKeyword()))
            return Task.CompletedTask;

        Data data = GetInternalData<Data>();
        if (data.commandToModify != null)
            return Task.CompletedTask;

        data.commandToModify = command;
        return Task.CompletedTask;
    }

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props,
        Creature? dealer, CardModel? cardSource)
    {
        if (cardSource == null)
            return 1m;

        if (cardSource.Owner.Creature != base.Owner)
            return 1m;

        if (!props.IsPoweredAttack())
            return 1m;

        Data data = GetInternalData<Data>();
        if (data.commandToModify is not null && cardSource == data.commandToModify.ModelSource)
            return 3m;

        return 1m;
    }

    public override async Task AfterAttack(PlayerChoiceContext choiceContext, AttackCommand command)
    {
        Data data = GetInternalData<Data>();
        if (command == data.commandToModify)
        {
            data.commandToModify = null;
            await PowerCmd.Decrement(this);
        }
    }
}
