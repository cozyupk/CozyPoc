using System.Collections.Generic;

namespace CozyPoC.SevensMCP.Domain.Abstractions
{
    public interface IModelsFactory
    {
        ISevensTableauModel GetOrCreateSevensTableauModel();

        (IReadOnlyList<ICardModel>, IReadOnlyDictionary<(Suit, int), ICardModel>) CreateCardsFromFolder(string cardsDir);
    }
}
