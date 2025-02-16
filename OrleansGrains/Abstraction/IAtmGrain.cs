namespace OrleansGrains.Abstraction
{
    public interface IAtmGrain: IGrainWithGuidKey
    {
        [Transaction(TransactionOption.Create)]
        public Task Initialize(decimal openingBalance);

        [Transaction(TransactionOption.CreateOrJoin)]
        public Task Withdraw(Guid checkingAccountID, decimal amount);

        [Transaction(TransactionOption.Create)]
        Task<decimal> GetBalance();

    }
}
