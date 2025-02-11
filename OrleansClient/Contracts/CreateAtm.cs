using System.Runtime.Serialization;

namespace OrleansClient.Contracts
{
    [DataContract]
    public record CreateAtm
    {
        [DataMember]
        public decimal InitialAtmCashBalance { get; init; }
    }

}
