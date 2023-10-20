using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Model
{
    public abstract class Mode
    {
        public abstract Player[] InitPlayers();
        // public Player GetNextPlayer(Player current)
        // {
        //     var index = System.Array.IndexOf(playersOrderByAction, current);
        //     return playersOrderByAction[(index + 1) % playersOrderByAction.Length];
        // }

        public abstract Task WhenPlayerDie(Player player, Player src);

        public Player[] GetActionQueue()
        {
            var players = SgsMain.Instance.players;
            return new Player[]
            {
                players[1],
                players[2],
                players[3],
                players[0],
            };
        }

        // public virtual List<Player> GetRenderList() => SgsMain.Instance.players.ToList();
        public bool positionIsVisible { get; private set; }

        public static Dictionary<string, Mode> modeMap = new Dictionary<string, Mode>
        {
            { "2v2", new TwoVSTwo() },
            { "3v3", new ThreeVSThree() },
        };

        public static Mode Instance => Room.Instance.mode;
    }

    public class TwoVSTwo : Mode
    {
        // public override List<Player> GetRenderList()
        // {
        //     var list = base.GetRenderList();
        //     list.Add(list[0]);
        //     list.RemoveAt(0);
        //     return list;
        // }

        public override Player[] InitPlayers() => new Player[]
        {
            new Player(Team.BLUE, 3),
            new Player(Team.BLUE, 0),
            new Player(Team.RED, 1),
            new Player(Team.RED, 2),
        };


        public async override Task WhenPlayerDie(Player player, Player src)
        {
            if (player.teammates.Count == 0) throw new GameOverException(player.team);
            await new GetCardFromPile(player.teammates[0], 1).Execute();
        }
    }


    public class ThreeVSThree : Mode
    {
        public override Player[] InitPlayers() => new Player[]
        {
            new Player(Team.BLUE, 0),
            new Player(Team.BLUE, 4, true),
            new Player(Team.BLUE, 2),
            new Player(Team.RED, 1),
            new Player(Team.RED, 5, true),
            new Player(Team.RED, 3),
        };

        public async override Task WhenPlayerDie(Player player, Player src)
        {
            if (player.isMaster) throw new GameOverException(player.team);
            if (src.team != player.team) await new GetCardFromPile(src, 2).Execute();
        }
    }
}