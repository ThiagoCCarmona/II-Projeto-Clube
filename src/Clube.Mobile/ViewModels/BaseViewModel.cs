using Clube.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Clube.Mobile.ViewModels;

/// <summary>
/// ViewModel base com controle de conectividade Online/Offline, contador de Outbox e sincronização.
/// </summary>
public abstract partial class BaseViewModel : ObservableObject
{
    protected SessionStateService Session { get; }
    protected MobileSyncService SyncService { get; }

    [ObservableProperty]
    private bool _ocupado;

    [ObservableProperty]
    private string _mensagemStatus = string.Empty;

    protected BaseViewModel(SessionStateService session, MobileSyncService syncService)
    {
        Session = session;
        SyncService = syncService;
        Session.StateChanged += () =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                OnPropertyChanged(nameof(TextoStatusRede));
                OnPropertyChanged(nameof(CorStatusRede));
                OnPropertyChanged(nameof(PendenciasOutbox));
                OnPropertyChanged(nameof(TextoBotaoModoRede));
            });
        };
    }

    public string TextoStatusRede => Session.TextoStatusRede;
    public Color CorStatusRede => Session.EstaConectado ? Color.FromArgb("#15803D") : Color.FromArgb("#B45309");
    public int PendenciasOutbox => Session.PendenciasOutbox;
    public string TextoBotaoModoRede => Session.ModoOfflineSimulado ? "Ficar Online" : "Simular Offline";

    [RelayCommand]
    private async Task AlternarModoOfflineAsync()
    {
        Session.ModoOfflineSimulado = !Session.ModoOfflineSimulado;
        Session.NotifyStateChanged();
        MensagemStatus = Session.ModoOfflineSimulado
            ? "Modo Offline simulado ativado. Novas operacoes irao para a fila Outbox."
            : "Modo Online reativado. Clique em Sincronizar para enviar pendencias.";
        await SyncService.AtualizarContagemOutboxAsync();
    }

    [RelayCommand]
    private async Task SincronizarAgoraAsync()
    {
        if (Ocupado) return;
        Ocupado = true;
        try
        {
            MensagemStatus = await SyncService.SincronizarAgoraAsync();
            await OnPosSincronizacaoAsync();
        }
        finally
        {
            Ocupado = false;
        }
    }

    protected virtual Task OnPosSincronizacaoAsync() => Task.CompletedTask;
}
