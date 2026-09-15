using System.Reflection;
using System.Runtime.CompilerServices;
using Fgo.Scripts.Character;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using MegaCrit.Sts2.Core.Runs;

namespace Fgo.Scripts;

/// <summary>
///     世界场景（火堆 / 商店）角色形象的皮肤套用。
///     火堆房间在多人下会为<b>每位玩家</b>各创建一个 <c>NRestSiteCharacter</c>，
///     商店房间同理为每位玩家各创建一个 <c>NMerchantCharacter</c>，
///     而传入的 <c>CharacterModel</c> 是同角色共享的单例，无法区分玩家；
///     因此必须按节点自身的拥有者解析皮肤，否则所有玩家的形象都会套成同一份（本机）皮肤。
/// </summary>
internal static class FgoWorldSkin
{
    // 商店形象根节点下皮肤精灵的节点名（guda_merchant.tscn）
    public const string MerchantSpritePath = "Icon";

    // 火堆形象根节点下皮肤精灵的节点路径（guda_rest_site.tscn）
    public const string RestSiteSpritePath = "ControlRoot/Sprite";

    // 节点弱引用 -> 皮肤绑定，供收到远端皮肤消息后即时刷新
    private static readonly ConditionalWeakTable<Node, SkinBinding> Bindings = new();

    private static FieldInfo? _merchantPlayersField;

    /// <summary>
    ///     把皮肤贴到给定形象根下的精灵节点。
    /// </summary>
    /// <param name="root">形象根节点（火堆为 NRestSiteCharacter，商店为 NMerchantCharacter）。</param>
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

    /// <summary>
    ///     商店房间加载完成后，为房内<b>每个</b>形象按各自拥有者套皮肤。
    ///     这是商店皮肤的可靠入口：<c>AfterRoomIsLoaded</c> 里 <c>_players</c> 与 <c>PlayerVisuals</c>
    ///     按下标一一对应且都已填充完毕，不依赖任何"惰性/延迟"的调用时机。
    /// </summary>
    public static void ApplyMerchantRoom(NMerchantRoom room)
    {
        var players = MerchantPlayers(room);
        if (players == null) return;

        var visuals = room.PlayerVisuals;
        var count = Math.Min(visuals.Count, players.Count);
        for (var i = 0; i < count; i++)
        {
            var player = players[i];
            if (player?.Character is not FgoCharacter) continue;

            Apply(visuals[i], MerchantSpritePath, player.NetId);
        }
    }

    /// <summary>
    ///     解析世界形象节点所属的玩家。
    ///     火堆形象直接带 <c>NRestSiteCharacter.Player</c>；商店形象（<c>NMerchantCharacter</c>）不带拥有者，
    ///     需要按它在 <c>NMerchantRoom.PlayerVisuals</c> 中的下标，去房间的私有 <c>_players</c> 里反查——
    ///     两者是按同一个下标一一对应创建的（见 NMerchantRoom.AfterRoomIsLoaded）。
    ///     解析不到时返回 null，由调用方决定是跳过还是按本机兜底。
    /// </summary>
    public static Player? ResolveOwner(Node root)
    {
        switch (root)
        {
            case NRestSiteCharacter restSite:
                return restSite.Player;
            case NMerchantCharacter merchant:
                return ResolveMerchantOwner(merchant);
            default:
                return null;
        }
    }

    public static ulong? ResolveOwnerNetId(Node root)
    {
        return ResolveOwner(root)?.NetId;
    }

    /// <summary>
    ///     商店形象的拥有者：沿父链找到 <see cref="NMerchantRoom" />，
    ///     再用 <c>PlayerVisuals</c> 的下标去私有字段 <c>_players</c> 取对应玩家。
    /// </summary>
    private static Player? ResolveMerchantOwner(NMerchantCharacter merchant)
    {
        var room = FindAncestor<NMerchantRoom>(merchant);
        var players = room == null ? null : MerchantPlayers(room);
        if (players == null) return null;

        var visuals = room!.PlayerVisuals;
        var count = Math.Min(visuals.Count, players.Count);
        for (var i = 0; i < count; i++)
            if (ReferenceEquals(visuals[i], merchant))
                return players[i];

        return null;
    }

    private static List<Player>? MerchantPlayers(NMerchantRoom room)
    {
        _merchantPlayersField ??= AccessTools.Field(typeof(NMerchantRoom), "_players");
        return _merchantPlayersField?.GetValue(room) as List<Player>;
    }

    private static T? FindAncestor<T>(Node node) where T : Node
    {
        for (var current = node.GetParent(); current != null; current = current.GetParent())
            if (current is T match)
                return match;

        return null;
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

    private sealed class SkinBinding
    {
        public ulong NetId;
        public string SpritePath = string.Empty;
    }
}
