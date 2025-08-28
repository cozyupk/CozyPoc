using CozyPoC.SevensMCP.Domain.Abstractions;

namespace CozyPoC.SevensMCP.Domain.Impl
{
    internal sealed class CardModel : ICardModel
    {
        /// <summary>スート</summary>
        public required Suit Suit { get; init; }

        /// <summary>1..13（1=Ace, 11=Jack, 12=Queen, 13=King）</summary>
        public required int Rank { get; init; }

        /// <summary>画像ファイルのフルパス</summary>
        /// <remarks>PoCレベルのプロジェクトなのでドメインに置きます</remarks>
        public required string FilePath { get; init; }
    }
}


