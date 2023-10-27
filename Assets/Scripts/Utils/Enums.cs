using System.Collections.Generic;
using System.Linq;

public class Team
{
    public static Team BLUE = new Team(false);
    public static Team RED = new Team(true);

    public bool value { get; private set; }
    private Team(bool value) => this.value = value;

    // public override bool Equals(object obj) => obj is Team team && value == team.value || obj is bool b && value == b;
    // public override int GetHashCode() => value.GetHashCode();
    public static Team operator !(Team team) => team == BLUE ? RED : BLUE;
    // public static bool operator ==(Team team, bool b) => team.value == b;
    // public static bool operator !=(Team team, bool b) => team.value != b;
    // public static bool operator ==(Team team1, Team tram2) => team1.value == tram2.value;
    // public static bool operator !=(Team team1, Team tram2) => team1.value != tram2.value;
    // public static Team GetByValue(bool value) => value ? RED : BLUE;

    public IEnumerable<Model.Player> GetAllPlayers() => Model.SgsMain.Instance.AlivePlayers.Where(x => this == x.team);
}



// public enum Mode
// {
//     欢乐成双,
//     统帅双军,
//     四对四
// }