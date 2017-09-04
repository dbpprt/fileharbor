using System.Data;

namespace Fileharbor.Common.Database
{
    public static class TransactionExtensions
    {
        public static Transaction Spawn(this Transaction transaction, IDbConnection database)
        {
            return transaction == null
                ? new Transaction(database.BeginTransaction(), true)
                : new Transaction(transaction.InnerTransaction, false);
        }
    }
}