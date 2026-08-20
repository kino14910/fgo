using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace Fgo.Scripts.Keywords;

/// <summary>
///     FGO mod 自定义卡牌关键词注册。
/// </summary>
[RegisterOwnedCardKeyword(
    nameof(IgnoreInvincible),
    CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]
public sealed class FgoKeywords
{
    /// <summary>
    ///     无敌贯通：攻击去除敌人格挡，并移除 [gold]硬化外壳[/gold] 和 [gold]难以杀灭[/gold]。
    /// </summary>
    public static readonly CardKeyword IgnoreInvincible =
        ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(IgnoreInvincible)).GetModCardKeyword();
}