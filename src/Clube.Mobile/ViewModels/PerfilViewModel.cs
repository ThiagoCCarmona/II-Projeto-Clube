using System.Collections.ObjectModel;
using Clube.Domain.Enums;
using Clube.Mobile.Models;
using Clube.Mobile.Services;
using Clube.Shared.DTOs;
using Clube.Shared.Validators;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Clube.Mobile.ViewModels;

public partial class PerfilViewModel : BaseViewModel
{
    private readonly LocalDatabaseService _localDb;
    private readonly ApiClient _api;

    public ObservableCollection<LocalDependenteItem> Dependentes { get; } = [];
    public List<GrauParentesco> GrausParentesco { get; } = Enum.GetValues<GrauParentesco>().ToList();

    [ObservableProperty] private string _depNome = string.Empty;
    [ObservableProperty] private string _depCpf = string.Empty;
    [ObservableProperty] private string _depEmail = string.Empty;
    [ObservableProperty] private GrauParentesco _depParentesco = GrauParentesco.Filho;

    public PerfilViewModel(
        LocalDatabaseService localDb,
        ApiClient api,
        SessionStateService session,
        MobileSyncService sync)
        : base(session, sync)
    {
        _localDb = localDb;
        _api = api;
        _depCpf = CpfValidator.GerarCpfValido(Random.Shared.Next(1000, 999999));
    }

    public string NomeUsuario => Session.Nome;
    public string CpfUsuario => Session.CPF;
    public string ClubeNome => Session.ClubeNome;
    public string TipoAssociadoTexto => Session.TipoAssociado.ToString();
    public string StatusAprovacaoTexto => Session.StatusAprovacao.ToString();
    public string DireitoAVotoTexto => Session.TemDireitoAVoto ? "SIM — Socio Fundador com Voto" : "NAO — Sem Direito a Voto";
    public Color CorDireitoAVoto => Session.TemDireitoAVoto ? Color.FromArgb("#15803D") : Color.FromArgb("#64748B");
    public string Interesses => Session.Interesses;

    public async Task CarregarAsync()
    {
        OnPropertyChanged(nameof(NomeUsuario));
        OnPropertyChanged(nameof(CpfUsuario));
        OnPropertyChanged(nameof(ClubeNome));
        OnPropertyChanged(nameof(TipoAssociadoTexto));
        OnPropertyChanged(nameof(StatusAprovacaoTexto));
        OnPropertyChanged(nameof(DireitoAVotoTexto));
        OnPropertyChanged(nameof(CorDireitoAVoto));
        OnPropertyChanged(nameof(Interesses));

        var itens = await _localDb.GetDependentesAsync();
        Dependentes.Clear();
        foreach (var d in itens) Dependentes.Add(d);
        await SyncService.AtualizarContagemOutboxAsync();
    }

    [RelayCommand]
    private void GerarCpfDependente()
    {
        DepCpf = CpfValidator.GerarCpfValido(Random.Shared.Next(1000, 999999));
    }

    [RelayCommand]
    private async Task AdicionarDependenteAsync()
    {
        if (string.IsNullOrWhiteSpace(DepNome) || !CpfValidator.EhValido(DepCpf))
        {
            MensagemStatus = "Informe um nome e CPF valido para o dependente.";
            return;
        }

        var local = new LocalDependenteItem
        {
            Id = Guid.NewGuid().ToString(),
            Nome = DepNome.Trim(),
            CPF = CpfValidator.Formatar(DepCpf),
            GrauParentesco = DepParentesco.ToString(),
            Email = DepEmail.Trim()
        };

        if (Session.EstaConectado && !string.IsNullOrWhiteSpace(Session.Token))
        {
            try
            {
                var criado = await _api.AdicionarDependenteAsync(Session.AssociadoId, new CriarDependenteRequest(
                    local.Nome, local.CPF, DepParentesco, new DateTime(2012, 1, 1), local.Email, "(45) 99900-0000"));
                if (criado is not null)
                {
                    local.Id = criado.Id.ToString();
                }
            }
            catch (Exception ex)
            {
                MensagemStatus = $"Salvo localmente (API: {ex.Message})";
            }
        }

        await _localDb.AddDependenteLocalAsync(local);
        DepNome = string.Empty;
        DepEmail = string.Empty;
        DepCpf = CpfValidator.GerarCpfValido(Random.Shared.Next(1000, 999999));
        MensagemStatus = "Dependente adicionado!";
        await CarregarAsync();
    }

    [RelayCommand]
    private async Task SairAsync()
    {
        await Shell.Current.GoToAsync("//LoginPage");
    }

    protected override Task OnPosSincronizacaoAsync() => CarregarAsync();
}
