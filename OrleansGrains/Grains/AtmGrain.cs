using OrleansGrains.Abstraction;
using OrleansGrains.States;
namespace OrleansGrains.Grains
{
    public class AtmGrain : Grain, IAtmGrain
    {
        private readonly IPersistentState<AtmState> _atmState;
        public AtmGrain(
           [PersistentState("atm", "tableStorage")] IPersistentState<AtmState> atmState)
        {
            _atmState = atmState;
        }
        public async Task initialize(decimal openingBalance)
        {
            _atmState.State.Balance = openingBalance;
            _atmState.State.ID = this.GetGrainId().GetGuidKey();
        }

        public async Task Withdraw(Guid checkingAccountID, decimal amount)
        {
            var checkingAccountGrain = GrainFactory.GetGrain<ICheckingAccountGrain>(checkingAccountID);
            await checkingAccountGrain.Debit(amount);

            var currentBalance = _atmState.State.Balance;
            var updatedBalance = currentBalance - amount;
            _atmState.State.Balance = updatedBalance;

            await _atmState.WriteStateAsync();
        }
    }
}
