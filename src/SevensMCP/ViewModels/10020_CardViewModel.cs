using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CozyPoC.SevensMCP.Domain.Abstractions;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace CozyPoC.SevensMCP.ViewModels
{
    public partial class CardViewModel(ICardModel model) : ObservableObject
    {
        private static readonly Random _random = new();

        private ICardModel InnerModel { get; } = model;

        public string FilePath => InnerModel.FilePath;

        // Visibility プロパティ（バインディング用）
        [ObservableProperty]
        public Visibility visibility = _random.Next(2) == 0
                ? Visibility.Visible
                : Visibility.Hidden;

        [RelayCommand]
        private Task ClickedAsync()
        {
            return Task.Run(() =>
            {
                Visibility = Visibility == Visibility.Visible
                    ? Visibility.Hidden
                    : Visibility.Visible;
            });
        }
    }
}
