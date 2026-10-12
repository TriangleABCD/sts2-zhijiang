## 卡图缺失

- 都是我的翅膀: all_are_my_wings.png
- 贝拉博士: doctor_bella.png
- 小打火机: little_lighter.png
- 小企鹅: little_penguin.png
- 小滑板: little_skateboard.png
- 尼拉: scorched_bella.png
- 挠头: scratch_head.png
- 短裙: short_skirt.png
- 天使贝拉: angel_bella.png
- 烧烤贝极星: bbq_bellaris.png
- Maria: maria.png
- NPK48: npk48.png
- 锅锤打击: pot_hammer_strike.png
- 老公x我: bella_do_me.png
- 贝丽君: bella_teng.png
- 沸腾期待: boiling_expectation.png
- 回旋镖: boomerang.png
- 队长的权力是无限的: captains_power_is_infinite.png
- 勇气大爆发: courage_burst.png
- 武斗会冠军: dance_battle_champion.png
- 杨桃干: dead_bellaris.png
- 晕3D: dizzy3d.png
- 巨龙腾飞: giant_dragon_soars.png
- 大咪: great_kitty.png
- 妹宝: little_sister_bella.png
- 长路归航: long_road_home.png
- 恋爱心事: loves_secret.png
- 一个女人满足不了我: one_woman_cant_satisfy_me.png
- 牛不灭: ox_never_die.png
- 可靠的拉姐: reliable_sister_la.png
- 日程表: schedule.png
- 牛牛民: cow_lover.png
- 双倍的寡妇: double_widow.png
- 情感障碍: emotional_disorder.png
- 绝无拉我: never_forgive.png
- 小伙伴对不起: sorry_little_friends.png
- 那我就是笨嘛: then_im_just_stupid.png
- 天猫精灵: tmall_genie.png
- 分奴: tryhard.png
- 奥特大英雄: ultraman.png
- 初号机: unit01.png
- 白龙马: white_dragon_horse.png
- 你不选红色: you_dont_choose_red.png
- 女女关系领域大神: yuri_domain_expert.png
- 枝江小百合: zhijiang_les.png
- 高雅: elegant.png
- 牛P: np.png
- 对抗路情侣: couple_on_the_clash_lane.png
- 铁棍: iron_stick.png
- 拖拉机下山: tractor_descends_the_mountain.png
- 上山: ascend_the_mountain.png
- 贝拉气晕: fainted_by_bella.png

## 能力图标缺失

> 目录 `Zhijiang/images/characters/Bella/`。每个能力两张图：`_64x64.png`（小图标）+ `_256x256.png`（大图标），文件名必须与能力 `PowerAssetProfile` 里的 `IconPath` / `BigIconPath` 完全一致。
> 代码已指向下列路径（文件不存在，构建会有 RITSU013 警告），补图时按这些文件名放进去即可。

- 对抗路情侣能力: `couple_on_the_clash_lane_power_64x64.png` + `couple_on_the_clash_lane_power_256x256.png`
- 贝拉气晕能力: `fainted_by_bella_power_64x64.png` + `fainted_by_bella_power_256x256.png`

## 卡图更新

> 目录 `Zhijiang/images/cards/Bella/`。文件名必须与卡牌 `AssetProfile.PortraitPath` 完全一致。

- 真情实感: heart_felt.png
- 预防针了: prevention_shot.png
- 防御: bella_defend.png
- 拉龙来袭: bella_rex_is_coming.png
- 贝拉求雨: bella_prays_for_rain.png
- 冰山美人: ice_beauty.png
- A.T. 立场: at_field.png
- 芭蕾舞者: ballerina.png
- 虚拟感: virtual_sense.png
- 了转反: turn_over.png

## 遗物修改

### 闪耀贝极星图标重做

代码侧：`ZhijiangCode/Relics/Bella/KiraBellaris.cs` 的 `AssetProfile`（第 61~64 行）。
图放这里：**`Zhijiang/images/relics/`**（遗物图没有 Bella 子目录，和其它遗物图平级）。

| 要重做的文件 | 尺寸 | 代码字段 | 会出现在哪 |
|---|---|---|---|
| `Zhijiang/images/relics/KiraBellaris_85x85.png` | 85×85 PNG | `IconPath` | 遗物栏小图标、遗物奖励/悬停列表里的小图标 |
| `Zhijiang/images/relics/KiraBellaris_256x256.png` | 256×256 PNG | `BigIconPath` | 商店与遗物详情的大图 |

- 这两张图现在都存在（是待替换的占位版），**只改内容、文件名不要动**，尺寸保持 85×85 / 256×256（已实测现有文件就是这个尺寸），否则其它 UI 会缩放变形。
- 对照参考：同目录的 `Bellaris_85x85.png` / `Bellaris_256x256.png` 是**贝极星**（未升级版）的成品图；闪耀贝极星是它的奥罗巴斯升级版，出图时建议保持同风格可区分（如加高光/闪耀元素）。
- ⚠️ `IconOutlinePath`（轮廓/剪影图）**目前和 `IconPath` 指向同一个彩色文件**，所以没有单独的剪影图要出。若之后想加原版那种纯色剪影效果：先按 `KiraBellaris_outline_85x85.png` 出图（85×85），再把 `KiraBellaris.cs` 里的 `IconOutlinePath` 改指到它，否则文件不会被读取。
- 替换 PNG 后重新构建一次（Godot 会重新导入资源、刷新 `.import`），无需改任何代码。
- `KiraBellaris` 没有其它图片引用（事件 JSON、场景都不用额外改图）。

## 其他

- mod 图标图片
- mod 体积太大











