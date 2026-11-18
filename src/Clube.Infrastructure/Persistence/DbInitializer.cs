using Clube.Application.Payments;
using Clube.Domain.Entities;
using Clube.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Clube.Infrastructure.Persistence;

/// <summary>
/// Popula dados iniciais idempotentes para demonstração multi-clube do sistema Projeto Clubes.
/// </summary>
public static class DbInitializer
{
    public static readonly Guid ClubeFozId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid ClubeGuairaId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public static async Task SeedAsync(ClubeDbContext db, CancellationToken ct = default)
    {
        if (await db.Clubes.IgnoreQueryFilters().AnyAsync(ct))
        {
            return;
        }

        var clubeFoz = new Domain.Entities.Clube
        {
            Id = ClubeFozId,
            Nome = "Rotary Foz Ponte",
            Sigla = "RFP-PR",
            Endereco = "Av. das Cataratas, 1450",
            Bairro = "Vila Yolanda",
            Cidade = "Foz do Iguacu",
            UF = "PR",
            Telefone = "(45) 3523-4000",
            Email = "contato@rotaryfozponte.org.br",
            ChavePix = "12345678000199",
            TipoChavePix = TipoChavePix.CNPJ,
            ValorMensalidadePadrao = 150.00m,
            Ativo = true
        };

        var clubeGuaira = new Domain.Entities.Clube
        {
            Id = ClubeGuairaId,
            Nome = "Iate Clube Guaira",
            Sigla = "ICG-PR",
            Endereco = "Av. Beira Rio, 320",
            Bairro = "Centro",
            Cidade = "Guaira",
            UF = "PR",
            Telefone = "(44) 3642-2200",
            Email = "secretaria@iateguaira.org.br",
            ChavePix = "98765432000155",
            TipoChavePix = TipoChavePix.CNPJ,
            ValorMensalidadePadrao = 180.00m,
            Ativo = true
        };

        db.Clubes.AddRange(clubeFoz, clubeGuaira);

        var hashAdmin = BCrypt.Net.BCrypt.HashPassword("Admin@123");
        var hashUser = BCrypt.Net.BCrypt.HashPassword("User@123");

        var uSuperAdmin = new Usuario
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            ClubeId = null,
            Nome = "Super Administrador da Federacao",
            CPF = "000.000.000-00",
            Email = "admin@federacaoclubes.org.br",
            Telefone = "(45) 99900-0000",
            SenhaHash = hashAdmin,
            Perfil = PerfilUsuario.SuperAdmin
        };

        var uPresFoz = new Usuario
        {
            Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
            ClubeId = ClubeFozId,
            Nome = "Carlos Kaneko",
            CPF = "111.111.111-11",
            Email = "presidente@rotaryfozponte.org.br",
            Telefone = "(45) 99911-1111",
            SenhaHash = hashUser,
            Perfil = PerfilUsuario.Presidente
        };

        var uTesFoz = new Usuario
        {
            Id = Guid.Parse("10000000-0000-0000-0000-000000000002"),
            ClubeId = ClubeFozId,
            Nome = "Helena Tesoureira",
            CPF = "222.222.222-22",
            Email = "tesouraria@rotaryfozponte.org.br",
            Telefone = "(45) 99922-2222",
            SenhaHash = hashUser,
            Perfil = PerfilUsuario.Tesoureiro
        };

        var uFundadorFoz = new Usuario
        {
            Id = Guid.Parse("10000000-0000-0000-0000-000000000003"),
            ClubeId = ClubeFozId,
            Nome = "Marcos Fundador Silva",
            CPF = "333.333.333-33",
            Email = "marcos.fundador@rotaryfozponte.org.br",
            Telefone = "(45) 99933-3333",
            SenhaHash = hashUser,
            Perfil = PerfilUsuario.Associado
        };

        var uConvidadoFoz = new Usuario
        {
            Id = Guid.Parse("10000000-0000-0000-0000-000000000004"),
            ClubeId = ClubeFozId,
            Nome = "Luciana Convidada Oliveira",
            CPF = "444.444.444-44",
            Email = "luciana.convidada@rotaryfozponte.org.br",
            Telefone = "(45) 99944-4444",
            SenhaHash = hashUser,
            Perfil = PerfilUsuario.Associado
        };

        var uSimpatizanteFoz = new Usuario
        {
            Id = Guid.Parse("10000000-0000-0000-0000-000000000005"),
            ClubeId = ClubeFozId,
            Nome = "Rafael Simpatizante Costa",
            CPF = "555.555.555-55",
            Email = "rafael.simpatizante@gmail.com",
            Telefone = "(45) 99955-5555",
            SenhaHash = hashUser,
            Perfil = PerfilUsuario.Simpatizante
        };

        var uPresGuaira = new Usuario
        {
            Id = Guid.Parse("20000000-0000-0000-0000-000000000001"),
            ClubeId = ClubeGuairaId,
            Nome = "Eduardo Presidente Guaira",
            CPF = "666.666.666-66",
            Email = "presidente@iateguaira.org.br",
            Telefone = "(44) 99966-6666",
            SenhaHash = hashUser,
            Perfil = PerfilUsuario.Presidente
        };

        var uPendenteFoz = new Usuario
        {
            Id = Guid.Parse("10000000-0000-0000-0000-000000000006"),
            ClubeId = ClubeFozId,
            Nome = "Juliana Candidata Souza",
            CPF = "777.777.777-77",
            Email = "juliana.candidata@gmail.com",
            Telefone = "(45) 99977-7777",
            SenhaHash = hashUser,
            Perfil = PerfilUsuario.Associado
        };

        var uFundadorGuaira = new Usuario
        {
            Id = Guid.Parse("20000000-0000-0000-0000-000000000002"),
            ClubeId = ClubeGuairaId,
            Nome = "Fernanda Nautica Guaira",
            CPF = "888.888.888-88",
            Email = "fernanda@iateguaira.org.br",
            Telefone = "(44) 99988-8888",
            SenhaHash = hashUser,
            Perfil = PerfilUsuario.Associado
        };

        db.Usuarios.AddRange(
            uSuperAdmin, uPresFoz, uTesFoz, uFundadorFoz,
            uConvidadoFoz, uSimpatizanteFoz, uPresGuaira,
            uPendenteFoz, uFundadorGuaira);

        var aPresFoz = new Associado
        {
            Id = Guid.Parse("a1000000-0000-0000-0000-000000000001"),
            UsuarioId = uPresFoz.Id,
            ClubeId = ClubeFozId,
            Nome = uPresFoz.Nome,
            CPF = uPresFoz.CPF,
            Email = uPresFoz.Email,
            Telefone = uPresFoz.Telefone,
            TipoAssociado = TipoAssociado.SocioFundadorComVoto,
            StatusAprovacao = StatusAprovacao.Aprovado,
            DataNascimento = new DateTime(1978, 5, 14),
            Sexo = "Masculino",
            EstadoCivil = "Casado",
            Endereco = "Rua Marechal Floriano, 410",
            Bairro = "Centro",
            Cidade = "Foz do Iguacu",
            UF = "PR",
            Interesses = "Eventos Beneficentes, Comida, Gestao"
        };

        var aTesFoz = new Associado
        {
            Id = Guid.Parse("a1000000-0000-0000-0000-000000000002"),
            UsuarioId = uTesFoz.Id,
            ClubeId = ClubeFozId,
            Nome = uTesFoz.Nome,
            CPF = uTesFoz.CPF,
            Email = uTesFoz.Email,
            Telefone = uTesFoz.Telefone,
            TipoAssociado = TipoAssociado.SocioFundadorComVoto,
            StatusAprovacao = StatusAprovacao.Aprovado,
            DataNascimento = new DateTime(1983, 8, 22),
            Sexo = "Feminino",
            EstadoCivil = "Casada",
            Endereco = "Rua Almirante Barroso, 880",
            Bairro = "Centro",
            Cidade = "Foz do Iguacu",
            UF = "PR",
            Interesses = "Cafe da Tarde, Comida, Voluntariado"
        };

        var aFundadorFoz = new Associado
        {
            Id = Guid.Parse("a1000000-0000-0000-0000-000000000003"),
            UsuarioId = uFundadorFoz.Id,
            ClubeId = ClubeFozId,
            Nome = uFundadorFoz.Nome,
            CPF = uFundadorFoz.CPF,
            Email = uFundadorFoz.Email,
            Telefone = uFundadorFoz.Telefone,
            TipoAssociado = TipoAssociado.SocioFundadorComVoto,
            StatusAprovacao = StatusAprovacao.Aprovado,
            DataNascimento = new DateTime(1980, 3, 10),
            Sexo = "Masculino",
            EstadoCivil = "Casado",
            Endereco = "Av. Brasil, 900",
            Bairro = "Centro",
            Cidade = "Foz do Iguacu",
            UF = "PR",
            Interesses = "Pesca, Caca, Comida, Bebida"
        };

        var aConvidadoFoz = new Associado
        {
            Id = Guid.Parse("a1000000-0000-0000-0000-000000000004"),
            UsuarioId = uConvidadoFoz.Id,
            ClubeId = ClubeFozId,
            Nome = uConvidadoFoz.Nome,
            CPF = uConvidadoFoz.CPF,
            Email = uConvidadoFoz.Email,
            Telefone = uConvidadoFoz.Telefone,
            TipoAssociado = TipoAssociado.SocioConvidadoSemVoto,
            StatusAprovacao = StatusAprovacao.Aprovado,
            DataNascimento = new DateTime(1990, 7, 18),
            Sexo = "Feminino",
            EstadoCivil = "Solteira",
            Endereco = "Av. Parana, 512",
            Bairro = "Jardim Central",
            Cidade = "Foz do Iguacu",
            UF = "PR",
            Interesses = "Comida, Bebida, Eventos Sociais"
        };

        var aSimpatizanteFoz = new Associado
        {
            Id = Guid.Parse("a1000000-0000-0000-0000-000000000005"),
            UsuarioId = uSimpatizanteFoz.Id,
            ClubeId = ClubeFozId,
            Nome = uSimpatizanteFoz.Nome,
            CPF = uSimpatizanteFoz.CPF,
            Email = uSimpatizanteFoz.Email,
            Telefone = uSimpatizanteFoz.Telefone,
            TipoAssociado = TipoAssociado.Simpatizante,
            StatusAprovacao = StatusAprovacao.Aprovado,
            DataNascimento = new DateTime(1995, 11, 2),
            Sexo = "Masculino",
            EstadoCivil = "Solteiro",
            Endereco = "Rua Santos Dumont, 200",
            Bairro = "Vila A",
            Cidade = "Foz do Iguacu",
            UF = "PR",
            Interesses = "Feijoada, Pesca"
        };

        var aPendenteFoz = new Associado
        {
            Id = Guid.Parse("a1000000-0000-0000-0000-000000000006"),
            UsuarioId = uPendenteFoz.Id,
            ClubeId = ClubeFozId,
            Nome = uPendenteFoz.Nome,
            CPF = uPendenteFoz.CPF,
            Email = uPendenteFoz.Email,
            Telefone = uPendenteFoz.Telefone,
            TipoAssociado = TipoAssociado.SocioFundadorComVoto,
            StatusAprovacao = StatusAprovacao.Pendente,
            DataNascimento = new DateTime(1992, 4, 25),
            Sexo = "Feminino",
            EstadoCivil = "Solteira",
            Endereco = "Rua Rui Barbosa, 340",
            Bairro = "Centro",
            Cidade = "Foz do Iguacu",
            UF = "PR",
            Interesses = "Cafe da Tarde, Eventos Beneficentes"
        };

        var aPresGuaira = new Associado
        {
            Id = Guid.Parse("a2000000-0000-0000-0000-000000000001"),
            UsuarioId = uPresGuaira.Id,
            ClubeId = ClubeGuairaId,
            Nome = uPresGuaira.Nome,
            CPF = uPresGuaira.CPF,
            Email = uPresGuaira.Email,
            Telefone = uPresGuaira.Telefone,
            TipoAssociado = TipoAssociado.SocioFundadorComVoto,
            StatusAprovacao = StatusAprovacao.Aprovado,
            DataNascimento = new DateTime(1975, 2, 12),
            Sexo = "Masculino",
            EstadoCivil = "Casado",
            Endereco = "Rua Bandeirantes, 110",
            Bairro = "Centro",
            Cidade = "Guaira",
            UF = "PR",
            Interesses = "Pesca, Nautica, Churrasco"
        };

        var aFundadorGuaira = new Associado
        {
            Id = Guid.Parse("a2000000-0000-0000-0000-000000000002"),
            UsuarioId = uFundadorGuaira.Id,
            ClubeId = ClubeGuairaId,
            Nome = uFundadorGuaira.Nome,
            CPF = uFundadorGuaira.CPF,
            Email = uFundadorGuaira.Email,
            Telefone = uFundadorGuaira.Telefone,
            TipoAssociado = TipoAssociado.SocioFundadorComVoto,
            StatusAprovacao = StatusAprovacao.Aprovado,
            DataNascimento = new DateTime(1987, 9, 30),
            Sexo = "Feminino",
            EstadoCivil = "Casada",
            Endereco = "Av. Mate Laranjeira, 450",
            Bairro = "Centro",
            Cidade = "Guaira",
            UF = "PR",
            Interesses = "Pesca, Comida, Eventos"
        };

        db.Associados.AddRange(
            aPresFoz, aTesFoz, aFundadorFoz, aConvidadoFoz,
            aSimpatizanteFoz, aPendenteFoz, aPresGuaira, aFundadorGuaira);

        db.Dependentes.AddRange(
            new Dependente
            {
                Id = Guid.Parse("d1000000-0000-0000-0000-000000000001"),
                ClubeId = ClubeFozId,
                AssociadoTitularId = aFundadorFoz.Id,
                Nome = "Beatriz Silva (Esposa)",
                CPF = "529.982.247-25",
                GrauParentesco = GrauParentesco.Conjuge,
                DataNascimento = new DateTime(1982, 6, 15),
                Email = "beatriz.silva@gmail.com",
                Telefone = "(45) 99933-1122"
            },
            new Dependente
            {
                Id = Guid.Parse("d1000000-0000-0000-0000-000000000002"),
                ClubeId = ClubeFozId,
                AssociadoTitularId = aFundadorFoz.Id,
                Nome = "Lucas Silva (Filho)",
                CPF = "987.654.321-00",
                GrauParentesco = GrauParentesco.Filho,
                DataNascimento = new DateTime(2012, 1, 20),
                Email = "lucas.silva@gmail.com",
                Telefone = "(45) 99933-1133"
            });

        var espacoSalaoFoz = new Espaco
        {
            Id = Guid.Parse("e1000000-0000-0000-0000-000000000001"),
            ClubeId = ClubeFozId,
            Nome = "Salao Social Principal",
            Capacidade = 220,
            Descricao = "Salao climatizado com cozinha industrial, palco e churrasqueira.",
            TaxaReserva = 350.00m
        };

        var espacoQuiosqueFoz = new Espaco
        {
            Id = Guid.Parse("e1000000-0000-0000-0000-000000000002"),
            ClubeId = ClubeFozId,
            Nome = "Quiosque Pesca & Churrasco",
            Capacidade = 50,
            Descricao = "Area gourmet coberta proxima ao lago para confraternizacoes.",
            TaxaReserva = 120.00m
        };

        var espacoSalaoGuaira = new Espaco
        {
            Id = Guid.Parse("e2000000-0000-0000-0000-000000000001"),
            ClubeId = ClubeGuairaId,
            Nome = "Salao Nautico Guaira",
            Capacidade = 180,
            Descricao = "Espaco panoramico as margens do Rio Parana.",
            TaxaReserva = 400.00m
        };

        db.Espacos.AddRange(espacoSalaoFoz, espacoQuiosqueFoz, espacoSalaoGuaira);

        var evMaes = new Evento
        {
            Id = Guid.Parse("f1000000-0000-0000-0000-000000000001"),
            ClubeId = ClubeFozId,
            Titulo = "Almoco Dia das Maes",
            Descricao = "Almoco festivo com musica ao vivo e homenagem as maes do clube.",
            TipoEvento = TipoEvento.AlmocoDiaDasMaes,
            PublicoAlvo = PublicoAlvoEvento.AbertoAoPublico,
            DataInicio = new DateTime(2027, 5, 9, 11, 30, 0),
            DataFim = new DateTime(2027, 5, 9, 16, 0, 0),
            Local = "Salao Social Principal",
            ValorIngresso = 65.00m
        };

        var evPais = new Evento
        {
            Id = Guid.Parse("f1000000-0000-0000-0000-000000000002"),
            ClubeId = ClubeFozId,
            Titulo = "Jantar Dia dos Pais",
            Descricao = "Jantar especial exclusivo para associados e familiares.",
            TipoEvento = TipoEvento.JantarDiaDosPais,
            PublicoAlvo = PublicoAlvoEvento.SomenteAssociados,
            DataInicio = new DateTime(2027, 8, 8, 19, 30, 0),
            DataFim = new DateTime(2027, 8, 8, 23, 30, 0),
            Local = "Salao Social Principal",
            ValorIngresso = 80.00m
        };

        var evCafe = new Evento
        {
            Id = Guid.Parse("f1000000-0000-0000-0000-000000000003"),
            ClubeId = ClubeFozId,
            Titulo = "Cafe da Tarde Beneficente",
            Descricao = "Encontro solidario aberto a associados e simpatizantes.",
            TipoEvento = TipoEvento.CafeDaTarde,
            PublicoAlvo = PublicoAlvoEvento.Simpatizantes,
            DataInicio = new DateTime(2027, 6, 20, 15, 0, 0),
            DataFim = new DateTime(2027, 6, 20, 18, 30, 0),
            Local = "Quiosque Pesca & Churrasco",
            ValorIngresso = 35.00m
        };

        var evFeijoada = new Evento
        {
            Id = Guid.Parse("f1000000-0000-0000-0000-000000000004"),
            ClubeId = ClubeFozId,
            Titulo = "Feijoada Beneficente",
            Descricao = "Tradicional feijoada beneficente com preparacao continua no fim de semana.",
            TipoEvento = TipoEvento.FeijoadaBeneficente,
            PublicoAlvo = PublicoAlvoEvento.AbertoAoPublico,
            DataInicio = new DateTime(2027, 10, 31, 11, 0, 0),
            DataFim = new DateTime(2027, 11, 1, 17, 0, 0),
            Local = "Salao Social Principal",
            ValorIngresso = 70.00m
        };

        var evGuaira = new Evento
        {
            Id = Guid.Parse("f2000000-0000-0000-0000-000000000001"),
            ClubeId = ClubeGuairaId,
            Titulo = "Torneio de Pesca & Confraternizacao",
            Descricao = "Abertura da temporada nautica em Guaira.",
            TipoEvento = TipoEvento.Geral,
            PublicoAlvo = PublicoAlvoEvento.SomenteAssociados,
            DataInicio = new DateTime(2027, 9, 12, 7, 0, 0),
            DataFim = new DateTime(2027, 9, 12, 19, 0, 0),
            Local = "Salao Nautico Guaira",
            ValorIngresso = 50.00m
        };

        db.Eventos.AddRange(evMaes, evPais, evCafe, evFeijoada, evGuaira);

        db.ReservasEspacos.AddRange(
            new ReservaEspaco
            {
                Id = Guid.Parse("b1000000-0000-0000-0000-000000000001"),
                EspacoId = espacoSalaoFoz.Id,
                ClubeId = ClubeFozId,
                AssociadoId = aFundadorFoz.Id,
                FinalidadeUso = FinalidadeUsoEspaco.Feijoada,
                DataHoraInicio = new DateTime(2027, 10, 31, 8, 0, 0),
                DataHoraFim = new DateTime(2027, 11, 1, 18, 0, 0),
                Status = StatusReserva.Confirmada,
                Observacoes = "Preparo e realizacao da Feijoada Beneficente (31/10 a 01/11)."
            },
            new ReservaEspaco
            {
                Id = Guid.Parse("b1000000-0000-0000-0000-000000000002"),
                EspacoId = espacoQuiosqueFoz.Id,
                ClubeId = ClubeFozId,
                AssociadoId = aFundadorFoz.Id,
                FinalidadeUso = FinalidadeUsoEspaco.Churrasco,
                DataHoraInicio = new DateTime(2027, 11, 15, 10, 0, 0),
                DataHoraFim = new DateTime(2027, 11, 15, 18, 0, 0),
                Status = StatusReserva.PendenteAprovacao,
                Observacoes = "Confraternizacao da equipe de pesca."
            },
            new ReservaEspaco
            {
                Id = Guid.Parse("b1000000-0000-0000-0000-000000000003"),
                EspacoId = espacoQuiosqueFoz.Id,
                ClubeId = ClubeFozId,
                AssociadoId = aConvidadoFoz.Id,
                FinalidadeUso = FinalidadeUsoEspaco.Festa,
                DataHoraInicio = new DateTime(2027, 11, 15, 12, 0, 0),
                DataHoraFim = new DateTime(2027, 11, 15, 20, 0, 0),
                Status = StatusReserva.PendenteAprovacao,
                Observacoes = "Aniversario em familia (conflito pendente para mediacao do Presidente)."
            },
            new ReservaEspaco
            {
                Id = Guid.Parse("b2000000-0000-0000-0000-000000000001"),
                EspacoId = espacoSalaoGuaira.Id,
                ClubeId = ClubeGuairaId,
                AssociadoId = aFundadorGuaira.Id,
                FinalidadeUso = FinalidadeUsoEspaco.Festa,
                DataHoraInicio = new DateTime(2027, 11, 22, 18, 0, 0),
                DataHoraFim = new DateTime(2027, 11, 22, 23, 30, 0),
                Status = StatusReserva.Confirmada,
                Observacoes = "Jantar dos socios de Guaira."
            });

        var competenciaAtual = DateTime.UtcNow.ToString("MM/yyyy");
        var vencimentoAtual = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 15);
        var pixFundador = PixPayloadGenerator.GerarPayload(
            clubeFoz.ChavePix,
            clubeFoz.Nome,
            clubeFoz.Cidade,
            clubeFoz.ValorMensalidadePadrao,
            "MENSALFOZ01");
        var pixConvidado = PixPayloadGenerator.GerarPayload(
            clubeFoz.ChavePix,
            clubeFoz.Nome,
            clubeFoz.Cidade,
            clubeFoz.ValorMensalidadePadrao,
            "MENSALFOZ02");

        db.Mensalidades.AddRange(
            new Mensalidade
            {
                Id = Guid.Parse("c1000000-0000-0000-0000-000000000001"),
                ClubeId = ClubeFozId,
                AssociadoId = aFundadorFoz.Id,
                Competencia = competenciaAtual,
                Valor = clubeFoz.ValorMensalidadePadrao,
                DataVencimento = vencimentoAtual,
                Status = StatusMensalidade.Pendente,
                CodigoPixCopiaECola = pixFundador,
                Observacoes = "Mensalidade gerada automaticamente na carga inicial."
            },
            new Mensalidade
            {
                Id = Guid.Parse("c1000000-0000-0000-0000-000000000002"),
                ClubeId = ClubeFozId,
                AssociadoId = aConvidadoFoz.Id,
                Competencia = competenciaAtual,
                Valor = clubeFoz.ValorMensalidadePadrao,
                DataVencimento = vencimentoAtual,
                Status = StatusMensalidade.Paga,
                DataPagamento = DateTime.UtcNow.AddDays(-2),
                CodigoPixCopiaECola = pixConvidado,
                Observacoes = "Quitada via PIX."
            });

        db.SolicitacoesReclamacoes.AddRange(
            new SolicitacaoReclamacao
            {
                Id = Guid.Parse("91000000-0000-0000-0000-000000000001"),
                ClubeId = ClubeFozId,
                AssociadoId = aFundadorFoz.Id,
                Tipo = TipoSolicitacao.Solicitacao,
                Assunto = "Iluminacao da quadra de pesca",
                Descricao = "Solicito reforco na iluminacao proxima ao trapiche para eventos noturnos.",
                Status = StatusSolicitacao.Aberta,
                DataCriacao = DateTime.UtcNow.AddDays(-1)
            },
            new SolicitacaoReclamacao
            {
                Id = Guid.Parse("91000000-0000-0000-0000-000000000002"),
                ClubeId = ClubeFozId,
                AssociadoId = aConvidadoFoz.Id,
                Tipo = TipoSolicitacao.Reclamacao,
                Assunto = "Som alto apos 22h no quiosque",
                Descricao = "No ultimo sabado houve musica acima do limite horario permitido.",
                RespostaDiretoria = "Diretoria notificou o responsavel e reforcou a portaria.",
                Status = StatusSolicitacao.Respondida,
                DataCriacao = DateTime.UtcNow.AddDays(-4),
                DataResposta = DateTime.UtcNow.AddDays(-3)
            });

        await db.SaveChangesAsync(ct);
    }
}
