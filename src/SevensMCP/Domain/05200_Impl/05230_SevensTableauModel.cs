using CozyPoC.SevensMCP.Domain.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;

namespace CozyPoC.SevensMCP.Domain.Impl
{
    internal class SevensTableauModel : ISevensTableauModel
    {
        // 52枚のカードをここで保持
        public IReadOnlyList<ICardModel> Cards { get; }

        // (Suit, Rank) から Card を引くための辞書
        private IReadOnlyDictionary<(Suit, int), ICardModel> CardMap { get; }

        public SevensTableauModel(IModelsFactory factory)
        {
            System.Diagnostics.Debug.WriteLine("MainViewModel Created.");
            var baseDir = AppContext.BaseDirectory;
            var cardsDir = Path.Combine(baseDir, "05200_Impl\\CardImages");
            (Cards, CardMap) = factory.CreateCardsFromFolder(cardsDir);
        }
    }
}
