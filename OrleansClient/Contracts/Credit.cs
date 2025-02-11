using System.Runtime.Serialization;
namespace OrleansClient.Contracts
{
    [DataContract]
    public record Credit
    {
        [DataMember]
        public decimal Amount { get; init; }
    }
}
