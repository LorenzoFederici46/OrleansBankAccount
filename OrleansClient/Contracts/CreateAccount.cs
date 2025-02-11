﻿using System.Runtime.Serialization;
namespace OrleansClient.Contracts
{
    [DataContract]
    public record CreateAccount
    {
        [DataMember]
        public decimal OpeningBalance { get; init; }
    }
}
