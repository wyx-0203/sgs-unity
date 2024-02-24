using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameCore;
using Phase = Model.Phase;

public class 克己 : Triggered
{
    protected override bool OnPhaseStart(Phase phase) => phase == Phase.Discard && !useSha;

    protected override bool OnUseCard(Card card)
    {
        if (card is 杀 && game.turnSystem.CurrentPlayer == src && game.turnSystem.CurrentPhase == Phase.Play)
        {
            useSha = true;
            game.turnSystem.AfterTurn += () => useSha = false;
        }
        return false;
    }

    private bool useSha;

    protected override Task Invoke(PlayDecision decision)
    {
        throw new SkipPhaseException();
    }
}

// public class 博图 : Triggered
// {
//     protected override bool OnPhaseStart(Phase phase) => phase == Phase.End
//         && cards.Select(x => x.suit).Distinct().Count() == 4;

//     protected override bool OnUseCard(Card card)
//     {
//         if (game.turnSystem.CurrentPlayer == src)
//         {
//             if (cards.Count == 0) game.turnSystem.AfterTurn += () => cards.Clear();
//             if (!card.isConvert) cards.Add(card);
//             else cards.AddRange(card.PrimiTives);
//         }
//         return false;
//     }
//     private List<Card> cards = new();

//     protected override Task Invoke(PlayDecision decision)
//     {
//         throw new System.NotImplementedException();
//     }
// }

// public class 勤学 : Awoken
// {
//     protected override bool OnPhaseStart(Phase phase) => (phase == Phase.Prepare || phase == Phase.End) 
//         && src.handCardsCount >= src.hp + 2;

//     protected override Task Invoke(PlayDecision decision)
//     {
//         throw new System.NotImplementedException();
//         // hplimit - 1
//         // get skill
//     }
// }