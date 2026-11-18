using Clube.Mobile.Services;
using Clube.Mobile.ViewModels;

namespace Clube.Mobile.Views;

public partial class SacPage : ContentPage
{
    private readonly SacViewModel _vm;

    public SacPage(SacViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;
    }

    public SacPage() : this(AppServiceHelper.GetRequiredService<SacViewModel>()) { }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.CarregarAsync();
    }
}
