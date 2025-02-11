using System.Runtime.Serialization;
namespace OrleansClient.Contracts
{
    [DataContract]
    public record Debit
    {
        [DataMember]
        public decimal Amount { get; init; }
    }
}
