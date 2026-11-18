using System.Collections.ObjectModel;
using Clube.Domain.Enums;
using Clube.Mobile.Models;
using Clube.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Clube.Mobile.ViewModels;

public partial class ReservasViewModel : BaseViewModel
{
    private readonly LocalDatabaseService _localDb;

    public ObservableCollection<LocalEspacoItem> Espacos { get; } = [];
    public ObservableCollection<LocalReservaItem> Reservas { get; } = [];
    public List<FinalidadeUsoEspaco> Finalidades { get; } = Enum.GetValues<FinalidadeUsoEspaco>().ToList();

    [ObservableProperty] private LocalEspacoItem? _espacoSelecionado;
    [ObservableProperty] private FinalidadeUsoEspaco _finalidadeSelecionada = FinalidadeUsoEspaco.Feijoada;
    [ObservableProperty] private DateTime _dataInicio = new(2027, 12, 20);
    [ObservableProperty] private TimeSpan _horaInicio = new(8, 0, 0);
    [ObservableProperty] private DateTime _dataFim = new(2027, 12, 21);
    [ObservableProperty] private TimeSpan _horaFim = new(18, 0, 0);
    [ObservableProperty] private string _observacoes = "Reserva multi-dia solicitada pelo app";

    public ReservasViewModel(LocalDatabaseService localDb, SessionStateService session, MobileSyncService sync)
        : base(session, sync)
    {
        _localDb = localDb;
    }

    public async Task CarregarAsync()
    {
        var espacos = await _localDb.GetEspacosAsync();
        Espacos.Clear();
        foreach (var e in espacos) Espacos.Add(e);
        if (EspacoSelecionado is null) EspacoSelecionado = Espacos.FirstOrDefault();

        var reservas = await _localDb.GetReservasAsync();
        Reservas.Clear();
        foreach (var r in reservas) Reservas.Add(r);

        await SyncService.AtualizarContagemOutboxAsync();
    }

    [RelayCommand]
    private async Task SolicitarReservaAsync()
    {
        if (EspacoSelecionado is null)
        {
            MensagemStatus = "Selecione um espaco.";
            return;
        }

        var inicio = DataInicio.Date + HoraInicio;
        var fim = DataFim.Date + HoraFim;
        if (fim <= inicio)
        {
            MensagemStatus = "A data/hora final deve ser maior que a data/hora inicial.";
            return;
        }

        MensagemStatus = await SyncService.EnfileirarReservaAsync(
            EspacoSelecionado, FinalidadeSelecionada, inicio, fim, Observacoes);
        await CarregarAsync();
    }

    protected override Task OnPosSincronizacaoAsync() => CarregarAsync();
}
