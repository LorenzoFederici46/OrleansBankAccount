using OrleansGrains.Abstraction;
using OrleansGrains.States;
namespace OrleansGrains.Grains
{
    public class CheckingAccountGrain : Grain, ICheckingAccountGrain
    {
        private readonly IPersistentState<BalanceState> _balanceState;
        private readonly IPersistentState<CheckingAccountState> _checkingAccountState;
        public CheckingAccountGrain(
            [PersistentState("balance", "tableStorage")] IPersistentState<BalanceState> balanceState,
            [PersistentState("checkingAccount", "blobStorage")] IPersistentState<CheckingAccountState> checkingAccountState)
        {
            _balanceState = balanceState;
            _checkingAccountState = checkingAccountState;
        }

        public async Task Credit(decimal amount)
        {
            var currentBalance = _balanceState.State.Balance;
            var newBalance = currentBalance + amount;

            _balanceState.State.Balance = newBalance;
            await _balanceState.WriteStateAsync();
        }

        public async Task Debit(decimal amount)
        {
            var currentBalance = _balanceState.State.Balance;
            var newBalance = currentBalance - amount;

            _balanceState.State.Balance = newBalance;
            await _balanceState.WriteStateAsync();
        }
        public async Task Transfer(Guid checkingAccountID, decimal amount)
        {
            var checkingAccountGrain = GrainFactory.GetGrain<ICheckingAccountGrain>(checkingAccountID);
            var currentBalance = _balanceState.State.Balance;
            var newBalance = currentBalance - amount;

            _balanceState.State.Balance = newBalance;
            await checkingAccountGrain.Credit(amount);

            await _balanceState.WriteStateAsync();

        }

        public async Task<decimal> GetBalance()
        {
            return _balanceState.State.Balance;
        }

        public async Task Initialize(decimal openingBalance)
        {
            _checkingAccountState.State.OpenedAtUtc = DateTime.UtcNow;
            _checkingAccountState.State.AccountType = "Default";
            _checkingAccountState.State.AccountID = this.GetGrainId().GetGuidKey();

           _balanceState.State.Balance = openingBalance;
           await _balanceState.WriteStateAsync();
           await _checkingAccountState.WriteStateAsync();
        }
    }
}
