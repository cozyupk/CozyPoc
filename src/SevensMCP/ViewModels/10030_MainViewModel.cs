using CozyPoC.SevensMCP.Domain.Abstractions;
using CozyPoC.SimplePoCBase.ViewModels;
using System.Collections.ObjectModel;
using System.Linq;

namespace CozyPoC.SevensMCP.ViewModels
{

    public sealed partial class MainViewModel
    {
        public DevToolViewModel DevToolViewModel { get; } = new DevToolViewModel();


        private ISevensTableauModel Tableau { get; }

        /// <summary>
        /// Gets the collection of cards currently managed by the view model.
        /// </summary>
        public ObservableCollection<CardViewModel> Cards { get; }

        public MainViewModel(IModelsFactory factory)
        {
            Tableau = factory.GetOrCreateSevensTableauModel();
            Cards = new ObservableCollection<CardViewModel>(Tableau.Cards.Select(c => new CardViewModel(c)));
        }
    }
}