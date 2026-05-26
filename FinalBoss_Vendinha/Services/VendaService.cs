using Microsoft.EntityFrameworkCore;
using FinalBoss_Vendinha.Data;
using FinalBoss_Vendinha.Models;

namespace FinalBoss_Vendinha.Services;

public class VendaService
{
    private readonly string _connectionString;

    public VendaService(string connectionString)
    {
        _connectionString = connectionString;
    }

    public Venda Criar(int clienteId, string descricao, decimal valorTotal, bool pendurar, DateTime? vencimento, int parcelas)
    {
        if (valorTotal <= 0)
        {
            throw new InvalidOperationException("Valor da venda deve ser maior que zero.");
        }

        if (pendurar && parcelas <= 0)
        {
            throw new InvalidOperationException("Numero de parcelas deve ser maior que zero.");
        }

        using var db = new VendinhaDbContext(_connectionString);
        var clienteExiste = db.Clientes.Any(cliente => cliente.Id == clienteId);
        if (!clienteExiste)
        {
            throw new InvalidOperationException("Cliente nao encontrado.");
        }

        var venda = new Venda
        {
            ClienteId = clienteId,
            Descricao = descricao.Trim(),
            ValorTotal = decimal.Round(valorTotal, 2),
            DataVenda = DateTime.Now,
            Pendurada = pendurar
        };

        if (pendurar)
        {
            var divida = db.Dividas
                .Include(item => item.Vendas)
                .FirstOrDefault(item => item.ClienteId == clienteId && !item.Paga);

            if (divida is null)
            {
                divida = new Divida
                {
                    ClienteId = clienteId,
                    ValorOriginal = 0,
                    ValorAtual = 0,
                    DataCriacao = DateTime.Now,
                    DataVencimento = vencimento,
                    NumeroParcelas = parcelas
                };
                db.Dividas.Add(divida);
            }
            else
            {
                divida.DataVencimento = vencimento ?? divida.DataVencimento;
                divida.NumeroParcelas = parcelas;
            }

            divida.ValorOriginal += venda.ValorTotal;
            divida.ValorAtual += venda.ValorTotal;
            venda.Divida = divida;
        }

        db.Vendas.Add(venda);
        db.SaveChanges();
        return venda;
    }

    public List<Venda> ListarTodas()
    {
        using var db = new VendinhaDbContext(_connectionString);
        return db.Vendas
            .AsNoTracking()
            .Include(venda => venda.Cliente)
            .Include(venda => venda.Divida)
            .OrderByDescending(venda => venda.DataVenda)
            .ToList();
    }

    public Venda? BuscarDetalhes(int id)
    {
        using var db = new VendinhaDbContext(_connectionString);
        return db.Vendas
            .AsNoTracking()
            .Include(venda => venda.Cliente)
            .Include(venda => venda.Divida)
            .FirstOrDefault(venda => venda.Id == id);
    }
}
