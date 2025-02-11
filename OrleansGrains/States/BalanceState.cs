namespace OrleansGrains.States
{
    [GenerateSerializer]
    public record BalanceState
    {
        [Id(0)]
        public decimal Balance { get; set; }
    }
}
