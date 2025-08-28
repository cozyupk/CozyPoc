using CozyPoC.SevensMCP.Domain.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CozyPoC.SevensMCP.Domain.Impl
{
    /// <summary>
    /// Provides methods for creating and managing card models and tableau models used in the game of Sevens.
    /// </summary>
    /// <remarks>This factory class is responsible for creating instances of card models from a specified
    /// directory and managing the lifecycle of a tableau model for the game. It ensures that the tableau model is
    /// created lazily and provides utility methods for parsing and organizing card data.</remarks>
    public class ModelsFactory : IModelsFactory
    {
        /// <summary>
        /// Defines the order of suits used for arranging cards, typically in a 4-row by 13-column layout.
        /// </summary>
        /// <remarks>The order of suits is defined as Club, Diamond, Heart, and Spade. This order can be
        /// used  for sorting or displaying cards in a standard sequence.</remarks>
        private static readonly Suit[] SuitOrder =
            [Suit.Club, Suit.Diamond, Suit.Heart, Suit.Spade];

        /// <summary>
        /// Gets a lazily initialized instance of the <see cref="ISevensTableauModel"/>.
        /// </summary>
        private Lazy<ISevensTableauModel> LazyTableuModel { get; }
            = new Lazy<ISevensTableauModel>(
                () => new SevensTableauModel(new ModelsFactory()),
                System.Threading.LazyThreadSafetyMode.ExecutionAndPublication
                ) {
            };

        /// <summary>
        /// Retrieves the existing instance of the Sevens tableau model or creates a new one if it does not already
        /// exist.
        /// </summary>
        /// <remarks>This method ensures that the Sevens tableau model is initialized only once and reuses
        /// the same instance for subsequent calls. It is thread-safe and suitable for scenarios where lazy
        /// initialization is required.</remarks>
        /// <returns>An instance of <see cref="ISevensTableauModel"/> representing the Sevens tableau model.</returns>
        public ISevensTableauModel GetOrCreateSevensTableauModel()
        {
            return LazyTableuModel.Value;
        }

        /// <summary>
        /// Creates a collection of card models from image files in the specified folder.
        /// </summary>
        /// <remarks>This method processes image files in the specified directory to create card models.
        /// The files must follow a specific naming convention: <list type="bullet"> <item> <description>The file name
        /// must include the card's suit (e.g., "spade", "heart") and rank (e.g., "4", "jack"), separated by an
        /// underscore.</description> </item> <item> <description>The file extension must be one of <c>.png</c>,
        /// <c>.jpg</c>, or <c>.jpeg</c>.</description> </item> </list> Files that do not match the naming convention
        /// are ignored. The method ensures that the returned list of cards is sorted in a predefined suit order and
        /// ascending rank.</remarks>
        /// <param name="cardsDir">The path to the directory containing card image files. The directory must exist and contain files named in
        /// the format "<c>suit_rank.extension</c>", where <c>suit</c> is the card suit (e.g., "spade", "heart"), and
        /// <c>rank</c> is the card rank (e.g., "4", "jack").</param>
        /// <returns>A tuple containing two elements: <list type="bullet"> <item> <description>An ordered list of card models
        /// (<see cref="ICardModel"/>), sorted first by suit and then by rank.</description> </item> <item>
        /// <description>A dictionary mapping each card's suit and rank to its corresponding card model. The key is a
        /// tuple of <see cref="Suit"/> and rank as an <see cref="int"/>.</description> </item> </list></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="cardsDir"/> is <see langword="null"/>.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown if the directory specified by <paramref name="cardsDir"/> does not exist.</exception>
        public (IReadOnlyList<ICardModel>, IReadOnlyDictionary<(Suit, int), ICardModel>) CreateCardsFromFolder(string cardsDir)
        {
            _ = cardsDir ?? throw new ArgumentNullException(nameof(cardsDir));
            if (!Directory.Exists(cardsDir))
            {
                throw new DirectoryNotFoundException($"Directory not found: {cardsDir}");
            }

            // 例: spade_04.png / hearts_10.png / diamond_jack.png などを拾う
            var rx = new Regex(@"^(?<suit>club|clubs|diamond|diamonds|heart|hearts|spade|spades)_(?<rank>\d{1,2}|jack|queen|king)\.(png|jpg|jpeg)$",
                               RegexOptions.IgnoreCase | RegexOptions.Compiled);

            var tmp = new List<ICardModel>();

            var cardMap = new Dictionary<(Suit, int), ICardModel>();
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

                var card = new CardModel
                {
                    Suit = suit,
                    Rank = rank,
                    FilePath = file
                };
                cardMap[(suit, rank)] = card;
                tmp.Add(card);
            }

            // 4行（SuitOrder）×13列（Rank 1..13）で整列
            var ordered = tmp
                .OrderBy(c => Array.IndexOf(SuitOrder, c.Suit))
                .ThenBy(c => c.Rank);

            return (new List<ICardModel>(ordered), cardMap);
        }

        /// <summary>
        /// Attempts to parse the specified string into a <see cref="Suit"/> enumeration value.
        /// </summary>
        /// <param name="s">The string representation of the suit to parse. Valid values are "club", "diamond", "heart", and "spade".</param>
        /// <param name="suit">When this method returns, contains the <see cref="Suit"/> value corresponding to the parsed string, if the
        /// conversion succeeded; otherwise, contains <see cref="Suit.Spade"/>.</param>
        /// <returns><see langword="true"/> if the string was successfully parsed into a <see cref="Suit"/> value; otherwise,
        /// <see langword="false"/>.</returns>
        private static bool TryParseSuit(string s, out Suit suit)
        {
            switch (s)
            {
                case "club": suit = Suit.Club; return true;
                case "diamond": suit = Suit.Diamond; return true;
                case "heart": suit = Suit.Heart; return true;
                case "spade": suit = Suit.Spade; return true;
                default:
                    suit = Suit.Spade; return false;
            }
        }

        /// <summary>
        /// Attempts to parse a string representation of a rank into its corresponding integer value.
        /// </summary>
        /// <param name="s">The string to parse. This can be a numeric string (e.g., "1", "13") or a rank name (e.g., "jack", "queen",
        /// "king").</param>
        /// <param name="rank">When this method returns, contains the integer value of the rank if the parsing succeeds; otherwise, 0.
        /// Valid ranks are integers from 1 to 13, where 11 represents "jack", 12 represents "queen", and 13 represents
        /// "king".</param>
        /// <returns><see langword="true"/> if the string was successfully parsed into a valid rank; otherwise, <see
        /// langword="false"/>.</returns>
        private static bool TryParseRank(string s, out int rank)
        {
            if (int.TryParse(s, out rank))
            {
                if (rank is >= 1 and <= 13) return true;
                return false;
            }

            rank = s switch
            {
                "jack" => 11,
                "queen" => 12,
                "king" => 13,
                _ => 0
            };
            return rank != 0;
        }
    }
}
