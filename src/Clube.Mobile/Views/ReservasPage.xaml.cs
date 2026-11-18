using Clube.Mobile.Services;
using Clube.Mobile.ViewModels;

namespace Clube.Mobile.Views;

public partial class ReservasPage : ContentPage
{
    private readonly ReservasViewModel _vm;

    public ReservasPage(ReservasViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;
    }

    public ReservasPage() : this(AppServiceHelper.GetRequiredService<ReservasViewModel>()) { }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.CarregarAsync();
    }
}
