namespace RealEstate.Infrastructure.Configuration;

public class MongoSettings
{
    public string ConnectionString { get; set; } = default!;
    public string DatabaseName { get; set; } = default!;
    public int MinPoolSize { get; set; } = 5;
    public int MaxPoolSize { get; set; } = 100;
    public int SocketTimeoutMs { get; set; } = 15000;
    public int ServerSelectionTimeoutMs { get; set; } = 10000;
}
