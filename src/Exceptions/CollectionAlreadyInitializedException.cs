
using System;
using Fileharbor.Exceptions;

public class CollectionAlreadyInitializedException : FileharborException
{
    public Guid CollectionId { get; }

    public Guid TemplateId { get; }

    public CollectionAlreadyInitializedException(Guid collectionId, Guid templateId, Exception innerException = null) : base(500, "The collection is already initialized", innerException)
    {
        CollectionId = collectionId;
        TemplateId = templateId;
    }
}