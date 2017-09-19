using System;
using Fileharbor.Exceptions;

public class AccessDeniedException : FileharborException
{
    public AccessDeniedException(Guid collectionId, Exception innerException = null) : base(401, "Access denied",
        innerException)
    {
        CollectionId = collectionId;
    }

    public Guid CollectionId { get; }
}