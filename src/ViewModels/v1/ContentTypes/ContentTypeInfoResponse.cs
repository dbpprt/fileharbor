using System;

namespace Fileharbor.ViewModels.v1.ContentTypes
{
    public class ContentTypeInfoResponse
    {
        public Guid Id { get; set; }

        public Guid? ParentId { get; set; }

        public string Name { get; set; }

        public string GroupName { get; set; }
    }
}