using CozyPoC.SevensMCP.Abstractions;

namespace CozyPoC.SevensMCP.Models;


internal sealed class Card : ICard
{
    public required Suit Suit { get; init; }
    /// <summary>1..13（1=Ace, 11=Jack, 12=Queen, 13=King）</summary>
    public required int Rank { get; init; }
    /// <summary>実ファイルのフルパス</summary>
    public required string FilePath { get; init; }
}