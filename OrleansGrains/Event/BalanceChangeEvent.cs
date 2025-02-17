namespace OrleansGrains.Event
{
    [GenerateSerializer]
    public record BalanceChangeEvent
    {
        [Id(0)]
        public Guid CheckingAccountID { get; init; }

        [Id(1)]
        public decimal Balance { get; init; }
    }
}
