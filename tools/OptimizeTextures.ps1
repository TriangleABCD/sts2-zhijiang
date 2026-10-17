<#
.SYNOPSIS
    把 mod 里的贴图换成一个更小的导入方案，压缩发布用 pck 的体积。

.DESCRIPTION
    Godot 默认以 Lossless 导入 PNG（1920x1080 一帧约 1.38 MB），这是 Zhijiang.pck 膨胀到
    300 MB+ 的唯一主因。实测 lossy q0.85 可把单张缩到原体积的 1/8 ~ 1/10。

    本脚本做两件事，然后重新构建（构建里的 pck 导出会触发 Godot 全量导入）：
      1. 批量改写 Zhijiang/**/*.png.import 的 compress 参数；
      2. 删掉 .godot/imported 里的 ctex 缓存 —— 只改参数不删缓存时 Godot 会跳过重导。

    ⚠️ 选人背景动画（select_bg_images，120 帧 / 占原体积 69%）对画质敏感：实测降到
    1280x720 + lossy q0.85 后玩家反馈"效果太差"，因此默认给它 -BgLossless（无损 + 原分辨率）。
    想再压背景时可以单独传 -BgMaxSize 1280 之类，但先考虑卡面之外的收益是否值得。

    原始 PNG 文件不会被修改：导入只读取源图、把编码结果写到 .godot/imported/*.ctex。
    注意 *.import 在 .gitignore 里，压缩参数属于本机设置，换机器/重新 clone 后要再跑一次。

.EXAMPLE
    tools\OptimizeTextures.ps1                                   # 默认：卡面/角色帧 lossy q0.85，选人背景无损原尺寸
    tools\OptimizeTextures.ps1 -Quality 0.7
    tools\OptimizeTextures.ps1 -BgMaxSize 1280                    # 也压选人背景（会降分辨率）
    tools\OptimizeTextures.ps1 -RestoreLossless                   # 全部还原成无损 + 原分辨率
#>
param(
    # 除选人背景外所有贴图的 Lossy 质量（0~1）。0.85 肉眼几乎无差；0.7 更小但卡面细节会糊。
    [double]$Quality = 0.85,
    # 选人背景帧的最长边像素上限（1920x1080 传 1280 得到 1280x720）；0 = 保持原始分辨率。
    [int]$BgMaxSize = 0,
    # 选人背景保持 Lossless（默认开）；传 -BgMaxSize 想压背景时记得同时去掉它。
    [switch]$BgLossless = $true,
    # 恢复所有贴图为 Godot 默认的 Lossless 导入 + 原始分辨率。
    [switch]$RestoreLossless,
    # 只改 .import 参数并清缓存，不跑构建。
    [switch]$NoBuild
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

$globalLossless = [bool]$RestoreLossless
$bgLossless = [bool]$BgLossless
if ($globalLossless) { $BgMaxSize = 0 }
# 只要显式给了 -BgMaxSize，就说明是想压背景：自动去掉无损。
if ($BgMaxSize -gt 0) { $bgLossless = $false }

$label = "Lossy q$Quality"
if ($globalLossless) { $label = 'Lossless（全部）' }
elseif ($bgLossless) { $label = "Lossy q$Quality（选人背景保持无损）" }

$utf8 = New-Object System.Text.UTF8Encoding($false)
$bgDir = Join-Path $root 'Zhijiang\images\characters\Bella\select_bg_images'

Write-Host "== 改写 .import 参数 -> $label =="
$changed = 0
Get-ChildItem (Join-Path $root 'Zhijiang') -Recurse -File -Filter *.import | ForEach-Object {
    $isBg = $_.DirectoryName -eq $bgDir
    $useLossless = $globalLossless -or ($isBg -and $bgLossless)
    $fileMode = 1
    if ($useLossless) { $fileMode = 0 }

    $text = [System.IO.File]::ReadAllText($_.FullName)
    $new = $text -replace 'compress/mode=\d+', "compress/mode=$fileMode" `
                -replace 'compress/lossy_quality=[\d.]+', "compress/lossy_quality=$Quality"
    if ($isBg) {
        $new = $new -replace 'process/size_limit=\d+', "process/size_limit=$BgMaxSize"
    }
    if ($new -ne $text) {
        [System.IO.File]::WriteAllText($_.FullName, $new, $utf8)
        $changed++
    }
}
Write-Host "   更新 $changed 个 .import"

Write-Host '== 清空 .ctex 缓存（强制重导）=='
$imported = Join-Path $root '.godot\imported'
if (Test-Path $imported) {
    $stale = Get-ChildItem $imported -File | Where-Object { $_.Extension -eq '.ctex' -or $_.Extension -eq '.md5' }
    $mb = [math]::Round((($stale | Measure-Object Length -Sum).Sum / 1MB), 1)
    $stale | Remove-Item -Force
    Write-Host "   删除 $($stale.Count) 个文件 / $mb MB"
}

if ($NoBuild) {
    Write-Host '(-NoBuild) 跳过构建；下次 dotnet build 的 pck 导出会触发全量重导。'
    return
}

Write-Host '== 重新构建 + 导出 pck（导入 + 打包可能要几分钟）=='
dotnet build (Join-Path $root 'Zhijiang.csproj') -v m
if ($LASTEXITCODE -ne 0) { throw "构建失败，退出码 $LASTEXITCODE" }

$ctex = Get-ChildItem $imported -File -Filter *.ctex -ErrorAction SilentlyContinue
$totalMb = [math]::Round((($ctex | Measure-Object Length -Sum).Sum / 1MB), 1)
Write-Host "   贴图缓存：$($ctex.Count) 个 ctex / $totalMb MB"

$props = Join-Path $root 'local.props'
if (Test-Path $props) {
    $m = [regex]::Match([System.IO.File]::ReadAllText($props), '<Sts2Dir>([^<]+)</Sts2Dir>')
    if ($m.Success) {
        $pck = Join-Path $m.Groups[1].Value.Trim() 'mods\Zhijiang\Zhijiang.pck'
        if (Test-Path $pck) {
            Write-Host ("   pck: {0} MB" -f [math]::Round((Get-Item $pck).Length / 1MB, 1))
        }
    }
}
