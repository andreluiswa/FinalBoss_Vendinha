# Documentacao - Vendinha Plena

Este arquivo explica o funcionamento do projeto Vendinha Plena. Ele foi feito para ajudar no estudo do codigo, das telas, das regras de negocio e do banco de dados.

## 1. Objetivo do projeto

O sistema simula uma vendinha que precisa controlar clientes, vendas e dividas penduradas.

A ideia principal e permitir que o dono da vendinha:

- cadastre clientes;
- consulte clientes cadastrados;
- registre vendas;
- marque vendas como pagas na hora ou penduradas;
- consulte dividas abertas;
- quite dividas;
- aplique juros quando houver atraso.

O projeto foi desenvolvido em C# com .NET e usa SQLite como banco de dados local.

## 2. Estrutura geral do projeto

O projeto principal fica na pasta:

```text
FinalBoss_Vendinha/
```

Arquivos e pastas principais:

```text
Program.cs
Data/VendinhaDbContext.cs
Models/Cliente.cs
Models/Venda.cs
Models/Divida.cs
Services/ClienteService.cs
Services/VendaService.cs
Services/DividaService.cs
Services/ClienteResumo.cs
schema.sql
README.md
DOC.md
```

### Program.cs

E o arquivo principal da aplicacao. Ele contem as telas de console e controla o fluxo de navegacao.

Nele ficam:

- menu Home;
- tela de cadastro de cliente;
- tela de listagem de clientes;
- tela de vendas;
- tela de dividas;
- funcoes auxiliares para ler texto, numeros, datas e valores monetarios.

### Data/VendinhaDbContext.cs

E o arquivo que conecta o sistema ao banco SQLite usando Entity Framework Core.

Ele define as tabelas:

- Clientes;
- Vendas;
- Dividas;
- HistoricoJuros.

Tambem define relacionamentos e regras importantes, como CPF unico e apenas uma divida aberta por cliente.

### Models

Os Models representam as entidades do sistema.

Exemplo:

- `Cliente` representa um cliente;
- `Venda` representa uma venda;
- `Divida` representa uma divida;
- `HistoricoJuros` representa os juros aplicados em uma divida.

### Services

Os Services concentram as regras de negocio e as operacoes com o banco.

Exemplo:

- `ClienteService` cadastra, edita, exclui e lista clientes;
- `VendaService` registra e lista vendas;
- `DividaService` lista, quita e aplica juros em dividas.

## 3. Tela Home

A Home e a primeira tela exibida quando o sistema inicia.

Menu:

```text
1 - Registrar novo cliente
2 - Clientes da vendinha
3 - Vendas feitas
4 - Dividas existentes
0 - Sair
```

### Opcao 1 - Registrar novo cliente

Abre a tela para cadastrar um novo cliente.

### Opcao 2 - Clientes da vendinha

Abre a tela que lista todos os clientes cadastrados.

### Opcao 3 - Vendas feitas

Abre a tela de vendas.

### Opcao 4 - Dividas existentes

Abre a tela de dividas abertas.

### Opcao 0 - Sair

Encerra a aplicacao.

## 4. Tela Registrar Novo Cliente

Essa tela solicita os dados de um cliente.

Campos:

```text
Nome completo
CPF
Data de nascimento
E-mail
```

### Nome completo

Campo obrigatorio. Deve ter no minimo 5 caracteres e no maximo 100.

A regra esta no arquivo `Models/Cliente.cs`:

```csharp
[Required(ErrorMessage = "O nome do cliente e obrigatorio.")]
[StringLength(100, MinimumLength = 5, ErrorMessage = "Informe o nome completo.")]
public string Nome { get; set; } = string.Empty;
```

### CPF

Campo obrigatorio. Deve estar no formato:

```text
000.000.000-00
```

Exemplo valido:

```text
123.456.789-10
```

A validacao fica diretamente no model `Cliente`:

```csharp
[RegularExpression(@"^\d{3}\.\d{3}\.\d{3}-\d{2}$", ErrorMessage = "O CPF deve estar no formato 000.000.000-00.")]
public string Cpf { get; set; } = string.Empty;
```

O sistema tambem bloqueia CPF duplicado. Essa regra fica no `ClienteService`.

### Data de nascimento

Campo obrigatorio. Deve ser informada no formato:

```text
dd/mm/aaaa
```

Exemplo:

```text
15/08/1999
```

O sistema calcula a idade automaticamente no model `Cliente`, por meio da propriedade `Idade`.

### E-mail

Campo opcional. Se for informado, precisa estar em formato valido de e-mail.

Exemplo:

```text
cliente@email.com
```

## 5. Tela Clientes da Vendinha

Essa tela mostra todos os clientes cadastrados.

Ela exibe:

```text
ID
Nome
CPF
Idade
Valor da divida aberta
```

Exemplo:

```text
#1 | Zoio Bazinga | CPF 123.456.789-10 | 26 anos | Divida aberta: R$ 150,00
```

Menu:

```text
1 - Buscar por nome
2 - Ver detalhes de cliente
3 - Editar cliente
4 - Excluir cliente
0 - Voltar para Home
```

### Opcao 1 - Buscar por nome

Permite filtrar clientes por parte do nome.

Exemplo:

```text
Texto da busca: maria
```

O sistema procura clientes que contenham esse texto no nome.

### Opcao 2 - Ver detalhes de cliente

Solicita o ID do cliente e mostra informacoes mais completas:

- nome;
- CPF;
- data de nascimento;
- idade;
- e-mail;
- divida aberta, caso exista.

Essa tela apenas mostra os dados. Ela nao leva diretamente para outra tela.

### Opcao 3 - Editar cliente

Permite alterar:

- nome;
- CPF;
- data de nascimento;
- e-mail.

Se o usuario deixar um campo vazio, o sistema mantem o valor antigo.

### Opcao 4 - Excluir cliente

Remove o cliente do banco de dados.

Como as vendas e dividas pertencem ao cliente, elas tambem sao removidas por causa da regra de relacionamento com `Cascade`.

### Opcao 0 - Voltar para Home

Retorna para a tela inicial.

## 6. Tela Vendas Feitas

Essa tela lista as vendas registradas.

Ela exibe:

```text
ID da venda
Data
Nome do cliente
Valor
Status
```

Exemplo:

```text
#1 | 22/06/2026 19:30 | Zoio Bazinga | R$ 50,00 | Pendurada (divida #1)
```

Menu:

```text
1 - Registrar venda
2 - Ver detalhes da venda
0 - Voltar para Home
```

### Opcao 1 - Registrar venda

Cadastra uma nova venda.

Campos solicitados:

```text
ID do cliente
Descricao da compra
Valor total
Se a compra sera pendurada
Vencimento da divida, caso seja pendurada
Quantidade de parcelas, caso seja pendurada
```

Se a venda for paga na hora, ela nao gera divida.

Se a venda for pendurada, ela entra na divida aberta do cliente.

### Opcao 2 - Ver detalhes da venda

Mostra:

- cliente;
- descricao;
- valor;
- data;
- status;
- ID da divida relacionada, se existir.

Essa tela apenas mostra os dados. Ela nao redireciona para a tela de dividas.

### Opcao 0 - Voltar para Home

Retorna para a Home.

## 7. Tela Dividas Existentes

Essa tela lista somente as dividas abertas.

Ela exibe:

```text
ID da divida
Nome do cliente
Valor atual
Numero de parcelas
Data de vencimento
Situacao
```

Exemplo:

```text
#1 | Zoio Bazinga | R$ 150,00 | 3 parcela(s) | venc. 30/06/2026 | em dia
```

Menu:

```text
1 - Ver detalhes da divida
2 - Quitar divida
3 - Aplicar juros por atraso
0 - Voltar para Home
```

### Opcao 1 - Ver detalhes da divida

Mostra:

- cliente;
- valor original;
- juros aplicados;
- valor atual;
- quantidade de parcelas;
- data de criacao;
- vencimento;
- situacao;
- vendas penduradas naquela divida;
- historico de juros, se existir.

### Opcao 2 - Quitar divida

Marca a divida como paga.

Quando uma divida e quitada:

- `Paga` passa para `true`;
- `DataPagamento` recebe a data atual;
- a divida deixa de aparecer na listagem de dividas abertas.

### Opcao 3 - Aplicar juros por atraso

Permite aplicar juros em uma divida atrasada.

O sistema so permite aplicar juros se:

- a divida nao estiver paga;
- a data atual for maior que a data de vencimento.

Exemplo:

```text
Percentual de juros: 10
```

Se a divida for de R$ 100,00, o sistema soma R$ 10,00 e o novo valor passa a ser R$ 110,00.

O sistema tambem grava um registro em `HistoricoJuros`.

### Opcao 0 - Voltar para Home

Retorna para a tela inicial.

## 8. Regras de negocio importantes

### CPF unico

Nao pode existir mais de um cliente com o mesmo CPF.

Essa regra aparece em dois lugares:

1. No banco de dados:

```csharp
modelBuilder.Entity<Cliente>()
    .HasIndex(cliente => cliente.Cpf)
    .IsUnique();
```

2. No service:

```csharp
if (db.Clientes.Any(cliente => cliente.Cpf == cpf))
{
    throw new InvalidOperationException("Ja existe um cliente cadastrado com este CPF.");
}
```

### Uma divida aberta por cliente

Cada cliente pode ter varias vendas penduradas, mas apenas uma divida aberta.

Exemplo:

- venda 1 pendurada: R$ 50,00;
- venda 2 pendurada: R$ 80,00;
- venda 3 pendurada: R$ 20,00.

O sistema nao cria tres dividas diferentes. Ele soma tudo na divida aberta do cliente:

```text
Divida total = R$ 150,00
```

Essa regra fica no `VendaService`.

Quando uma venda pendurada e criada, o sistema procura uma divida aberta do cliente:

```csharp
var divida = db.Dividas
    .Include(item => item.Vendas)
    .FirstOrDefault(item => item.ClienteId == clienteId && !item.Paga);
```

Se nao existir, cria uma nova.

Se existir, soma o valor da venda na divida existente.

### Quitacao da divida

Quando a divida e quitada, ela nao e apagada do banco. Ela apenas deixa de estar aberta.

Isso e importante porque o historico continua existindo.

### Juros

O juros e aplicado sobre o valor atual da divida.

Formula:

```text
valorJuros = valorAtual * (percentual / 100)
```

Depois o sistema atualiza:

```text
ValorAtual = ValorAtual + valorJuros
TotalJurosAplicados = TotalJurosAplicados + valorJuros
```

## 9. Banco de dados

O banco usado e SQLite.

O arquivo do banco e criado automaticamente na pasta de saida da aplicacao:

```text
bin/Debug/net10.0/vendinha.db
```

No codigo, o caminho e montado em `Program.cs`:

```csharp
var dbPath = Path.Combine(AppContext.BaseDirectory, "vendinha.db");
var connectionString = $"Data Source={dbPath}";
```

Depois o sistema cria o banco automaticamente:

```csharp
using (var db = new VendinhaDbContext(connectionString))
{
    db.Database.EnsureCreated();
}
```

## 10. Tabelas do banco

### Clientes

Guarda os clientes cadastrados.

Campos principais:

```text
Id
Nome
Cpf
DataNascimento
Email
```

### Vendas

Guarda as vendas feitas.

Campos principais:

```text
Id
ClienteId
Descricao
ValorTotal
DataVenda
Pendurada
DividaId
```

Se `Pendurada` for falso, a venda foi paga na hora.

Se `Pendurada` for verdadeiro, a venda esta relacionada a uma divida.

### Dividas

Guarda as dividas dos clientes.

Campos principais:

```text
Id
ClienteId
ValorOriginal
ValorAtual
TotalJurosAplicados
Paga
DataCriacao
DataVencimento
DataPagamento
NumeroParcelas
Observacoes
```

### HistoricoJuros

Guarda cada aplicacao de juros.

Campos principais:

```text
Id
DividaId
PercentualAplicado
ValorJuros
ValorAntes
ValorDepois
AplicadoEm
Motivo
```

## 11. Relacionamentos

### Cliente e Venda

Um cliente pode ter varias vendas.

No model:

```csharp
public ICollection<Venda> Vendas { get; set; } = new List<Venda>();
```

### Cliente e Divida

Um cliente pode ter varias dividas no historico, mas apenas uma aberta por vez.

No banco:

```csharp
modelBuilder.Entity<Divida>()
    .HasIndex(divida => divida.ClienteId)
    .IsUnique()
    .HasFilter("Paga = 0");
```

Essa regra cria um indice unico apenas para dividas abertas.

### Divida e Venda

Uma divida pode ter varias vendas penduradas.

No model `Divida`:

```csharp
public ICollection<Venda> Vendas { get; set; } = new List<Venda>();
```

No model `Venda`:

```csharp
public int? DividaId { get; set; }
public Divida? Divida { get; set; }
```

## 12. Comandos uteis

### Compilar o projeto

```powershell
dotnet build
```

### Executar o projeto principal

```powershell
dotnet run --project .\FinalBoss_Vendinha\FinalBoss_Vendinha.csproj
```

### Abrir no Visual Studio

Abra o arquivo:

```text
FinalBoss_Vendinha.slnx
```

Depois defina `FinalBoss_Vendinha` como projeto de inicializacao e pressione:

```text
Ctrl + F5
```

## 13. Fluxo de exemplo para testar

1. Abra o sistema.
2. Escolha `1 - Registrar novo cliente`.
3. Cadastre um cliente com CPF no formato `587.125.945-60`.
4. Volte para a Home.
5. Escolha `3 - Vendas feitas`.
6. Escolha `1 - Registrar venda`.
7. Informe o ID do cliente.
8. Marque que a compra sera pendurada.
9. Informe vencimento e parcelas.
10. Volte para a Home.
11. Escolha `4 - Dividas existentes`.
12. Veja a divida criada.
13. Se estiver atrasada, aplique juros.
14. Quando o cliente pagar, use `2 - Quitar divida`.

## 14. Resumo final

O projeto esta dividido em camadas simples:

```text
Program.cs -> telas e interacao com usuario
Models -> estrutura dos dados
Services -> regras de negocio
Data -> configuracao do banco
SQLite -> persistencia dos dados
```

Essa organizacao ajuda a separar responsabilidades:

- a tela nao acessa o banco diretamente;
- o service concentra as regras;
- o model representa os dados;
- o DbContext configura o banco.

Com isso, o codigo fica mais facil de entender, manter e explicar em apresentacao.
