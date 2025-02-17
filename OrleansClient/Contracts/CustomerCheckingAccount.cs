using System.Runtime.Serialization;

namespace OrleansClient.Contracts
{
    [DataContract]
    public record CustomerCheckingAccount
    {
        [DataMember]
        public Guid AccountID { get; init; }
    }
}
