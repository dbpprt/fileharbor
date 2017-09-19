using System;
using Fileharbor.Services.Entities;

namespace Fileharbor.ViewModels.v1.Collections
{
    public class MyCollectionsResponse
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public PermissionLevel PermissionLevel { get; set; }
    }
}