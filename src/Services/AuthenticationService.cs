using System;
using System.Data;
using System.Data.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Dapper;
using Fileharbor.Common;
using Fileharbor.Common.Configuration;
using Fileharbor.Common.Database;
using Fileharbor.Common.Utilities;
using Fileharbor.Services.Contracts;
using Fileharbor.Services.Entities;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Fileharbor.Services
{
    [UsedImplicitly]
    public class AuthenticationService : ServiceBase, IAuthenticationService
    {
        private readonly IOptions<AuthenticationConfiguration> _authenticationConfiguration;
        private readonly ILogger<AuthenticationService> _logger;
        private readonly SigningCredentials _signingCredentials;

        public AuthenticationService(IOptions<AuthenticationConfiguration> authenticationConfiguration, ILogger<AuthenticationService> logger, IDbConnection database, SigningCredentials signingCredentials)
            : base(database)
        {
            _authenticationConfiguration = authenticationConfiguration;
            _logger = logger;
            _signingCredentials = signingCredentials;
        }

        private string NormalizeMailAddress(string mailAddress)
        {
            return mailAddress.ToLowerInvariant();
        }
        
        public async Task<(Guid?, bool)> RegisterAsync(UserEntity entity, string password, Transaction transaction)
        {
            var database = await GetDatabaseConnectionAsync();
            transaction = transaction.Spawn(database);
            
            try
            {
                var existingUser = await TryGetUserByMailAsync(entity.MailAddress, transaction);

                if (existingUser != null)
                {
                    _logger.LogWarning(LoggingEvents.InsertItemAlreadyExists, "Unable to create new user, mail address is already taken {0}", entity.MailAddress);
                    throw new ArgumentOutOfRangeException(nameof(entity.MailAddress), "E-Mail address is already taken!");
                }

                entity.PasswordHash =
                    PasswordHasher.HashPassword(password, _authenticationConfiguration.Value.HashIterations);
                entity.Validated = _authenticationConfiguration.Value.EnableMailValidation != true;
                entity.Id = Guid.NewGuid();

                _logger.LogDebug(LoggingEvents.InsertItem, "Trying to create new user {0} with id {1}", entity.MailAddress, entity.Id);

                await database.ExecuteAsync(
                    $"insert into users (id, email, givenname, surname, superadmin, password_hash, validated) " +
                    $"values (@Id, @MailAddress, @GivenName, @SurName, false, @PasswordHash, @Validated)",
                    new { entity.Id, entity.MailAddress, entity.GivenName, entity.SurName, entity.PasswordHash, entity.Validated },
                    (DbTransaction)transaction);
                await transaction.CommitAsync();

                return (entity.Id, entity.Validated);
            }
            catch (Exception e)
            {
                _logger.LogWarning(LoggingEvents.InsertItem, e, "Unable to create new user - unexpected exception");
                await transaction.RollbackAsync();
                return (null, true);
            }
        }

        public async Task<UserEntity> TryGetUserByMailAsync(string mailAddress, Transaction transaction)
        {
            try
            {
                return await GetUserByMailAsync(mailAddress, transaction);
            }
            catch (ArgumentOutOfRangeException)
            {
                return null;
            }
        }

        public async Task<UserEntity> GetUserByMailAsync(string mailAddress, Transaction transaction)
        {
            var database = await GetDatabaseConnectionAsync();

            var normalizedMailAddress = NormalizeMailAddress(mailAddress);
            _logger.LogDebug("Trying to lookup user with mail {0} and normalized mail {1}", mailAddress, normalizedMailAddress);

            var user = await database.QueryFirstOrDefaultAsync<UserEntity>("select * from users where email = @MailAddress",
                new { MailAddress = normalizedMailAddress }, (DbTransaction)transaction);

            if (user == null)
            {
                _logger.LogWarning(LoggingEvents.GetItemNotFound, "Unable to lookup user {0}", normalizedMailAddress);
                throw new ArgumentOutOfRangeException(nameof(mailAddress));
            }

            _logger.LogDebug(LoggingEvents.GetItem, "Found user with mail {0}", mailAddress);
            return user;
        }

        public async Task<string> AcquireTokenAsync(string mailAddress, string password, Transaction transaction)
        {
            _logger.LogDebug("Trying to login user {0}", mailAddress);

            var user = await GetUserByMailAsync(mailAddress, null);
            if (!PasswordHasher.VerifyHashedPassword(user.PasswordHash, password,
                _authenticationConfiguration.Value.HashIterations))
            {
                _logger.LogWarning(LoggingEvents.AuthFailure, "User tried to login with invalid credentials! Mail: {0} Password: {1}");
                throw new ArgumentOutOfRangeException(nameof(password), "Invalid password.");
            }

            var now = DateTime.UtcNow;

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Iss, _authenticationConfiguration.Value.Issuer),
                new Claim(JwtRegisteredClaimNames.Sub, mailAddress),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUniversalTime().ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            // ReSharper disable ArgumentsStyleNamedExpression
            var jwt = new JwtSecurityToken(
                issuer: _authenticationConfiguration.Value.Issuer,
                audience: _authenticationConfiguration.Value.Audience,
                claims: claims,
                notBefore: now,
                expires: now.AddHours(_authenticationConfiguration.Value.TokenLifeTimeInHours),
                signingCredentials: _signingCredentials);
            // ReSharper restore ArgumentsStyleNamedExpression

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }
    }
}
