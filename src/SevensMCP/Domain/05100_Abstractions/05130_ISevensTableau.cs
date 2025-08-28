using System.Collections.Generic;

namespace CozyPoC.SevensMCP.Domain.Abstractions
{
    public interface ISevensTableauModel
    {
        IReadOnlyList<ICardModel> Cards { get; }
    }
}
