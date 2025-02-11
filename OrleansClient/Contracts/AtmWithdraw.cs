using System.Runtime.Serialization;

namespace OrleansClient.Contracts
{
    [DataContract]
    public record AtmWithdraw
    {
            [DataMember]
            public Guid CheckingAccountID { get; init; }

            [DataMember]
            public decimal Amount { get; init; } 
    }
}
