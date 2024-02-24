using System.Collections.Generic;

public class Config : SingletonMono<Config>
{
    public List<string> DebugCards = new();
    public List<string> DebugGenerals = new();
    public bool selfAI;
    public bool EnableMCTS;
}
