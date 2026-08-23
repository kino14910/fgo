using Fgo.Scripts.Cards;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Scaffolding.Content;

namespace Fgo.Scripts.Powers;

/// <summary>
///     巨型规模（HugeScalePower）：每个回合开始时，
///     获得 Amount 点额外最大生命值，并将一张幼儿退化加入手牌。
/// </summary>
public class HugeScalePower : FgoPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new("res://Fgo/images/powers/EveryTurnPower.png",
        "res://Fgo/images/powers/EveryTurnPower.png");

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player) return;
        Flash();

        // 每回合开始: 获得额外最大生命值。
        await PowerCmd.Apply<MaxHpPower>(choiceContext, Owner, Amount, Owner, null);
    }
}