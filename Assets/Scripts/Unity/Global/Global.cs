public class Global : GlobalSingleton<Global>
{
    public string token { get; set; }
    public int userId { get; set; }
    public int roomId { get; set; } = -1;

    public bool IsStandalone => userId == Model.User.StandaloneId;
}
