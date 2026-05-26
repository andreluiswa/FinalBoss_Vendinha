using System.Globalization;
using FinalBoss_Vendinha.Data;
using FinalBoss_Vendinha.Models;
using FinalBoss_Vendinha.Services;

var cultura = new CultureInfo("pt-BR");
CultureInfo.DefaultThreadCurrentCulture = cultura;
CultureInfo.DefaultThreadCurrentUICulture = cultura;

var dbPath = Path.Combine(AppContext.BaseDirectory, "vendinha.db");
var connectionString = $"Data Source={dbPath}";

using (var db = new VendinhaDbContext(connectionString))
{
    db.Database.EnsureCreated();
}

var clienteService = new ClienteService(connectionString);
var vendaService = new VendaService(connectionString);
var dividaService = new DividaService(connectionString);

while (true)
{
    LimparTela();
    Titulo("Home");
    Console.WriteLine("1 - Registrar novo cliente");
    Console.WriteLine("2 - Clientes da vendinha");
    Console.WriteLine("3 - Vendas feitas");
    Console.WriteLine("4 - Dividas existentes");
    Console.WriteLine("0 - Sair");

    switch (LerOpcao("Escolha: "))
    {
        case "1":
            TelaRegistrarCliente();
            break;
        case "2":
            TelaClientes();
            break;
        case "3":
            TelaVendas();
            break;
        case "4":
            TelaDividas();
            break;
        case "0":
            return;
        default:
            Aviso("Opcao invalida.");
            break;
    }
}

void TelaRegistrarCliente()
{
    LimparTela();
    Titulo("Registrar novo cliente");

    try
    {
        var nome = LerTexto("Nome completo: ", obrigatorio: true);
        var cpf = LerTexto("CPF: ", obrigatorio: true);
        var dataNascimento = LerData("Data de nascimento (dd/mm/aaaa): ", obrigatorio: true)!.Value;
        var email = LerTexto("E-mail (opcional): ", obrigatorio: false);

        var cliente = clienteService.Criar(nome, cpf, dataNascimento, email);
        Aviso($"Cliente #{cliente.Id} cadastrado com sucesso.");
    }
    catch (Exception ex)
    {
        Aviso(ex.Message);
    }
}

void TelaClientes()
{
    var busca = string.Empty;

    while (true)
    {
        LimparTela();
        Titulo("Clientes da vendinha");
        Console.WriteLine($"Busca atual: {(string.IsNullOrWhiteSpace(busca) ? "sem filtro" : busca)}");
        Console.WriteLine();

        var clientes = clienteService.Listar(busca);
        if (clientes.Count == 0)
        {
            Console.WriteLine("Nenhum cliente encontrado.");
        }
        else
        {
            foreach (var resumo in clientes)
            {
                var cliente = resumo.Cliente;
                Console.WriteLine($"#{cliente.Id} | {cliente.Nome} | CPF {cliente.Cpf} | {cliente.Idade} anos | Divida aberta: {Moeda(resumo.TotalDividasAbertas)}");
            }
        }

        Console.WriteLine();
        Console.WriteLine("1 - Buscar por nome");
        Console.WriteLine("2 - Ver detalhes de cliente");
        Console.WriteLine("3 - Editar cliente");
        Console.WriteLine("4 - Excluir cliente");
        Console.WriteLine("0 - Voltar para Home");

        switch (LerOpcao("Escolha: "))
        {
            case "1":
                busca = LerTexto("Texto da busca: ", obrigatorio: false);
                break;
            case "2":
                DetalharCliente();
                break;
            case "3":
                EditarCliente();
                break;
            case "4":
                ExcluirCliente();
                break;
            case "0":
                return;
            default:
                Aviso("Opcao invalida.");
                break;
        }
    }
}

void DetalharCliente()
{
    var id = LerInteiro("ID do cliente: ");
    var cliente = clienteService.BuscarPorId(id);
    if (cliente is null)
    {
        Aviso("Cliente nao encontrado.");
        return;
    }

    LimparTela();
    Titulo($"Detalhes do cliente #{cliente.Id}");
    Console.WriteLine($"Nome: {cliente.Nome}");
    Console.WriteLine($"CPF: {cliente.Cpf}");
    Console.WriteLine($"Nascimento: {cliente.DataNascimento:dd/MM/yyyy}");
    Console.WriteLine($"Idade: {cliente.Idade}");
    Console.WriteLine($"E-mail: {cliente.Email ?? "nao informado"}");
    Console.WriteLine();

    var dividaAberta = cliente.Dividas.FirstOrDefault(divida => !divida.Paga);
    if (dividaAberta is null)
    {
        Console.WriteLine("Cliente sem divida aberta.");
    }
    else
    {
        Console.WriteLine($"Divida aberta #{dividaAberta.Id}: {Moeda(dividaAberta.ValorAtual)} em {dividaAberta.NumeroParcelas} parcela(s).");
    }

    Pausar();
}

void EditarCliente()
{
    var id = LerInteiro("ID do cliente: ");
    var atual = clienteService.BuscarPorId(id);
    if (atual is null)
    {
        Aviso("Cliente nao encontrado.");
        return;
    }

    try
    {
        var nome = LerTexto($"Nome completo ({atual.Nome}): ", obrigatorio: false);
        var cpf = LerTexto($"CPF ({atual.Cpf}): ", obrigatorio: false);
        var data = LerData($"Nascimento ({atual.DataNascimento:dd/MM/yyyy}): ", obrigatorio: false);
        var email = LerTexto($"E-mail ({atual.Email ?? "vazio"}): ", obrigatorio: false);

        clienteService.Atualizar(
            id,
            string.IsNullOrWhiteSpace(nome) ? atual.Nome : nome,
            string.IsNullOrWhiteSpace(cpf) ? atual.Cpf : cpf,
            data ?? atual.DataNascimento,
            string.IsNullOrWhiteSpace(email) ? atual.Email : email);

        Aviso("Cliente atualizado.");
    }
    catch (Exception ex)
    {
        Aviso(ex.Message);
    }
}

void ExcluirCliente()
{
    var id = LerInteiro("ID do cliente: ");
    if (!Confirmar("Excluir cliente e seus registros relacionados?"))
    {
        return;
    }

    try
    {
        clienteService.Excluir(id);
        Aviso("Cliente excluido.");
    }
    catch (Exception ex)
    {
        Aviso(ex.Message);
    }
}

void TelaVendas()
{
    while (true)
    {
        LimparTela();
        Titulo("Vendas feitas");
        var vendas = vendaService.ListarTodas();
        if (vendas.Count == 0)
        {
            Console.WriteLine("Nenhuma venda registrada.");
        }
        else
        {
            foreach (var venda in vendas)
            {
                var status = venda.Pendurada ? $"Pendurada (divida #{venda.DividaId})" : "Paga na hora";
                Console.WriteLine($"#{venda.Id} | {venda.DataVenda:dd/MM/yyyy HH:mm} | {venda.Cliente?.Nome} | {Moeda(venda.ValorTotal)} | {status}");
            }
        }

        Console.WriteLine();
        Console.WriteLine("1 - Registrar venda");
        Console.WriteLine("2 - Ver detalhes da venda");
        Console.WriteLine("0 - Voltar para Home");

        switch (LerOpcao("Escolha: "))
        {
            case "1":
                RegistrarVenda();
                break;
            case "2":
                DetalharVenda();
                break;
            case "0":
                return;
            default:
                Aviso("Opcao invalida.");
                break;
        }
    }
}

void RegistrarVenda()
{
    try
    {
        LimparTela();
        Titulo("Registrar venda");
        var clienteId = LerInteiro("ID do cliente: ");
        var descricao = LerTexto("Descricao da compra: ", obrigatorio: true);
        var valor = LerDecimal("Valor total: ");
        var pendurar = Confirmar("Cliente vai pendurar esta compra?");
        DateTime? vencimento = null;
        var parcelas = 1;

        if (pendurar)
        {
            vencimento = LerData("Vencimento da divida (dd/mm/aaaa): ", obrigatorio: false);
            parcelas = LerInteiro("Quantidade de parcelas do total em aberto: ");
        }

        var venda = vendaService.Criar(clienteId, descricao, valor, pendurar, vencimento, parcelas);
        Aviso($"Venda #{venda.Id} registrada.");
    }
    catch (Exception ex)
    {
        Aviso(ex.Message);
    }
}

void DetalharVenda()
{
    var id = LerInteiro("ID da venda: ");
    var venda = vendaService.BuscarDetalhes(id);
    if (venda is null)
    {
        Aviso("Venda nao encontrada.");
        return;
    }

    LimparTela();
    Titulo($"Detalhes da venda #{venda.Id}");
    Console.WriteLine($"Cliente: {venda.Cliente?.Nome} (#{venda.ClienteId})");
    Console.WriteLine($"Descricao: {venda.Descricao}");
    Console.WriteLine($"Valor: {Moeda(venda.ValorTotal)}");
    Console.WriteLine($"Data: {venda.DataVenda:dd/MM/yyyy HH:mm}");
    Console.WriteLine($"Status: {(venda.Pendurada ? "Pendurada" : "Paga na hora")}");

    if (venda.DividaId.HasValue)
    {
        Console.WriteLine($"Divida relacionada: #{venda.DividaId} - {Moeda(venda.Divida?.ValorAtual ?? 0)}");
    }

    Pausar();
}

void TelaDividas(int? destacarId = null)
{
    while (true)
    {
        LimparTela();
        Titulo("Dividas existentes");
        var dividas = dividaService.ListarAbertas();
        if (dividas.Count == 0)
        {
            Console.WriteLine("Nenhuma divida aberta.");
        }
        else
        {
            foreach (var divida in dividas)
            {
                var destaque = divida.Id == destacarId ? ">> " : "   ";
                var atraso = divida.EmAtraso ? "EM ATRASO" : "em dia";
                Console.WriteLine($"{destaque}#{divida.Id} | {divida.Cliente?.Nome} | {Moeda(divida.ValorAtual)} | {divida.NumeroParcelas} parcela(s) | venc. {DataOuVazio(divida.DataVencimento)} | {atraso}");
            }
        }

        Console.WriteLine();
        Console.WriteLine("1 - Ver detalhes da divida");
        Console.WriteLine("2 - Quitar divida");
        Console.WriteLine("3 - Aplicar juros por atraso");
        Console.WriteLine("0 - Voltar para Home");

        switch (LerOpcao("Escolha: "))
        {
            case "1":
                DetalharDivida();
                break;
            case "2":
                QuitarDivida();
                break;
            case "3":
                AplicarJuros();
                break;
            case "0":
                return;
            default:
                Aviso("Opcao invalida.");
                break;
        }
    }
}

void DetalharDivida()
{
    var id = LerInteiro("ID da divida: ");
    var divida = dividaService.BuscarDetalhes(id);
    if (divida is null)
    {
        Aviso("Divida nao encontrada.");
        return;
    }

    LimparTela();
    Titulo($"Detalhes da divida #{divida.Id}");
    Console.WriteLine($"Cliente: {divida.Cliente?.Nome} (#{divida.ClienteId})");
    Console.WriteLine($"Valor original: {Moeda(divida.ValorOriginal)}");
    Console.WriteLine($"Juros aplicados: {Moeda(divida.TotalJurosAplicados)}");
    Console.WriteLine($"Valor atual: {Moeda(divida.ValorAtual)}");
    Console.WriteLine($"Parcelas: {divida.NumeroParcelas} x {Moeda(divida.ValorAtual / divida.NumeroParcelas)}");
    Console.WriteLine($"Criacao: {divida.DataCriacao:dd/MM/yyyy}");
    Console.WriteLine($"Vencimento: {DataOuVazio(divida.DataVencimento)}");
    Console.WriteLine($"Situacao: {(divida.Paga ? "paga" : divida.EmAtraso ? "em atraso" : "aberta")}");
    Console.WriteLine();
    Console.WriteLine("Vendas penduradas nesta divida:");
    foreach (var venda in divida.Vendas)
    {
        Console.WriteLine($"- Venda #{venda.Id}: {venda.Descricao} | {Moeda(venda.ValorTotal)} | {venda.DataVenda:dd/MM/yyyy}");
    }

    if (divida.HistoricoJuros.Count > 0)
    {
        Console.WriteLine();
        Console.WriteLine("Historico de juros:");
        foreach (var juros in divida.HistoricoJuros)
        {
            Console.WriteLine($"- {juros.AplicadoEm:dd/MM/yyyy HH:mm}: {juros.PercentualAplicado:N2}% = {Moeda(juros.ValorJuros)} ({juros.Motivo})");
        }
    }

    Pausar();
}

void QuitarDivida()
{
    var id = LerInteiro("ID da divida: ");
    if (!Confirmar("Confirmar pagamento e quitar divida?"))
    {
        return;
    }

    try
    {
        dividaService.Quitar(id);
        Aviso("Divida quitada.");
    }
    catch (Exception ex)
    {
        Aviso(ex.Message);
    }
}

void AplicarJuros()
{
    try
    {
        var id = LerInteiro("ID da divida: ");
        var percentual = LerDecimal("Percentual de juros: ");
        var motivo = LerTexto("Motivo (opcional): ", obrigatorio: false);
        var juros = dividaService.AplicarJuros(id, percentual, motivo);
        Aviso($"Juros aplicados: {Moeda(juros.ValorJuros)}. Novo valor: {Moeda(juros.ValorDepois)}.");
    }
    catch (Exception ex)
    {
        Aviso(ex.Message);
    }
}

static void Titulo(string texto)
{
    Console.WriteLine(texto);
    Console.WriteLine(new string('-', texto.Length));
}

static void LimparTela()
{
    if (Console.IsOutputRedirected)
    {
        return;
    }

    Console.Clear();
}

static string LerOpcao(string rotulo)
{
    Console.Write(rotulo);
    return Console.ReadLine()?.Trim() ?? string.Empty;
}

static string LerTexto(string rotulo, bool obrigatorio)
{
    while (true)
    {
        Console.Write(rotulo);
        var texto = Console.ReadLine()?.Trim() ?? string.Empty;
        if (!obrigatorio || !string.IsNullOrWhiteSpace(texto))
        {
            return texto;
        }

        Console.WriteLine("Campo obrigatorio.");
    }
}

static int LerInteiro(string rotulo)
{
    while (true)
    {
        Console.Write(rotulo);
        if (int.TryParse(Console.ReadLine(), out var valor) && valor > 0)
        {
            return valor;
        }

        Console.WriteLine("Informe um numero inteiro maior que zero.");
    }
}

static decimal LerDecimal(string rotulo)
{
    while (true)
    {
        Console.Write(rotulo);
        var entrada = Console.ReadLine();
        if (decimal.TryParse(entrada, NumberStyles.Number, CultureInfo.CurrentCulture, out var valor) && valor > 0)
        {
            return valor;
        }

        Console.WriteLine("Informe um valor maior que zero.");
    }
}

static DateTime? LerData(string rotulo, bool obrigatorio)
{
    while (true)
    {
        Console.Write(rotulo);
        var entrada = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(entrada) && !obrigatorio)
        {
            return null;
        }

        if (DateTime.TryParseExact(entrada, "dd/MM/yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out var data))
        {
            return data;
        }

        Console.WriteLine("Informe uma data no formato dd/mm/aaaa.");
    }
}

static bool Confirmar(string pergunta)
{
    Console.Write($"{pergunta} (s/n): ");
    return string.Equals(Console.ReadLine()?.Trim(), "s", StringComparison.OrdinalIgnoreCase);
}

static string Moeda(decimal valor)
{
    return valor.ToString("C", CultureInfo.CurrentCulture);
}

static string DataOuVazio(DateTime? data)
{
    return data.HasValue ? data.Value.ToString("dd/MM/yyyy") : "sem vencimento";
}

static void Aviso(string mensagem)
{
    Console.WriteLine();
    Console.WriteLine(mensagem);
    Pausar();
}

static void Pausar()
{
    Console.WriteLine();
    Console.Write("Pressione Enter para continuar...");
    Console.ReadLine();
}
