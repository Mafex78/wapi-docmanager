namespace Shared.Infrastructure;

public class MongoOptions
{
    public const string SectionName = "Mongo";
    public string? ConnectionString { get; set; }
    public string? Database { get; set; }
}