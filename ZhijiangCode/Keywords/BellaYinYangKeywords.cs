using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace Zhijiang.ZhijiangCode.Keywords;

// 贝拉阴阳机制的卡牌关键词注册。
// 阳/阴作为 CardKeyword 存在：既注入卡牌描述（显示在描述上方，BeforeCardDescription），
// 又可作为逻辑判定依据（card.HasModKeyword(...)），并随卡牌存档保存。
// 生成的 id：ZHIJIANG_KEYWORD_YANG / ZHIJIANG_KEYWORD_YIN。
[RegisterOwnedCardKeyword(
    "yang",
    CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]
[RegisterOwnedCardKeyword(
    "yin",
    CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]
public sealed class BellaYinYangKeywords
{
}
