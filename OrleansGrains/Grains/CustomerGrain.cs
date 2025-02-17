using Orleans.Streams;
using OrleansGrains.Abstraction;
using OrleansGrains.Event;
using OrleansGrains.States;
namespace OrleansGrains.Grains
{
    class CustomerGrain : Grain, ICustomerGrain, IAsyncObserver<BalanceChangeEvent>
    {
        private readonly IPersistentState<CustomerState> _customerState;

        public CustomerGrain([PersistentState("customer", "tableStorage")] IPersistentState<CustomerState> customerState)
        {
            _customerState = customerState;
        }

        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            var streamProvider = this.GetStreamProvider("StreamProvider");
            foreach (var checkingAccountID in _customerState.State.CheckingAccountBalanceByID.Keys)
            {
                var streamID = StreamId.Create("BalanceStream", checkingAccountID);
                var stream = streamProvider.GetStream<BalanceChangeEvent>(streamID);
                var handles = await stream.GetAllSubscriptionHandles();

                foreach (var handle in handles)
                {
                    await handle.ResumeAsync(this);
                }
            }
        }

        public async Task AddCheckingAccount(Guid checkingAccountID)
        {
            _customerState.State.CheckingAccountBalanceByID.Add(checkingAccountID, 0);
            var streamProvider = this.GetStreamProvider("StreamProvider");
            var streamID = StreamId.Create("BalanceStream", checkingAccountID);
            var stream = streamProvider.GetStream<BalanceChangeEvent>(streamID);

            await stream.SubscribeAsync(this);
            await _customerState.WriteStateAsync();
        }

        public async Task<decimal> GetNetWorth()
        {
            return _customerState.State.CheckingAccountBalanceByID.Values.Sum();
        }

        public Task OnCompletedAsync()
        {
            return Task.CompletedTask;
        }

        public Task OnErrorAsync(Exception ex)
        {
            return Task.CompletedTask;
        }

        public async Task OnNextAsync(BalanceChangeEvent item, StreamSequenceToken? token = null)
        {
            var checkingAccountBalancesById = _customerState.State.CheckingAccountBalanceByID;

            if (checkingAccountBalancesById.ContainsKey(item.CheckingAccountID))
            {
                checkingAccountBalancesById[item.CheckingAccountID] = item.Balance;
            }
            else
            {
                checkingAccountBalancesById.Add(item.CheckingAccountID, item.Balance);
            }

            await _customerState.WriteStateAsync();
        }
    }
}
