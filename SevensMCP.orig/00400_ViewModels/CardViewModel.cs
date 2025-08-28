using CozyPoC.SevensMCP.Models;

namespace CozyPoC.SevensMCP.ViewModels
{
    internal class CardViewModel
    {
        Card Model { get; }
        public string FilePath => Model.FilePath;
        public CardViewModel(Card model) {
            Model = model;
        }
    }
}
