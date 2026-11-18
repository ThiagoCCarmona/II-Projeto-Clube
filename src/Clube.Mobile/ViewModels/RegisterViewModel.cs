using System.Collections.ObjectModel;
using Clube.Domain.Enums;
using Clube.Mobile.Services;
using Clube.Shared.DTOs;
using Clube.Shared.Validators;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Clube.Mobile.ViewModels;

public partial class RegisterViewModel : ObservableObject
{
    private readonly ApiClient _api;

    public ObservableCollection<ClubeOption> Clubes { get; } = [];
    public List<TipoAssociado> TiposAssociado { get; } = Enum.GetValues<TipoAssociado>().ToList();

    [ObservableProperty] private ClubeOption? _clubeSelecionado;
    [ObservableProperty] private string _nome = string.Empty;
    [ObservableProperty] private string _cpf = string.Empty;
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _telefone = "(45) 99900-0000";
    [ObservableProperty] private string _senha = "User@123";
    [ObservableProperty] private TipoAssociado _tipoSelecionado = TipoAssociado.SocioFundadorComVoto;
    [ObservableProperty] private DateTime _dataNascimento = new(1995, 1, 1);
    [ObservableProperty] private string _sexo = "Masculino";
    [ObservableProperty] private string _estadoCivil = "Solteiro(a)";
    [ObservableProperty] private string _endereco = "Av. Brasil, 100";
    [ObservableProperty] private string _bairro = "Centro";
    [ObservableProperty] private string _cidade = "Foz do Iguacu";
    [ObservableProperty] private string _uf = "PR";
    [ObservableProperty] private string _interesses = "Pesca, Comida, Bebida";
    [ObservableProperty] private string _mensagem = string.Empty;
    [ObservableProperty] private bool _sucesso;

    public RegisterViewModel(ApiClient api)
    {
        _api = api;
        Cpf = CpfValidator.GerarCpfValido(Random.Shared.Next(1000, 999999));
    }

    public async Task CarregarClubesAsync()
    {
        Clubes.Clear();
        try
        {
            var lista = await _api.ListarClubesAsync();
            foreach (var c in lista)
            {
                Clubes.Add(new ClubeOption(c.Id, c.Nome));
            }
        }
        catch
        {
            Clubes.Add(new ClubeOption(SessionStateService.DefaultClubeFozId, "Rotary Foz Ponte"));
            Clubes.Add(new ClubeOption(Guid.Parse("22222222-2222-2222-2222-222222222222"), "Iate Clube Guaira"));
        }

        ClubeSelecionado = Clubes.FirstOrDefault();
    }

    [RelayCommand]
    private void GerarCpfValido()
    {
        Cpf = CpfValidator.GerarCpfValido(Random.Shared.Next(1000, 999999));
    }

    [RelayCommand]
    private async Task EnviarCadastroAsync()
    {
        Sucesso = false;
        if (ClubeSelecionado is null)
        {
            Mensagem = "Selecione um clube.";
            return;
        }

        if (!CpfValidator.EhValido(Cpf))
        {
            Mensagem = "CPF invalido segundo o algoritmo de validacao.";
            return;
        }

        if (string.IsNullOrWhiteSpace(Nome) || string.IsNullOrWhiteSpace(Email))
        {
            Mensagem = "Nome e E-mail sao obrigatorios.";
            return;
        }

        try
        {
            var (ok, msg) = await _api.RegistroPublicoAsync(new RegistroPublicoRequest(
                ClubeSelecionado.Id, Nome, Cpf, Email, Telefone, Senha,
                TipoSelecionado, DataNascimento, Sexo, EstadoCivil,
                Endereco, Bairro, Cidade, Uf, Interesses));

            Sucesso = ok;
            Mensagem = msg;
        }
        catch (Exception ex)
        {
            Mensagem = $"Nao foi possivel conectar a API: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task VoltarAsync() => await Shell.Current.GoToAsync("..");

    public sealed record ClubeOption(Guid Id, string Nome)
    {
        public override string ToString() => Nome;
    }
}
