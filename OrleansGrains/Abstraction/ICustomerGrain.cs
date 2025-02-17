namespace OrleansGrains.Abstraction
{
    public interface ICustomerGrain : IGrainWithGuidKey
    {
        Task AddCheckingAccount(Guid checkingAccountID);
        Task<decimal> GetNetWorth();
    }
}
