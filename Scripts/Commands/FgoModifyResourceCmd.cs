using Fgo.Scripts.Commands;
using MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;

public sealed class FgoModifyNpCmd : AbstractConsoleCmd
{
    public override string CmdName => "np";

    public override string Args => "<amount:int>";

    public override string Description => "调整当前玩家的 NP（可正可负）";

    public override bool IsNetworked => true;

    public override CmdResult Process(Player? issuingPlayer, string[] args)
    {
        if (args.Length < 1 || !int.TryParse(args[0], out var amount))
            return new CmdResult(false, "需要一个整数参数，例如：np 50");

        if (issuingPlayer == null || !RunManager.Instance.IsInProgress)
            return new CmdResult(false, "当前没有进行中的游戏");

        return new CmdResult(FgoResCmd.ModifyNp(amount, issuingPlayer), true,
            $"已调整 NP：{amount:+#;-#;0}");
    }
}

public sealed class FgoModifyStarCmd : AbstractConsoleCmd
{
    public override string CmdName => "star";

    public override string Args => "<amount:int>";

    public override string Description => "调整当前玩家的暴击星（可正可负）";

    public override bool IsNetworked => true;

    public override CmdResult Process(Player? issuingPlayer, string[] args)
    {
        if (args.Length < 1 || !int.TryParse(args[0], out var amount))
            return new CmdResult(false, "需要一个整数参数，例如：star 20");

        if (issuingPlayer == null || !RunManager.Instance.IsInProgress)
            return new CmdResult(false, "当前没有进行中的游戏");

        return new CmdResult(FgoResCmd.ModifyStars(amount, issuingPlayer), true,
            $"已调整暴击星：{amount:+#;-#;0}");
    }
}
