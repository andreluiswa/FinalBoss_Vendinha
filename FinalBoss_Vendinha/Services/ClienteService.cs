using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using FinalBoss_Vendinha.Data;
using FinalBoss_Vendinha.Models;

namespace FinalBoss_Vendinha.Services;

public class ClienteService
{
    private readonly string _connectionString;

    public ClienteService(string connectionString)
    {
        _connectionString = connectionString;
    }

    public Cliente Criar(string nome, string cpf, DateTime dataNascimento, string? email)
    {
        using var db = CriarContexto();
        var cliente = new Cliente
        {
            Nome = nome.Trim(),
            Cpf = cpf.Trim(),
            DataNascimento = dataNascimento.Date,
            Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLower()
        };

        ValidarCliente(cliente);
        GarantirCpfUnico(db, cliente.Cpf);

        db.Clientes.Add(cliente);
        db.SaveChanges();
        return cliente;
    }

    public Cliente Atualizar(int id, string nome, string cpf, DateTime dataNascimento, string? email)
    {
        using var db = CriarContexto();
        var cliente = db.Clientes.FirstOrDefault(item => item.Id == id)
            ?? throw new InvalidOperationException("Cliente nao encontrado.");

        cliente.Nome = nome.Trim();
        cliente.Cpf = cpf.Trim();
        cliente.DataNascimento = dataNascimento.Date;
        cliente.Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLower();

        ValidarCliente(cliente);
        if (db.Clientes.Any(item => item.Id != id && item.Cpf == cliente.Cpf))
        {
            throw new InvalidOperationException("Ja existe outro cliente cadastrado com este CPF.");
        }

        db.SaveChanges();
        return cliente;
    }

    public void Excluir(int id)
    {
        using var db = CriarContexto();
        var cliente = db.Clientes
            .Include(item => item.Vendas)
            .Include(item => item.Dividas)
            .FirstOrDefault(item => item.Id == id)
            ?? throw new InvalidOperationException("Cliente nao encontrado.");

        db.Clientes.Remove(cliente);
        db.SaveChanges();
    }

    public Cliente? BuscarPorId(int id)
    {
        using var db = CriarContexto();
        return db.Clientes
            .Include(cliente => cliente.Vendas.OrderByDescending(venda => venda.DataVenda))
            .Include(cliente => cliente.Dividas.Where(divida => !divida.Paga))
            .FirstOrDefault(cliente => cliente.Id == id);
    }

    public List<ClienteResumo> Listar(string? busca)
    {
        using var db = CriarContexto();
        var consulta = db.Clientes.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(busca))
        {
            consulta = consulta.Where(cliente => EF.Functions.Like(cliente.Nome, $"%{busca.Trim()}%"));
        }

        return consulta
            .Select(cliente => new
            {
                Cliente = cliente,
                TotalDividasAbertas = cliente.Dividas
                    .Where(divida => !divida.Paga)
                    .Sum(divida => (decimal?)divida.ValorAtual) ?? 0m
            })
            .OrderByDescending(resumo => resumo.TotalDividasAbertas)
            .ThenBy(resumo => resumo.Cliente.Nome)
            .AsEnumerable()
            .Select(resumo => new ClienteResumo(resumo.Cliente, resumo.TotalDividasAbertas))
            .ToList();
    }

    private VendinhaDbContext CriarContexto()
    {
        return new VendinhaDbContext(_connectionString);
    }

    private static void ValidarCliente(Cliente cliente)
    {
        if (cliente.DataNascimento.Date >= DateTime.Today)
        {
            throw new ValidationException("Data de nascimento deve ser anterior a hoje.");
        }

        var erros = new List<ValidationResult>();
        var contexto = new ValidationContext(cliente);
        if (!Validator.TryValidateObject(cliente, contexto, erros, true))
        {
            throw new ValidationException(string.Join(Environment.NewLine, erros.Select(erro => erro.ErrorMessage)));
        }
    }

    private static void GarantirCpfUnico(VendinhaDbContext db, string cpf)
    {
        if (db.Clientes.Any(cliente => cliente.Cpf == cpf))
        {
            throw new InvalidOperationException("Ja existe um cliente cadastrado com este CPF.");
        }
    }
}
