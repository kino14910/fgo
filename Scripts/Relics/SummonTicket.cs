using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interactions.RightClick;

namespace Fgo.Scripts.Relics;

/// <summary>
///     呼符: 圣晶石被点金石精炼后的升级版。每层 +1 计数，≥3 时右键消耗 3 计数，
///     在全部尚未拥有的宝具里手动挑选 1 张加入 NobleDeck。计数与圣晶石共享按玩家的槽位。
/// </summary>
public class SummonTicket : FgoRelic, IModRightClickableRelic
{
    public const int CostPerChoice = 3;

    public override RelicRarity Rarity => RelicRarity.Ancient;
    public override bool ShowCounter => true;
    public override int DisplayAmount => QuartzCounter;

    public bool CanHandleRightClickLocal(ModRightClickContext context)
    {
        if (!FgoQuartzSummon.CanOpenSelection()) return false;
        if (FgoConfigSync.IsNetworkedRun() && !LocalContext.IsMe(Owner)) return false;
        return QuartzCounter >= CostPerChoice;
    }

    public async Task OnRightClick(ModRightClickExecutionContext context)
    {
        if (context.PlayerChoiceContext is not { } choiceContext) return;

        var player = context.Player;
        var candidates = FgoQuartzSummon.BuildCandidates(player);
        if (candidates.Count == 0)
        {
            Flash();
            return;
        }

        var prefs = new CardSelectorPrefs(SelectionScreenPrompt, 1);
        var selected = (await CardSelectCmd.FromSimpleGrid(choiceContext, candidates, player, prefs)).FirstOrDefault();
        if (selected == null)
        {
            Flash();
            return;
        }

        if (!await FgoQuartzSummon.Settle(player, selected))
        {
            Flash();
            return;
        }

        Flash();
    }

    public override Task AfterRoomEntered(AbstractRoom room)
    {
        if (FgoQuartzSync.ShouldSkipRoomEntry()) return Task.CompletedTask;
        IncrementQuartzOnRoomEntry(CostPerChoice);
        return Task.CompletedTask;
    }
}