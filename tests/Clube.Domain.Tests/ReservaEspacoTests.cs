using Clube.Domain.Entities;

namespace Clube.Domain.Tests;

public class ReservaEspacoTests
{
    [Fact]
    public void PeriodoEhValido_DevePermitirReservaContinuaMultiDia()
    {
        var reserva = new ReservaEspaco
        {
            Id = Guid.NewGuid(),
            EspacoId = Guid.NewGuid(),
            DataHoraInicio = new DateTime(2026, 10, 31, 8, 0, 0),
            DataHoraFim = new DateTime(2026, 11, 1, 18, 0, 0)
        };

        Assert.True(reserva.PeriodoEhValido());
    }

    [Fact]
    public void PeriodoEhValido_DeveRetornarFalse_QuandoFimMenorOuIgualInicio()
    {
        var inicio = new DateTime(2026, 10, 31, 10, 0, 0);
        var reservaIgual = new ReservaEspaco { DataHoraInicio = inicio, DataHoraFim = inicio };
        var reservaInvertida = new ReservaEspaco { DataHoraInicio = inicio, DataHoraFim = inicio.AddHours(-1) };

        Assert.False(reservaIgual.PeriodoEhValido());
        Assert.False(reservaInvertida.PeriodoEhValido());
    }

    [Fact]
    public void SobrepoeHorario_DeveDetectarSobreposicao_ENaoConflitarEmIntervalosAdjacentes()
    {
        var reserva = new ReservaEspaco
        {
            Id = Guid.NewGuid(),
            EspacoId = Guid.NewGuid(),
            DataHoraInicio = new DateTime(2026, 10, 31, 8, 0, 0),
            DataHoraFim = new DateTime(2026, 11, 1, 18, 0, 0)
        };

        // Sobreposição parcial no segundo dia
        Assert.True(reserva.SobrepoeHorario(
            new DateTime(2026, 11, 1, 12, 0, 0),
            new DateTime(2026, 11, 1, 20, 0, 0)));

        // Adjacente exato (termina às 18:00, próxima inicia às 18:00)
        Assert.False(reserva.SobrepoeHorario(
            new DateTime(2026, 11, 1, 18, 0, 0),
            new DateTime(2026, 11, 1, 22, 0, 0)));
    }

    [Fact]
    public void ConflitaCom_DeveConsiderarApenasMesmoEspacoEReservasDistintas()
    {
        var espacoA = Guid.NewGuid();
        var espacoB = Guid.NewGuid();
        var inicio = new DateTime(2026, 11, 15, 10, 0, 0);
        var fim = new DateTime(2026, 11, 15, 16, 0, 0);

        var r1 = new ReservaEspaco { Id = Guid.NewGuid(), EspacoId = espacoA, DataHoraInicio = inicio, DataHoraFim = fim };
        var r2MesmoEspaco = new ReservaEspaco { Id = Guid.NewGuid(), EspacoId = espacoA, DataHoraInicio = inicio.AddHours(2), DataHoraFim = fim.AddHours(2) };
        var r3OutroEspaco = new ReservaEspaco { Id = Guid.NewGuid(), EspacoId = espacoB, DataHoraInicio = inicio, DataHoraFim = fim };

        Assert.False(r1.ConflitaCom(r1));
        Assert.True(r1.ConflitaCom(r2MesmoEspaco));
        Assert.False(r1.ConflitaCom(r3OutroEspaco));
    }
}
