namespace Automotive.Marketplace.Application.Common.Exceptions;

public class DbEntityNotFoundException : Exception
{
    public string EntityType { get; } = string.Empty;

    public Guid EntityId { get; set; }

    public DbEntityNotFoundException()
        : base("Database entity was not found.")
    {
        EntityType = string.Empty;
        EntityId = Guid.Empty;
    }

    public DbEntityNotFoundException(string? message)
        : base(message)
    {
        EntityType = string.Empty;
        EntityId = Guid.Empty;
    }

    public DbEntityNotFoundException(string? message, Exception? innerException)
        : base(message, innerException)
    {
        EntityType = string.Empty;
        EntityId = Guid.Empty;
    }

    public DbEntityNotFoundException(string entityType, Guid entityId)
        : base($"{entityType} with ID '{entityId}' was not found.")
    {
        EntityType = entityType;
        EntityId = entityId;
    }

    public DbEntityNotFoundException(string entityType, Guid entityId, Exception? innerException)
        : base($"{entityType} with ID '{entityId}' was not found.", innerException)
    {
        EntityType = entityType;
        EntityId = entityId;
    }
}
