using Mesada.Domain.Calculo;
using Mesada.Domain.Enums;
using Xunit;

namespace Mesada.Domain.Tests;

public class MotorCalculoMesadaTests
{
    // ---- CalcularValorTarefa: Modo Valor Direto ----

    [Fact]
    public void ValorDireto_ComConclusaoTotal_RetornaValorCadastrado()
    {
        var valor = MotorCalculoMesada.CalcularValorTarefa(ModoCalculo.ValorDireto, valor: 20m, pontos: null, valorPonto: null, percentualConclusao: 100m);
        Assert.Equal(20m, valor);
    }

    [Theory]
    [InlineData(20, 50, 10.00)]
    [InlineData(15, 33, 4.95)]
    [InlineData(10, 0, 0.00)]
    public void ValorDireto_AplicaPercentualDeConclusao(decimal valorCadastrado, decimal percentual, decimal esperado)
    {
        var valor = MotorCalculoMesada.CalcularValorTarefa(ModoCalculo.ValorDireto, valorCadastrado, null, null, percentual);
        Assert.Equal(esperado, valor);
    }

    [Fact]
    public void ValorDireto_SemValorCadastrado_Lanca()
    {
        Assert.Throws<InvalidOperationException>(() =>
            MotorCalculoMesada.CalcularValorTarefa(ModoCalculo.ValorDireto, null, null, null, 100m));
    }

    // ---- CalcularValorTarefa: Modo Pontos ----

    [Fact]
    public void Pontos_ComConclusaoTotal_ConvertePontosParaReais()
    {
        // 10 pontos * R$0,50/ponto = R$5,00
        var valor = MotorCalculoMesada.CalcularValorTarefa(ModoCalculo.Pontos, valor: null, pontos: 10m, valorPonto: 0.50m, percentualConclusao: 100m);
        Assert.Equal(5.00m, valor);
    }

    [Fact]
    public void Pontos_AplicaPercentualDeConclusao()
    {
        // 10 pontos * R$0,50 * 50% = R$2,50
        var valor = MotorCalculoMesada.CalcularValorTarefa(ModoCalculo.Pontos, null, 10m, 0.50m, 50m);
        Assert.Equal(2.50m, valor);
    }

    [Fact]
    public void Pontos_SemPontosCadastrados_Lanca()
    {
        Assert.Throws<InvalidOperationException>(() =>
            MotorCalculoMesada.CalcularValorTarefa(ModoCalculo.Pontos, null, null, 0.5m, 100m));
    }

    [Fact]
    public void Pontos_SemValorDoPontoConfigurado_Lanca()
    {
        Assert.Throws<InvalidOperationException>(() =>
            MotorCalculoMesada.CalcularValorTarefa(ModoCalculo.Pontos, null, 10m, null, 100m));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void PercentualForaDoIntervalo_Lanca(decimal percentualInvalido)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            MotorCalculoMesada.CalcularValorTarefa(ModoCalculo.ValorDireto, 10m, null, null, percentualInvalido));
    }

    // ---- CalcularMulta ----

    [Fact]
    public void Multa_TarefaObrigatoriaNaoFeita_RetornaValorDaMulta()
    {
        var multa = MotorCalculoMesada.CalcularMulta(NaturezaTarefa.Obrigatoria, StatusExecucao.NaoFeito, valorMulta: 15m);
        Assert.Equal(15m, multa);
    }

    [Fact]
    public void Multa_TarefaBonusNaoFeita_NaoGeraMulta()
    {
        var multa = MotorCalculoMesada.CalcularMulta(NaturezaTarefa.Bonus, StatusExecucao.NaoFeito, valorMulta: 15m);
        Assert.Equal(0m, multa);
    }

    [Fact]
    public void Multa_TarefaObrigatoriaCumprida_NaoGeraMulta()
    {
        var multa = MotorCalculoMesada.CalcularMulta(NaturezaTarefa.Obrigatoria, StatusExecucao.Feito, valorMulta: 15m);
        Assert.Equal(0m, multa);
    }

    // ---- Sugestão automática de valor/pontos ----

    [Fact]
    public void SugerirValorDireto_DivideMesadaBasePelasTarefasAderentes()
    {
        var sugestao = MotorCalculoMesada.SugerirValorDireto(mesadaBase: 100m, quantidadeTarefasAderentes: 4);
        Assert.Equal(25m, sugestao);
    }

    [Fact]
    public void SugerirValorDireto_SemTarefasAderentes_Lanca()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => MotorCalculoMesada.SugerirValorDireto(100m, 0));
    }

    [Fact]
    public void SugerirPontos_ConverteMesadaBaseEDivideEntreTarefas()
    {
        // Pontos_totais = 100 / 0.5 = 200; Pontos_sugeridos = 200 / 4 = 50
        var sugestao = MotorCalculoMesada.SugerirPontos(mesadaBase: 100m, valorPonto: 0.5m, quantidadeTarefasAderentes: 4);
        Assert.Equal(50m, sugestao);
    }

    [Fact]
    public void SugerirPontos_ValorDoPontoInvalido_Lanca()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => MotorCalculoMesada.SugerirPontos(100m, 0m, 4));
    }

    // ---- CalcularResumoCiclo ----

    [Fact]
    public void ResumoCiclo_SomaApenasBonusCumpridos()
    {
        var itens = new[]
        {
            new ItemExecucaoCiclo(NaturezaTarefa.Bonus, ModoCalculo.ValorDireto, 10m, null, null, null, StatusExecucao.Feito, 100m),
            new ItemExecucaoCiclo(NaturezaTarefa.Bonus, ModoCalculo.ValorDireto, 20m, null, null, null, StatusExecucao.Parcial, 50m), // 10
            new ItemExecucaoCiclo(NaturezaTarefa.Bonus, ModoCalculo.ValorDireto, 5m, null, null, null, StatusExecucao.Pendente, 100m), // ignorado
        };

        var resumo = MotorCalculoMesada.CalcularResumoCiclo(itens);

        Assert.Equal(20m, resumo.SomaBonus); // 10 + 10
        Assert.Equal(0m, resumo.SomaMultas);
    }

    [Fact]
    public void ResumoCiclo_ObrigatoriaCumprida_NaoSomaBonusNemMulta()
    {
        var itens = new[]
        {
            new ItemExecucaoCiclo(NaturezaTarefa.Obrigatoria, ModoCalculo.ValorDireto, 30m, null, null, 10m, StatusExecucao.Feito, 100m),
        };

        var resumo = MotorCalculoMesada.CalcularResumoCiclo(itens);

        Assert.Equal(0m, resumo.SomaBonus);
        Assert.Equal(0m, resumo.SomaMultas);
    }

    [Fact]
    public void ResumoCiclo_ObrigatoriaNaoFeita_SomaMulta()
    {
        var itens = new[]
        {
            new ItemExecucaoCiclo(NaturezaTarefa.Obrigatoria, ModoCalculo.ValorDireto, 30m, null, null, 10m, StatusExecucao.NaoFeito, 0m),
        };

        var resumo = MotorCalculoMesada.CalcularResumoCiclo(itens);

        Assert.Equal(0m, resumo.SomaBonus);
        Assert.Equal(10m, resumo.SomaMultas);
    }

    // ---- FecharCiclo ----

    [Fact]
    public void FecharCiclo_SemBonusMultaOuDebito_RetornaMesadaBaseIntegral()
    {
        var resultado = MotorCalculoMesada.FecharCiclo(mesadaBase: 100m, somaBonus: 0m, somaMultas: 0m, saldoDevedorAnterior: 0m);
        Assert.Equal(100m, resultado.ValorFinal);
        Assert.Equal(0m, resultado.SaldoDevedorResultante);
    }

    [Fact]
    public void FecharCiclo_ComBonusEMulta_CalculaLiquido()
    {
        var resultado = MotorCalculoMesada.FecharCiclo(mesadaBase: 100m, somaBonus: 30m, somaMultas: 20m, saldoDevedorAnterior: 0m);
        Assert.Equal(110m, resultado.ValorFinal);
        Assert.Equal(0m, resultado.SaldoDevedorResultante);
    }

    [Fact]
    public void FecharCiclo_ResultadoNegativo_ZeraEAcumulaDebito()
    {
        // 50 base - 80 multas = -30 → zera, acumula 30 de débito
        var resultado = MotorCalculoMesada.FecharCiclo(mesadaBase: 50m, somaBonus: 0m, somaMultas: 80m, saldoDevedorAnterior: 0m);
        Assert.Equal(0m, resultado.ValorFinal);
        Assert.Equal(30m, resultado.SaldoDevedorResultante);
    }

    [Fact]
    public void FecharCiclo_DeduzDebitoAnteriorAntesDePagar()
    {
        // 100 base - 40 de débito anterior = 60 pago, débito quitado
        var resultado = MotorCalculoMesada.FecharCiclo(mesadaBase: 100m, somaBonus: 0m, somaMultas: 0m, saldoDevedorAnterior: 40m);
        Assert.Equal(60m, resultado.ValorFinal);
        Assert.Equal(0m, resultado.SaldoDevedorResultante);
    }

    [Fact]
    public void FecharCiclo_DebitoAnteriorMaiorQueOGanho_AcumulaRestante()
    {
        // 30 base - 50 de débito anterior = -20 → zera, acumula 20
        var resultado = MotorCalculoMesada.FecharCiclo(mesadaBase: 30m, somaBonus: 0m, somaMultas: 0m, saldoDevedorAnterior: 50m);
        Assert.Equal(0m, resultado.ValorFinal);
        Assert.Equal(20m, resultado.SaldoDevedorResultante);
    }

    [Fact]
    public void FecharCiclo_SaldoDevedorAnteriorNegativo_Lanca()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            MotorCalculoMesada.FecharCiclo(100m, 0m, 0m, saldoDevedorAnterior: -1m));
    }
}
