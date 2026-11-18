using Clube.Mobile.Services;
using Clube.Mobile.ViewModels;

namespace Clube.Mobile.Views;

public partial class RegisterPage : ContentPage
{
    private readonly RegisterViewModel _vm;

    public RegisterPage(RegisterViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;
    }

    public RegisterPage() : this(AppServiceHelper.GetRequiredService<RegisterViewModel>()) { }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.CarregarClubesAsync();
    }
}
