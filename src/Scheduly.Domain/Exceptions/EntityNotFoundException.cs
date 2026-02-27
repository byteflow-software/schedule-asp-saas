namespace Scheduly.Domain.Exceptions;

public class EntityNotFoundException : DomainException
{
    public EntityNotFoundException(string entityName, object key)
        : base("ENTITY_NOT_FOUND", $"Entity '{entityName}' with key '{key}' was not found.")
    {
    }
}
