using System.ComponentModel.DataAnnotations;

namespace FinalBoss_Vendinha.Models;

public class Venda
{
    public int Id { get; set; }

    [Required]
    public int ClienteId { get; set; }

    [Required(ErrorMessage = "Descricao e obrigatoria.")]
    [MaxLength(500)]
    public string Descricao { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Valor deve ser maior que zero.")]
    public decimal ValorTotal { get; set; }

    public DateTime DataVenda { get; set; } = DateTime.Now;

    public bool Pendurada { get; set; }

    public int? DividaId { get; set; }

    public Cliente? Cliente { get; set; }
    public Divida? Divida { get; set; }
}
