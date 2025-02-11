namespace OrleansGrains.Abstraction
{
    public interface IAtmGrain: IGrainWithGuidKey
    {
        public Task Initialize(decimal openingBalance);
        public Task Withdraw(Guid checkingAccountID, decimal amount);
    }
}
