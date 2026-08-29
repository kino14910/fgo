using Fgo.Scripts.Cards.Colorless.OptionCards;
using Fgo.Scripts.Singletons;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Networking.ManagedActions;

namespace Fgo.Scripts.Commands;

public static class FgoCommandSpellCmd
{
    private const int HealAmount = 30;
    private const int NpAmount = 100;

    /// <summary>
    ///     托管网络动作: 令咒按钮的多人同步（详见 FgoNoblePhantasmCmd.SyncDescriptor 注释）。
    ///     必须在 Entry.Init 注册，保证任何 peer 发起前本端已注册。
    /// </summary>
    internal static readonly RitsuLibManagedNetActionDescriptor<byte> SyncDescriptor = new(
        Entry.ModId,
        "command_spell_button",
        static _ => [],
        static _ => 0,
        ExecuteManaged,
        GameActionType.CombatPlayPhaseOnly);

    private static async Task ExecuteManaged(RitsuLibManagedNetActionContext<byte> context)
    {
        await TryUseCommandSpell(context.PlayerChoiceContext, context.Player);
    }

    /// <summary>
    ///     UI 按钮入口: 仅本机玩家调用（调用方需保证玩家为本地 FGO 角色）。
    /// </summary>
    public static bool Request()
    {
        if (CombatManager.Instance.IsInProgress &&
            RunManager.Instance.ActionQueueSynchronizer.CombatState != ActionSynchronizerCombatState.PlayPhase)
            return false;

        return RitsuLibManagedNetActions.Request<byte>(RunManager.Instance, SyncDescriptor, 0);
    }

    public static async Task<bool> TryUseCommandSpell(PlayerChoiceContext choiceContext, Player player)
    {
        var playerState = FgoBattleHooks.Get(player);
        if (!playerState.CanUseCommandSpell) return false;

        var combatState = player.Creature.CombatState;
        if (combatState == null) return false;
        var cards = new List<CardModel>
        {
            combatState.CreateCard<RepairSpiritOrigin>(player),
            combatState.CreateCard<ReleaseNoblePhantasm>(player)
        };

        var selected = await CardSelectCmd.FromChooseACardScreen(choiceContext, cards, player);
        if (selected == null) return false;

        await playerState.UseCommandSpell();
        // 不在此处立即同步到 RunSavedData。
        // 使用后仅更新内存值，在战斗结束时（AfterCombatVictory）才同步到 RunSavedData。
        // 这样退出战斗中再继续会恢复到战前值，而打赢后下一场战斗保留战后值（SL后不消耗令咒数）。

        switch (selected)
        {
            case RepairSpiritOrigin:
                await CreatureCmd.Heal(player.Creature, HealAmount);
                break;
            case ReleaseNoblePhantasm:
                await playerState.ModifyNp(NpAmount, player);
                break;
        }

        return true;
    }
}