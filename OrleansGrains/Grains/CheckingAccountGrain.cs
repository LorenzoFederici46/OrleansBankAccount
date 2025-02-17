using Orleans.Concurrency;
using Orleans.Transactions.Abstractions;
using OrleansGrains.Abstraction;
using OrleansGrains.Event;
using OrleansGrains.States;
namespace OrleansGrains.Grains
{
    [Reentrant]
    public class CheckingAccountGrain : Grain, ICheckingAccountGrain
    {
        private readonly ITransactionalState<BalanceState> _balanceTransactionalState;
        private readonly IPersistentState<CheckingAccountState> _checkingAccountState;
        public CheckingAccountGrain(
            [TransactionalState("balance")] ITransactionalState<BalanceState> balanceTransactionalState,
            [PersistentState("checkingAccount", "blobStorage")] IPersistentState<CheckingAccountState> checkingAccountState)
        {
            _balanceTransactionalState = balanceTransactionalState;
            _checkingAccountState = checkingAccountState;
        }

        public async Task Credit(decimal amount)
        {
            await _balanceTransactionalState.PerformUpdate(state =>
            {
                var currentBalance = state.Balance;
                var newBalance = currentBalance + amount;
                state.Balance = newBalance;
            });

            var streamProvider = this.GetStreamProvider("StreamProvider");
            var streamID = StreamId.Create("BalanceStream", this.GetGrainId().GetGuidKey());
            var stream = streamProvider.GetStream<BalanceChangeEvent>(streamID);

            await stream.OnNextAsync(new BalanceChangeEvent()
            {
                CheckingAccountID = this.GetGrainId().GetGuidKey(),
                Balance = await GetBalance()
            });
        }

        public async Task Debit(decimal amount)
        {
            await _balanceTransactionalState.PerformUpdate(state =>
            {
                var currentBalance = state.Balance;
                var newBalance = currentBalance - amount;
                state.Balance = newBalance;
            });

            var streamProvider = this.GetStreamProvider("StreamProvider");
            var streamID = StreamId.Create("BalanceStream", this.GetGrainId().GetGuidKey());
            var stream = streamProvider.GetStream<BalanceChangeEvent>(streamID);

            await stream.OnNextAsync(new BalanceChangeEvent()
            {
                CheckingAccountID = this.GetGrainId().GetGuidKey(),
                Balance = await GetBalance()
            });
        }
        public async Task Transfer(Guid checkingAccountID, decimal amount)
        {
            var checkingAccountGrain = GrainFactory.GetGrain<ICheckingAccountGrain>(checkingAccountID);
            await _balanceTransactionalState.PerformUpdate(state =>
            {
                var currentBalance = state.Balance;
                var newBalance = currentBalance - amount;
                state.Balance = newBalance;
            });

            var streamProvider = this.GetStreamProvider("StreamProvider");
            var streamID = StreamId.Create("BalanceStream", this.GetGrainId().GetGuidKey());
            var stream = streamProvider.GetStream<BalanceChangeEvent>(streamID);

            await stream.OnNextAsync(new BalanceChangeEvent()
            {
                CheckingAccountID = this.GetGrainId().GetGuidKey(),
                Balance = await GetBalance()
            });

            await checkingAccountGrain.Credit(amount);
        }

        public async Task<decimal> GetBalance()
        {
            return await _balanceTransactionalState.PerformRead(state => state.Balance);
        }

        public async Task Initialize(decimal openingBalance)
        {
            _checkingAccountState.State.OpenedAtUtc = DateTime.UtcNow;
            _checkingAccountState.State.AccountType = "Default";
            _checkingAccountState.State.AccountID = this.GetGrainId().GetGuidKey();

            await _balanceTransactionalState.PerformUpdate(state =>
            {
                state.Balance = openingBalance;
            });

            await _checkingAccountState.WriteStateAsync();
        }
    }
}
