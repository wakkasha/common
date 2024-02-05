namespace Common.Books.Configs;

public class AppConfig : KvConfig
{
    public string AppName { get; set; }
    public string Env { get; set; }
}