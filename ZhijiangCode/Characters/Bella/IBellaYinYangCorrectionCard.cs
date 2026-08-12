namespace Zhijiang.ZhijiangCode.Characters.Bella;

/// <summary>
/// 标记参与阴阳差值修正的卡牌接口。
/// 差值修正 Power 通过该接口识别哪些卡牌的伤害/格挡参与 `±(d ÷ 3)` 修正。
/// 不实现此接口的卡牌天然豁免差值修正。
/// </summary>
public interface IBellaYinYangCorrectionCard
{
    /// <summary>
    /// 是否参与伤害修正（ModifyDamageAdditive）。
    /// </summary>
    bool CorrectDamage { get; }

    /// <summary>
    /// 是否参与格挡修正（ModifyBlockAdditive）。
    /// </summary>
    bool CorrectBlock { get; }
}
