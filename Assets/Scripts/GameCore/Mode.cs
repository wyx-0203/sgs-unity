using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameCore
{
    public abstract class Mode
    {
        public abstract Player[] InitPlayers();

        public abstract Task OnPlayerDie(Player player, Player src);

        public Player[] GetActionQueue()
        {
            var players = Main.Instance.players;
            return new Player[]
            {
                players[1],
                players[2],
                players[3],
                players[0],
            };
        }

        // public bool positionIsVisible { get; private set; }

        public static Dictionary<string, Mode> modeMap = new Dictionary<string, Mode>
        {
            { "2v2", new TwoVSTwo() },
            { "3v3", new ThreeVSThree() },
        };

        public static Mode Instance => Room.Instance.mode;
    }

    public class TwoVSTwo : Mode
    {
        public override Player[] InitPlayers() => new Player[]
        {
            new Player(Team.BLUE, 3),
            new Player(Team.BLUE, 0),
            new Player(Team.RED, 1),
            new Player(Team.RED, 2),
        };

        public async override Task OnPlayerDie(Player player, Player src)
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

        public async override Task OnPlayerDie(Player player, Player src)
        {
            if (player.isMaster) throw new GameOverException(player.team);
            if (src != null && src.team != player.team) await new GetCardFromPile(src, 2).Execute();
        }
    }
}