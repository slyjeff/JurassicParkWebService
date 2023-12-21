namespace JurassicParkWebService;

public interface IDatabaseConfiguration {
    public string ConnectionString { get; }
}

public class DatabaseConfiguration : IDatabaseConfiguration {
    public string ConnectionString { get; set; } = string.Empty;
}