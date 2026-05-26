using System.ComponentModel.DataAnnotations;

namespace FinalBoss_Vendinha.Models;

public class Divida
{
    public int Id { get; set; }

    [Required]
    public int ClienteId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal ValorOriginal { get; set; }

    public decimal ValorAtual { get; set; }

    public decimal TotalJurosAplicados { get; set; }

    public bool Paga { get; set; }

    public DateTime DataCriacao { get; set; } = DateTime.Now;

    public DateTime? DataVencimento { get; set; }

    public DateTime? DataPagamento { get; set; }

    public int NumeroParcelas { get; set; } = 1;

    [MaxLength(500)]
    public string? Observacoes { get; set; }

    public Cliente? Cliente { get; set; }
    public ICollection<Venda> Vendas { get; set; } = new List<Venda>();
    public ICollection<HistoricoJuros> HistoricoJuros { get; set; } = new List<HistoricoJuros>();

    public bool EmAtraso => !Paga && DataVencimento.HasValue && DateTime.Today > DataVencimento.Value.Date;
}

public class HistoricoJuros
{
    public int Id { get; set; }
    public int DividaId { get; set; }
    public decimal PercentualAplicado { get; set; }
    public decimal ValorJuros { get; set; }
    public decimal ValorAntes { get; set; }
    public decimal ValorDepois { get; set; }
    public DateTime AplicadoEm { get; set; } = DateTime.Now;
    public string? Motivo { get; set; }

    public Divida? Divida { get; set; }
}
