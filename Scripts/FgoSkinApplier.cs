using Godot;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;

namespace Fgo.Scripts;

/// <summary>
///     角色皮肤的纹理加载与套用。前 19 个皮肤为 res://Fgo/images/char/Master{index}.png（Master0-Master18），
///     其后为具名贴图（Monte_Cristo / Cat_Arcueid_Brunestud / Romani_Archaman / Guda）；
///     与 guda.tscn 中 Visuals 精灵的默认贴图（Master0）一致；切换皮肤即替换该精灵贴图。
/// </summary>
internal static class FgoSkinApplier
{
    public const int SkinCount = 23;

    // Master0-Master18 之外的皮肤使用具名贴图（无编号规律），单独列出。
    private const int MasterSkinCount = 19;
    private const string SkinTextureDir = "res://Fgo/images/char/";

    private static readonly string[] ExtraSkinTextures =
    [
        "Monte_Cristo",
        "Cat_Arcueid_Brunestud",
        "Romani_Archaman",
        "Guda"
    ];

    public static readonly IReadOnlyList<string> SkinNames =
    [
        "迦勒底（默认）",
        "万圣节王室成员",
        "奏章2校服",
        "三咲高中校服",
        "新春装束",
        "白色圣诞",
        "夏日街头",
        "总耶高中校服",
        "第五真实元素环境用迦勒底制服",
        "华美的新年",
        "热带夏日",
        "2004年的碎片",
        "月之背面的记忆",
        "月之海的记忆",
        "明亮夏日",
        "王室品牌",
        "金色庆典",
        "迦勒底探险者",
        "冬日便装",
        "岩窟王　基督山",
        "猫姬",
        "罗曼",
        "人类恶"
    ];

    public static Texture2D? LoadSkinTexture(int index)
    {
        index = Math.Clamp(index, 0, SkinCount - 1);
        string name = index < MasterSkinCount
            ? $"Master{index}"
            : ExtraSkinTextures[index - MasterSkinCount];
        return GD.Load<Texture2D>($"{SkinTextureDir}{name}.png");
    }

    public static void ApplySkinToCreature(NCreatureVisuals? visuals, int index)
    {
        if (visuals == null) return;
        var sprite = visuals.GetNodeOrNull<Sprite2D>("%Visuals");
        if (sprite != null)
            sprite.Texture = LoadSkinTexture(index);
    }

    public static void ApplySkinToSelectPreview(NCharacterSelectScreen screen, int index)
    {
        var creature = FindCreatureVisuals(screen);
        ApplySkinToCreature(creature, index);
    }

    private static NCreatureVisuals? FindCreatureVisuals(Node root)
    {
        foreach (var child in root.GetChildren())
        {
            if (child is NCreatureVisuals cv) return cv;
            var nested = FindCreatureVisuals(child);
            if (nested != null) return nested;
        }

        return null;
    }
}
