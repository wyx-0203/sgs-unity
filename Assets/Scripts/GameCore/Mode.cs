using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Team = Model.Team;
using Mode = Model.Mode;

namespace GameCore
{
    public static class ModeExtensions
    {
        public static Player[] InitPlayers(this Mode mode, Game game)
        {
            switch (mode)
            {
                case Mode._2V2:
                    return new Player[]
                    {
                        new Player(Team.BLUE, 3),
                        new Player(Team.BLUE, 0),
                        new Player(Team.RED, 1),
                        new Player(Team.RED, 2),
                    };
                case Mode._3V3:
                    return new Player[]
                    {
                        new Player(Team.BLUE, 0),
                        new Player(Team.BLUE, 4, true),
                        new Player(Team.BLUE, 2),
                        new Player(Team.RED, 1),
                        new Player(Team.RED, 5, true),
                        new Player(Team.RED, 3)
                    };
                default: return null;
            }
        }

        public static async Task OnPlayerDie(this Mode mode, Player player, Player src)
        {
            switch (mode)
            {
                case Mode._2V2:
                    if (player.teammates.Count == 0) throw new GameOverException(player.team);
                    await new DrawCard(player.teammates[0], 1).Execute();
                    break;

                case Mode._3V3:
                    if (player.isMonarch) throw new GameOverException(player.team);
                    if (src?.team != player.team) await new DrawCard(src, 2).Execute();
                    break;
            }
        }
        //     {
        //         if (player.teammates.Count == 0) throw new GameOverException(player.team);
        //         await new DrawCard(player.teammates[0], 1).Execute();
        //     }
    }
    // public abstract class Mode
    // {
    //     public abstract Player[] InitPlayers();

    //     public abstract Task OnPlayerDie(Player player, Player src);

    //     // public Player[] GetActionQueue()
    //     // {
    //     //     var players = game.players;
    //     //     return new Player[]
    //     //     {
    //     //         players[1],
    //     //         players[2],
    //     //         players[3],
    //     //         players[0],
    //     //     };
    //     // }

    //     // public bool positionIsVisible { get; private set; }
    //     // public int playersNumber { get; protected set; }
    //     public bool isSingle{get;set;}

    //     public static Dictionary<string, Mode> modeMap = new Dictionary<string, Mode>
    //     {
    //         { "2v2", new TwoVSTwo() },
    //         { "3v3", new ThreeVSThree() },
    //     };

    //     // public static Mode Instance => Room.Instance.mode;
    // }

    // public class TwoVSTwo : Mode
    // {
    //     // public TwoVSTwo()
    //     // {
    //     //     playersNumber = 4;
    //     // }

    //     public override Player[] InitPlayers() => new Player[]
    //     {
    //         new Player(Team.BLUE, 3),
    //         new Player(Team.BLUE, 0),
    //         new Player(Team.RED, 1),
    //         new Player(Team.RED, 2),
    //     };

    //     public async override Task OnPlayerDie(Player player, Player src)
    //     {
    //         if (player.teammates.Count == 0) throw new GameOverException(player.team);
    //         await new DrawCard(player.teammates[0], 1).Execute();
    //     }
    // }


    // public class ThreeVSThree : Mode
    // {
    //     // public ThreeVSThree()
    //     // {
    //     //     playersNumber = 6;
    //     // }

    //     public override Player[] InitPlayers() => new Player[]
    //     {
    //         new Player(Team.BLUE, 0),
    //         new Player(Team.BLUE, 4, true),
    //         new Player(Team.BLUE, 2),
    //         new Player(Team.RED, 1),
    //         new Player(Team.RED, 5, true),
    //         new Player(Team.RED, 3),
    //     };

    //     public async override Task OnPlayerDie(Player player, Player src)
    //     {
    //         if (player.isMonarch) throw new GameOverException(player.team);
    //         if (src != null && src.team != player.team) await new DrawCard(src, 2).Execute();
    //     }
    // }
}