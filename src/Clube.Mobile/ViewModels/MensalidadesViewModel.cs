using System.Collections.ObjectModel;
using Clube.Mobile.Models;
using Clube.Mobile.Services;
using CommunityToolkit.Mvvm.Input;

namespace Clube.Mobile.ViewModels;

public partial class MensalidadesViewModel : BaseViewModel
{
    private readonly LocalDatabaseService _localDb;

    public ObservableCollection<LocalMensalidadeItem> Mensalidades { get; } = [];

    public MensalidadesViewModel(LocalDatabaseService localDb, SessionStateService session, MobileSyncService sync)
        : base(session, sync)
    {
        _localDb = localDb;
    }

    public async Task CarregarAsync()
    {
        var itens = await _localDb.GetMensalidadesAsync();
        Mensalidades.Clear();
        foreach (var m in itens) Mensalidades.Add(m);
        await SyncService.AtualizarContagemOutboxAsync();
    }

    [RelayCommand]
    private async Task CopiarPixAsync(LocalMensalidadeItem? item)
    {
        if (item is null || string.IsNullOrWhiteSpace(item.CodigoPixCopiaECola))
        {
            MensagemStatus = "Codigo PIX indisponivel.";
            return;
        }

        await Clipboard.Default.SetTextAsync(item.CodigoPixCopiaECola);
        MensagemStatus = $"Codigo PIX da competencia {item.Competencia} copiado!";
    }

    protected override Task OnPosSincronizacaoAsync() => CarregarAsync();
}
