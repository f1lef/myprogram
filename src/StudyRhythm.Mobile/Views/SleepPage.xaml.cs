using StudyRhythm.Mobile.ViewModels;
using StudyRhythm.Mobile.Models;

namespace StudyRhythm.Mobile.Views;

public partial class SleepPage : ContentPage
{
    private readonly SleepViewModel viewModel;

    public SleepPage(SleepViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await viewModel.LoadCommand.ExecuteAsync(null);
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        if (sender is not Button { CommandParameter: SleepHistoryRow row }) return;

        var confirmed = await DisplayAlert(
            "Видалити запис?",
            $"Сон за {row.DateText} буде видалено з історії.",
            "Видалити",
            "Скасувати");
        if (confirmed)
        {
            await viewModel.DeleteCommand.ExecuteAsync(row);
        }
    }
}
