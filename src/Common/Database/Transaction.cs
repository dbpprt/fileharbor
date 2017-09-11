using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Npgsql;

namespace Fileharbor.Common.Database
{
    public class Transaction
    {
        private bool ShouldCommit { get; }

        public DbTransaction InnerTransaction { get; }

        public Transaction(IDbTransaction innerTransaction, bool shouldCommit)
        {
            InnerTransaction = (DbTransaction)innerTransaction;
            ShouldCommit = shouldCommit;
        }

        public async Task CommitAsync()
        {
            if (ShouldCommit && InnerTransaction is NpgsqlTransaction postgresTransaction)
            {
                await postgresTransaction.CommitAsync();
            }
            else if (ShouldCommit)
            {
                InnerTransaction.Commit();
            }
        }

        public async Task RollbackAsync()
        {
            if (ShouldCommit && InnerTransaction is NpgsqlTransaction postgresTransaction)
            {
                await postgresTransaction.RollbackAsync();
            }
            else if (ShouldCommit)
            {
                InnerTransaction.Rollback();
            }
        }

        public static implicit operator DbTransaction(Transaction transaction)
        {
            return transaction?.InnerTransaction;
        }

        public async Task<T> ExecuteAsync<T>(Func<Task<T>> action)
        {
            try
            {
                var result = await action();
                await CommitAsync();
                return result;
            }
            catch (Exception)
            {
                await RollbackAsync();
                throw;
            }
        }
    }
}
