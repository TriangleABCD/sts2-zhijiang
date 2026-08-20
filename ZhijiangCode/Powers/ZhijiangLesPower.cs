using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Zhijiang.ZhijiangCode.Characters.Bella;

namespace Zhijiang.ZhijiangCode.Powers;

/// <summary>
/// 枝江小百合能力：每打出 {ContrastThreshold} 张反差牌（与当前黑白拉状态阴阳相反的牌）
/// 获得 1 点敏捷（战斗内永久）。Amount 显示距下次触发还差的反差牌数（倒计时），触发后重置。
/// 反差牌判定复用 BellaYinYangService.IsContrastCard（含「了转反」判定翻转），
/// Replay 类多段重复打出只算第一段（IsFirstInSeries，与反差牌计数口径一致）。
/// </summary>
[RegisterPower]
public sealed class ZhijiangLesPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override bool IsVisibleInternal => true;

    // 专属能力图标：zhijiang_les_power_64x64.png / zhijiang_les_power_256x256.png（待替换为成品图）。
    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/characters/Bella/zhijiang_les_power_64x64.png",
        BigIconPath: $"{Entry.ResPath}/images/characters/Bella/zhijiang_les_power_256x256.png");

    // 触发阈值（卡牌施加时写入；基础 5、升级 3）。
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("ContrastThreshold", 5m)
    ];

    /// <summary>设定本实例的触发阈值（Amount 用于显示剩余倒计时，阈值需单独存储）。</summary>
    public void SetThreshold(decimal threshold)
    {
        AssertMutable();
        base.DynamicVars["ContrastThreshold"].BaseValue = threshold;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // Replay 类多段重复打出只算第一段。
        if (!cardPlay.IsFirstInSeries)
            return;

        // 仅统计自己打出的牌。
        if (cardPlay.Card.Owner?.Creature != base.Owner)
            return;

        if (base.Owner.Player is not { } player)
            return;

        if (!BellaYinYangService.IsContrastCard(player, cardPlay.Card))
            return;

        if (base.Amount <= 1m)
        {
            // 到达阈值：获得 1 点敏捷，倒计时重置。
            await PowerCmd.Apply<DexterityPower>(choiceContext, base.Owner, 1m, base.Owner, null);
            await PowerCmd.ModifyAmount(choiceContext, this,
                base.DynamicVars["ContrastThreshold"].BaseValue - base.Amount, base.Owner, null);
        }
        else
        {
            await PowerCmd.Decrement(this);
        }
    }
}
