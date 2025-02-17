namespace OrleansGrains.States
{
    [GenerateSerializer]
    public record CustomerState
    {
        [Id(0)]
        public Dictionary<Guid, decimal> CheckingAccountBalanceByID { get; set; } = new Dictionary<Guid, decimal>();
    }
}
