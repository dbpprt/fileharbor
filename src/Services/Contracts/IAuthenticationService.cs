using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Fileharbor.Common.Database;
using Fileharbor.Services.Entities;

namespace Fileharbor.Services.Contracts
{
    public interface IAuthenticationService
    {
        Task<string> AcquireTokenAsync(string mailAddress, string password, Transaction transaction);
        Task<(Guid?, bool)> RegisterAsync(UserEntity entity, string password, Transaction transaction);
        Task<UserEntity> GetUserByMailAsync(string mailAddress, Transaction transaction);
    }
}
