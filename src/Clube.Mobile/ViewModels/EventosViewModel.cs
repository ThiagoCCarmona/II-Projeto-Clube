using System.Collections.ObjectModel;
using Clube.Mobile.Models;
using Clube.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Clube.Mobile.ViewModels;

public partial class EventosViewModel : BaseViewModel
{
    private readonly LocalDatabaseService _localDb;

    public ObservableCollection<LocalEventoItem> Eventos { get; } = [];

    [ObservableProperty]
    private int _quantidadeAcompanhantes = 1;

    public EventosViewModel(LocalDatabaseService localDb, SessionStateService session, MobileSyncService sync)
        : base(session, sync)
    {
        _localDb = localDb;
    }

    public async Task CarregarAsync()
    {
        var itens = await _localDb.GetEventosAsync();
        Eventos.Clear();
        foreach (var item in itens)
        {
            Eventos.Add(item);
        }
        await SyncService.AtualizarContagemOutboxAsync();
    }

    [RelayCommand]
    private async Task ConfirmarPresencaAsync(LocalEventoItem? evento)
    {
        if (evento is null) return;
        MensagemStatus = await SyncService.EnfileirarInscricaoEventoAsync(evento.Id, QuantidadeAcompanhantes);
        await CarregarAsync();
    }

    protected override Task OnPosSincronizacaoAsync() => CarregarAsync();
}
