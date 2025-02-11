namespace OrleansGrains.States
{
    [GenerateSerializer]
    public record AtmState
    {
        [Id(0)]
        public Guid ID { get; set; }    

        [Id(1)]
        public decimal Balance { get; set; }
    }
}
