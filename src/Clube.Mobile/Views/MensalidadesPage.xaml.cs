using Clube.Mobile.Services;
using Clube.Mobile.ViewModels;

namespace Clube.Mobile.Views;

public partial class MensalidadesPage : ContentPage
{
    private readonly MensalidadesViewModel _vm;

    public MensalidadesPage(MensalidadesViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;
    }

    public MensalidadesPage() : this(AppServiceHelper.GetRequiredService<MensalidadesViewModel>()) { }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.CarregarAsync();
    }
}
