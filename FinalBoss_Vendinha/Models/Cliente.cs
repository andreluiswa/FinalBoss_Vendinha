using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinalBoss_Vendinha.Models;

public class Cliente
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome do cliente e obrigatorio.")]
    [StringLength(100, MinimumLength = 5, ErrorMessage = "Informe o nome completo.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "O CPF e obrigatorio.")]
    [StringLength(14)]
    [RegularExpression(@"^\d{3}\.\d{3}\.\d{3}-\d{2}$", ErrorMessage = "O CPF deve estar no formato 000.000.000-00.")]
    public string Cpf { get; set; } = string.Empty;

    [Required(ErrorMessage = "A data de nascimento e obrigatoria.")]
    public DateTime DataNascimento { get; set; }

    [NotMapped]
    public int Idade
    {
        get
        {
            var hoje = DateTime.Today;
            var idade = hoje.Year - DataNascimento.Year;
            if (DataNascimento.Date > hoje.AddYears(-idade))
            {
                idade--;
            }

            return idade;
        }
    }

    [EmailAddress(ErrorMessage = "E-mail invalido.")]
    [StringLength(120)]
    public string? Email { get; set; }

    public ICollection<Venda> Vendas { get; set; } = new List<Venda>();
    public ICollection<Divida> Dividas { get; set; } = new List<Divida>();
}
