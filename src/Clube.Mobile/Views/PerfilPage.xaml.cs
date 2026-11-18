using Clube.Mobile.Services;
using Clube.Mobile.ViewModels;

namespace Clube.Mobile.Views;

public partial class PerfilPage : ContentPage
{
    private readonly PerfilViewModel _vm;

    public PerfilPage(PerfilViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;
    }

    public PerfilPage() : this(AppServiceHelper.GetRequiredService<PerfilViewModel>()) { }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.CarregarAsync();
    }
}
