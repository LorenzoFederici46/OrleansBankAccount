namespace OrleansGrains.Abstraction
{
    public interface ICheckingAccountGrain : IGrainWithGuidKey
    {
        [Transaction(TransactionOption.Create)]
        Task Initialize(decimal openingBalance);

        [Transaction(TransactionOption.Create)]
        Task<decimal> GetBalance();

        [Transaction(TransactionOption.CreateOrJoin)]
        Task Debit(decimal amount);

        [Transaction(TransactionOption.CreateOrJoin)]
        Task Credit(decimal amount);

        [Transaction(TransactionOption.CreateOrJoin)]
        Task Transfer(Guid checkingAccountID, decimal amount);

    }
}
