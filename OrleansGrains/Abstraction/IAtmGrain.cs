namespace OrleansGrains.Abstraction
{
    public interface IAtmGrain: IGrainWithGuidKey
    {
        public Task initialize(decimal openingBalance);
        public Task Withdraw(Guid checkingAccountID, decimal amount);
    }
}
