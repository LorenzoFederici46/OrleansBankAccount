using Orleans.Concurrency;
using Orleans.Transactions.Abstractions;
using OrleansGrains.Abstraction;
using OrleansGrains.States;
namespace OrleansGrains.Grains
{
    [Reentrant]
    public class AtmGrain : Grain, IAtmGrain
    {
        private readonly ITransactionalState<AtmState> _atmTransactionalState;
        public AtmGrain(
           [TransactionalState("atm")] ITransactionalState<AtmState> atmTransactionalState)
        {
            _atmTransactionalState = atmTransactionalState;
        }

        public async Task Initialize(decimal openingBalance)
        {
            await _atmTransactionalState.PerformUpdate(state =>
            {
                state.Balance = openingBalance;
                state.ID= this.GetGrainId().GetGuidKey();
            });
        }
        public async Task<decimal> GetBalance()
        {
            return await _atmTransactionalState.PerformRead((state) => state.Balance);
        }

        public async Task Withdraw(Guid checkingAccountID, decimal amount)
        {
            var checkingAccountGrain = GrainFactory.GetGrain<ICheckingAccountGrain>(checkingAccountID);
            await _atmTransactionalState.PerformUpdate(state =>
            {
                var currentAtmBalance = state.Balance;
                var updatedBalance = currentAtmBalance - amount;
                state.Balance = updatedBalance;
            });
        }
    }
}
