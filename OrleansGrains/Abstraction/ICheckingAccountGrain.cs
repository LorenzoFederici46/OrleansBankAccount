namespace OrleansGrains.Abstraction
{
    public interface ICheckingAccountGrain : IGrainWithGuidKey
    {
        Task Initialize(decimal openingBalance);
        Task<decimal> GetBalance();
        Task Debit(decimal amount);
        Task Credit(decimal amount);
    }
}
