using CozyPoC.SevensMCP.Abstractions;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CozyPoC.SevensMCP.Models
{
    class ModelsFactory : IModelsFactory
    {
        public (IReadOnlyList<ICard>, ConcurrentDictionary<(Suit, int), ICard>) CreateCardsFromFolder(string cardsDir)
        {

        }

        private static bool TryParseSuit(string s, out Suit suit)
        {
            System.Diagnostics.Debug.WriteLine($"LoadFromFolder: {cardsDir}");
            if (!Directory.Exists(cardsDir)) return;

            // 例: spade_04.png / hearts_10.png / diamond_jack.png などを拾う
            var rx = new Regex(@"^(?<suit>club|clubs|diamond|diamonds|heart|hearts|spade|spades)_(?<rank>\d{1,2}|jack|queen|king)\.(png|jpg|jpeg)$",
                               RegexOptions.IgnoreCase | RegexOptions.Compiled);

            var tmp = new List<Card>();

            foreach (var file in Directory.EnumerateFiles(cardsDir, "*.*", SearchOption.TopDirectoryOnly))
            {
                System.Diagnostics.Debug.WriteLine($"  file: {file}");
                var name = Path.GetFileName(file);
                var m = rx.Match(name);
                if (!m.Success) continue;

                var suitStr = m.Groups["suit"].Value.ToLowerInvariant().TrimEnd('s'); // 複数形→単数
                var rankStr = m.Groups["rank"].Value.ToLowerInvariant();

                if (!TryParseSuit(suitStr, out var suit)) continue;
                if (!TryParseRank(rankStr, out var rank)) continue;

                var card = new Card
                {
                    Suit = suit,
                    Rank = rank,
                    FilePath = file
                };
                CardMap[(suit, rank)] = card;
                tmp.Add(card);
            }

            // 4行（SuitOrder）×13列（Rank 1..13）で整列
            var ordered = tmp
                .OrderBy(c => Array.IndexOf(SuitOrder, c.Suit))
                .ThenBy(c => c.Rank);

            Cards.Clear();
            foreach (var c in ordered) Cards.Add(c);

        }
    }
}
