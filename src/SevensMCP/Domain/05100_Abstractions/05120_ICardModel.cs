namespace CozyPoC.SevensMCP.Domain.Abstractions
{
    /// <summary>
    /// Represents a playing card with a suit, rank, and associated file path.
    /// </summary>
    /// <remarks>This interface defines the properties of a playing card, including its suit, rank, and the
    /// file path to an associated resource (e.g., an image file). The rank is represented as an integer from 1 to 13,
    /// where 1 corresponds to Ace, 11 to Jack, 12 to Queen, and 13 to King.</remarks>
    public interface ICardModel
    {
        /// <summary>Suit of the card (Club, Diamond, Heart, Spade)</summary>
        Suit Suit { get; }

        /// <summary>1..13（1=Ace, 11=Jack, 12=Queen, 13=King）</summary>
        int Rank { get; }

        /// <summary>実ファイルのフルパス</summary>
        string FilePath { get; }
    }
}
