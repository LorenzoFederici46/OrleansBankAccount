using System.Runtime.Serialization;
namespace OrleansClient.Contracts
{
    [DataContract]   
    public record Transfer
    {
            [DataMember]
            public Guid CheckingAccountID { get; init; }

            [DataMember]
            public decimal Amount { get; init; }
    }
}
