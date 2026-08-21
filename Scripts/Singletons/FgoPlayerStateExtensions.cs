using MegaCrit.Sts2.Core.Models;

namespace Fgo.Scripts.Singletons;

public static class FgoPlayerStateExtensions
{
    /// <summary>
    ///     获取模型所属 FGO 玩家的战斗资源状态。
    ///     卡牌 / 遗物取其 Owner（Player），Power 取其 Owner 的生物上的 Player。
    ///     无玩家上下文（如卡牌图鉴预览）时返回一个临时零状态用于显示，避免空引用。
    /// </summary>
    public static FgoPlayerState FgoRes<T>(this T model) where T : AbstractModel
    {
        var player = model switch
        {
            CardModel card => card.Owner,
            PowerModel power => power.Owner?.Player,
            RelicModel relic => relic.Owner,
            _ => null
        };

        return player != null ? FgoBattleHooks.Get(player) : new FgoPlayerState();
    }
}