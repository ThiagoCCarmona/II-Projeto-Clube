using Clube.Mobile.Services;
using Clube.Mobile.ViewModels;

namespace Clube.Mobile.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    public LoginPage() : this(AppServiceHelper.GetRequiredService<LoginViewModel>()) { }
}
