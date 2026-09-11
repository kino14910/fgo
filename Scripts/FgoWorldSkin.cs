using System.Runtime.CompilerServices;
using Godot;
using MegaCrit.Sts2.Core.Runs;

namespace Fgo.Scripts;

/// <summary>
///     世界场景（火堆 / 商店）角色形象的皮肤套用。
///     火堆房间在多人下会为<b>每位玩家</b>各创建一个 <c>NRestSiteCharacter</c>，
///     而传入的 <c>CharacterModel</c> 是同角色共享的单例，无法区分玩家；
///     因此必须按节点自身的拥有者（<c>NRestSiteCharacter.Player.NetId</c>）解析皮肤，
///     否则所有玩家的形象都会套成同一份（本机）皮肤。
/// </summary>
internal static class FgoWorldSkin
{
    // 节点弱引用 -> 皮肤绑定，供收到远端皮肤消息后即时刷新
    private static readonly ConditionalWeakTable<Node, SkinBinding> Bindings = new();

    private sealed class SkinBinding
    {
        public string SpritePath = string.Empty;
        public ulong NetId;
    }

    /// <summary>
    ///     把皮肤贴到给定形象根下的精灵节点。
    /// </summary>
    /// <param name="root">形象根节点（火堆为 NRestSiteCharacter）。</param>
    /// <param name="spritePath">精灵节点路径，找不到则递归取第一个 Sprite2D。</param>
    /// <param name="ownerNetId">拥有者玩家的 NetId；为 null 时按本机玩家处理。</param>
    public static void Apply(Node? root, string spritePath, ulong? ownerNetId)
    {
        if (root == null) return;

        var sprite = FindSprite(root, spritePath);
        if (sprite == null) return;

        var netId = ownerNetId ?? RunManager.Instance?.NetService?.NetId ?? 0;
        sprite.Texture = FgoSkinApplier.LoadSkinTexture(FgoSkinSync.ResolveSkin(netId));

        Bindings.Remove(root);
        Bindings.Add(root, new SkinBinding { NetId = netId, SpritePath = spritePath });
    }

    /// <summary>
    ///     某位玩家的皮肤更新后，刷新仍在场景树中的世界形象（覆盖"形象先于皮肤消息创建"的时序）。
    /// </summary>
    public static void Refresh(ulong netId, int skin)
    {
        foreach (var pair in Bindings)
        {
            var binding = pair.Value;
            if (binding.NetId != netId) continue;

            var node = pair.Key;
            if (!GodotObject.IsInstanceValid(node)) continue;

            var sprite = FindSprite(node, binding.SpritePath);
            if (sprite != null)
                sprite.Texture = FgoSkinApplier.LoadSkinTexture(skin);
        }
    }

    private static Sprite2D? FindSprite(Node root, string spritePath)
    {
        return root.GetNodeOrNull<Sprite2D>(spritePath) ?? FindFirstSprite(root);
    }

    private static Sprite2D? FindFirstSprite(Node root)
    {
        foreach (var child in root.GetChildren())
        {
            if (child is Sprite2D found) return found;
            var nested = FindFirstSprite(child);
            if (nested != null) return nested;
        }

        return null;
    }
}
