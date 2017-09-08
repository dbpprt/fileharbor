using System;

namespace Fileharbor.Common
{
    public class CurrentPrincipal
    {
        public CurrentPrincipal()
        {
            Id = Guid.Empty;
            IsAuthenticated = false;
        }

        public Guid Id { get; set; }
       
        public bool IsAuthenticated { get; set; }
    }
}
