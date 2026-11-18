using System.Collections.ObjectModel;
using Clube.Domain.Enums;
using Clube.Mobile.Models;
using Clube.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Clube.Mobile.ViewModels;

public partial class SacViewModel : BaseViewModel
{
    private readonly LocalDatabaseService _localDb;

    public ObservableCollection<LocalSolicitacaoItem> Solicitacoes { get; } = [];
    public List<TipoSolicitacao> Tipos { get; } = Enum.GetValues<TipoSolicitacao>().ToList();

    [ObservableProperty] private TipoSolicitacao _tipoSelecionado = TipoSolicitacao.Solicitacao;
    [ObservableProperty] private string _assunto = string.Empty;
    [ObservableProperty] private string _descricao = string.Empty;

    public SacViewModel(LocalDatabaseService localDb, SessionStateService session, MobileSyncService sync)
        : base(session, sync)
    {
        _localDb = localDb;
    }

    public async Task CarregarAsync()
    {
        var itens = await _localDb.GetSolicitacoesAsync();
        Solicitacoes.Clear();
        foreach (var s in itens) Solicitacoes.Add(s);
        await SyncService.AtualizarContagemOutboxAsync();
    }

    [RelayCommand]
    private async Task EnviarAsync()
    {
        if (string.IsNullOrWhiteSpace(Assunto) || string.IsNullOrWhiteSpace(Descricao))
        {
            MensagemStatus = "Preencha assunto e descricao.";
            return;
        }

        MensagemStatus = await SyncService.EnfileirarSolicitacaoAsync(TipoSelecionado, Assunto, Descricao);
        Assunto = string.Empty;
        Descricao = string.Empty;
        await CarregarAsync();
    }

    protected override Task OnPosSincronizacaoAsync() => CarregarAsync();
}
