using Clube.Domain.Enums;
using Clube.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Clube.Mobile.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly ApiClient _api;
    private readonly SessionStateService _session;
    private readonly MobileSyncService _sync;

    [ObservableProperty]
    private string _cpf = "333.333.333-33";

    [ObservableProperty]
    private string _senha = "User@123";

    [ObservableProperty]
    private string _mensagemErro = string.Empty;

    [ObservableProperty]
    private bool _ocupado;

    public LoginViewModel(ApiClient api, SessionStateService session, MobileSyncService sync)
    {
        _api = api;
        _session = session;
        _sync = sync;
    }

    [RelayCommand]
    private void PreencherDemo(string perfil)
    {
        MensagemErro = string.Empty;
        switch (perfil)
        {
            case "fundador":
                Cpf = "333.333.333-33";
                Senha = "User@123";
                break;
            case "convidado":
                Cpf = "444.444.444-44";
                Senha = "User@123";
                break;
            case "simpatizante":
                Cpf = "555.555.555-55";
                Senha = "User@123";
                break;
            case "presidente":
                Cpf = "111.111.111-11";
                Senha = "User@123";
                break;
        }
    }

    [RelayCommand]
    private async Task EntrarAsync()
    {
        if (Ocupado) return;
        Ocupado = true;
        MensagemErro = string.Empty;

        try
        {
            var login = await _api.LoginAsync(Cpf, Senha);
            if (login is null)
            {
                MensagemErro = "CPF ou senha invalidos.";
                return;
            }

            _session.ModoOfflineSimulado = false;
            _session.AplicarLogin(login);
            await _sync.SincronizarAgoraAsync();
            await Shell.Current.GoToAsync("//EventosPage");
        }
        catch (Exception)
        {
            MensagemErro = "API indisponivel no momento. Use 'Entrar Offline (Cache)' para demonstrar o modo offline-first.";
        }
        finally
        {
            Ocupado = false;
        }
    }

    [RelayCommand]
    private async Task EntrarOfflineAsync()
    {
        var (nome, tipo, voto) = Cpf.Trim() switch
        {
            "444.444.444-44" => ("Luciana Convidada Oliveira", TipoAssociado.SocioConvidadoSemVoto, false),
            "555.555.555-55" => ("Rafael Simpatizante Costa", TipoAssociado.Simpatizante, false),
            _ => ("Marcos Fundador Silva", TipoAssociado.SocioFundadorComVoto, true)
        };

        _session.EntrarModoOfflineDemonstracao(Cpf, nome, tipo, voto);
        await _sync.AtualizarContagemOutboxAsync();
        await Shell.Current.GoToAsync("//EventosPage");
    }

    [RelayCommand]
    private async Task IrParaCadastroAsync()
    {
        await Shell.Current.GoToAsync(nameof(Views.RegisterPage));
    }
}
