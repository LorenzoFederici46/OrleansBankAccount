using System.Runtime.Serialization;

namespace OrleansClient.Contracts
{
    [DataContract]
    public record CreateAccount
    {
        public decimal OpeningBalance { get; init; }
    }
}
