using Clube.Mobile.Services;
using Clube.Mobile.ViewModels;

namespace Clube.Mobile.Views;

public partial class EventosPage : ContentPage
{
    private readonly EventosViewModel _vm;

    public EventosPage(EventosViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;
    }

    public EventosPage() : this(AppServiceHelper.GetRequiredService<EventosViewModel>()) { }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.CarregarAsync();
    }
}
