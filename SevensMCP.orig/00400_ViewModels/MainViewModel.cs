using CozyPoC.SevensMCP.Models;
using System.Collections.ObjectModel;

namespace CozyPoC.SevensMCP.ViewModels
{

    internal sealed partial class MainViewModel
    {
        private SevensTableau Tableau { get; } = new();
        public ObservableCollection<Card> Cards => Tableau.Cards;

        public MainViewModel()
        {

        }
    }
}