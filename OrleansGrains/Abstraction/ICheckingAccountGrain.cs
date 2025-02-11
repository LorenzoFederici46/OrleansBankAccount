namespace OrleansGrains.Abstraction
{
    public interface ICheckingAccountGrain : IGrainWithGuidKey
    {
        Task initialize(decimal openingBalance);
        Task<decimal> getBalance();
        Task Debit(decimal amount);
        Task Credit(decimal amount);
    }
}
