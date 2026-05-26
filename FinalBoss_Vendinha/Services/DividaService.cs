using Microsoft.EntityFrameworkCore;
using FinalBoss_Vendinha.Data;
using FinalBoss_Vendinha.Models;

namespace FinalBoss_Vendinha.Services;

public class DividaService
{
    private readonly string _connectionString;

    public DividaService(string connectionString)
    {
        _connectionString = connectionString;
    }

    public List<Divida> ListarAbertas()
    {
        using var db = new VendinhaDbContext(_connectionString);
        return db.Dividas
            .AsNoTracking()
            .Include(divida => divida.Cliente)
            .Include(divida => divida.Vendas)
            .Where(divida => !divida.Paga)
            .OrderByDescending(divida => divida.ValorAtual)
            .ToList();
    }

    public Divida? BuscarDetalhes(int id)
    {
        using var db = new VendinhaDbContext(_connectionString);
        return db.Dividas
            .AsNoTracking()
            .Include(divida => divida.Cliente)
            .Include(divida => divida.Vendas.OrderByDescending(venda => venda.DataVenda))
            .Include(divida => divida.HistoricoJuros.OrderByDescending(juros => juros.AplicadoEm))
            .FirstOrDefault(divida => divida.Id == id);
    }

    public void Quitar(int id)
    {
        using var db = new VendinhaDbContext(_connectionString);
        var divida = db.Dividas.FirstOrDefault(item => item.Id == id)
            ?? throw new InvalidOperationException("Divida nao encontrada.");

        if (divida.Paga)
        {
            throw new InvalidOperationException("Esta divida ja esta quitada.");
        }

        divida.Paga = true;
        divida.DataPagamento = DateTime.Now;
        db.SaveChanges();
    }

    public HistoricoJuros AplicarJuros(int id, decimal percentual, string? motivo)
    {
        if (percentual <= 0)
        {
            throw new InvalidOperationException("Percentual de juros deve ser maior que zero.");
        }

        using var db = new VendinhaDbContext(_connectionString);
        var divida = db.Dividas.FirstOrDefault(item => item.Id == id)
            ?? throw new InvalidOperationException("Divida nao encontrada.");

        if (divida.Paga)
        {
            throw new InvalidOperationException("Nao e possivel aplicar juros em divida paga.");
        }

        if (!divida.EmAtraso)
        {
            throw new InvalidOperationException("Juros so podem ser aplicados quando a divida esta em atraso.");
        }

        var valorAntes = divida.ValorAtual;
        var valorJuros = decimal.Round(valorAntes * (percentual / 100m), 2);
        divida.ValorAtual += valorJuros;
        divida.TotalJurosAplicados += valorJuros;

        var historico = new HistoricoJuros
        {
            DividaId = id,
            PercentualAplicado = percentual,
            ValorAntes = valorAntes,
            ValorJuros = valorJuros,
            ValorDepois = divida.ValorAtual,
            AplicadoEm = DateTime.Now,
            Motivo = string.IsNullOrWhiteSpace(motivo) ? "Atraso no pagamento" : motivo.Trim()
        };

        db.HistoricoJuros.Add(historico);
        db.SaveChanges();
        return historico;
    }

}
