using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Godot;

namespace Fgo.Scripts.Character;

[RegisterCharacter]
public class FgoCharacter : ModCharacterTemplate<FgoCardPool, FgoRelicPool, FgoPotionPool>
{
    // 角色名称颜色
    public override Color NameColor => new(0.5f, 0.5f, 1f);

    // 能量图标轮廓颜色
    public override Color EnergyLabelOutlineColor => new(0.5f, 0.5f, 1f);

    // 地图绘制颜色
    public override Color MapDrawingColor => new(0.5f, 0.5f, 1f);

    // 人物性别（男女中立）
    public override CharacterGender Gender => CharacterGender.Masculine;

    // 初始血量和金币
    public override int StartingHp => 80;
    public override int StartingGold => 99;

    public override CharacterAssetProfile AssetProfile => CharacterAssetProfiles.Merge(
        CharacterAssetProfiles.Ironclad(),
        new CharacterAssetProfile(
            new CharacterSceneAssetSet(
                // 人物模型tscn路径。
                "res://Fgo/scenes/guda.tscn",
                // 能量表盘tscn路径。
                "res://Fgo/scenes/fgo_energy_counter.tscn",
                // 商店人物场景。
                "res://Fgo/scenes/guda_merchant.tscn",
                // 篝火休息场景。
                "res://Fgo/scenes/guda_rest_site.tscn"
            ),
            new CharacterUiAssetSet(
                // 对于图片，只要是godot支持的格式都可以，例如png,jpg,svg等等，之后不再说明
                // 人物头像路径。自适应大小。
                "res://Fgo/images/charui/character_icon_fgo.png",
                // 游戏左上角头像、角色统计页头像、每日挑战角色头像。这个是场景而不是图片。参考下方附赠资源搭建。
                IconPath: "res://Fgo/scenes/fgo_icon.tscn",
                // 人物选择背景。
                CharacterSelectBgPath: "res://Fgo/scenes/fgo_bg.tscn",
                // 人物选择图标。
                CharacterSelectIconPath: "res://Fgo/images/charui/char_select_fgo.png",
                // 人物选择图标-锁定状态。
                CharacterSelectLockedIconPath: "res://Fgo/images/charui/char_select_fgo_locked.png",
                // 人物选择过渡动画。
                // CharacterSelectTransitionPath: "res://Fgo/materials/transitions/ironclad_transition_mat.tres",
                // 地图上的角色标记图标、表情轮盘上的角色头像。
                MapMarkerPath: "res://Fgo/images/charui/map_marker_fgo.png"
            ),
            new CharacterVfxAssetSet(
                // 卡牌拖尾场景。
                // TrailPath: "res://Fgo/scenes/vfx/card_trail_ironclad.tscn"
            ),
            Audio: new CharacterAudioAssetSet(
                // 攻击音效
                // AttackSfx: null,
                // 施法音效
                // CastSfx: null,
                // 死亡音效
                // DeathSfx: null,
                // 角色选择音效
                // CharacterSelectSfx: null,
                // 过渡音效
                // CharacterTransitionSfx: "event:/sfx/ui/wipe_ironclad"
            ),
            Multiplayer: new CharacterMultiplayerAssetSet(
                // 多人模式-手指。
                // ArmPointingTexturePath: null,
                // 多人模式剪刀石头布-石头。
                // ArmRockTexturePath: null,
                // 多人模式剪刀石头布-布。
                // ArmPaperTexturePath: null,
                // 多人模式剪刀石头布-剪刀。
                // ArmScissorsTexturePath: null
            )
            // 其余如果有需要自行取消注释使用
            // Spine: null,
            // VisualCues: null, // 帧动画静态图人物使用，查看角色动画一章
            // WorldProceduralVisuals: null,
            // 以下为让遗物根据你的人物展现不同的图像资源，在列表里添加即可
            // VanillaCardVisualOverrides: [],
            // VanillaRelicVisualOverrides: [
            //     new (CharacterOwnedVanillaRelicModelId.YummyCookie, new("res://Fgo/icon.svg")) // 美味饼干覆盖
            // ],
            // VanillaPotionVisualOverrides: []
        ));

    // 攻击和施法动画延迟，以对齐动画
    public override float AttackAnimDelay => 0f;
    public override float CastAnimDelay => 0f;

    // 如果你的人物不需要时间线小故事，加上这句。
    public override bool RequiresEpochAndTimeline => false;

    // 自动转换人物场景，让你不需要手动挂脚本。复制即可。
    protected override NCreatureVisuals? TryCreateCreatureVisuals()
    {
        return RitsuGodotNodeFactories.CreateFromScenePath<NCreatureVisuals>(AssetProfile.Scenes!.VisualsPath!);
    }

    // 初始卡组，或者在卡牌类上用RegisterCharacterStarterCard就不用写这个
    // protected override IEnumerable<StartingDeckEntry> StartingDeckEntries => [
    //     new(typeof(FgoCardModel), 5)
    // ];

    // 初始遗物，或者在遗物类上用RegisterCharacterStarterRelic就不用写这个
    // protected override IEnumerable<Type> StartingRelicTypes => [
    //     typeof(Akabeko)
    // ];

    // 攻击建筑师的攻击特效列表
    public override List<string> GetArchitectAttackVfx()
    {
        return
        [
            "vfx/vfx_attack_blunt",
            "vfx/vfx_heavy_blunt",
            "vfx/vfx_attack_slash",
            "vfx/vfx_bloody_impact",
            "vfx/vfx_rock_shatter"
        ];
    }
}