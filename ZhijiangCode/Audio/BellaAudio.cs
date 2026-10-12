using STS2RitsuLib.Audio;

using Zhijiang.ZhijiangCode.Settings;

namespace Zhijiang.ZhijiangCode.Audio;

/// <summary>
/// 贝拉卡牌音效的统一播放入口：音频文件放在 <c>Zhijiang/audios/</c> 下（会被打包进 pck，
/// 路径即 <c>res://Zhijiang/audios/{文件名}</c>），调用处只需传文件名。
/// </summary>
internal static class BellaAudio
{
    private const string AudioDir = "res://Zhijiang/audios";

    // 卡牌本身已有原版默认音效，自定义音效是叠加，音量压到 0.6 避免糊在一起。
    private const float DefaultVolume = 0.6f;

    /// <summary>播放一次性卡牌音效，例如 <c>BellaAudio.PlayCardSfx("ygnn.ogg")</c>。</summary>
    public static void PlayCardSfx(string fileName, float volume = DefaultVolume)
    {
        // 设置里的总开关（实时读取，改完立刻生效）；关掉后原版自带的卡牌音效仍然会播。
        if (!ZhijiangSettingsAccess.CardSfxEnabled)
            return;

        GameAudioService.Shared.PlayOneShot(
            AudioSource.ResourceFile($"{AudioDir}/{fileName}"),
            new AudioPlaybackOptions
            {
                Volume = volume,
                // 跟随原版音效音量/静音设置（默认即 true，写出来是为了意图明确）。
                UseVanillaRouting = true,
            });
    }
}
