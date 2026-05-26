# Vendinha Plena

Aplicacao de console em C#/.NET para controle de clientes, vendas e dividas penduradas de uma vendinha.

## Funcionalidades

- Cadastro, listagem, edicao e exclusao de clientes.
- Validacao de CPF diretamente no model `Cliente`, no formato `000.000.000-00`, e bloqueio de CPF duplicado.
- Idade calculada automaticamente pela data de nascimento.
- Listagem de clientes com busca por nome e ordenacao por maior divida.
- Cadastro de vendas pagas na hora ou penduradas.
- Uma unica divida aberta por cliente, somando varias vendas penduradas.
- Detalhe de clientes e vendas com atalho para a divida relacionada.
- Tela de dividas com detalhes, quitacao e aplicacao de juros quando houver atraso.
- Persistencia em SQLite local.

## Como executar

1. Abra `FinalBoss_Vendinha.slnx` no Visual Studio 2026 ou rode pelo terminal.
2. Execute:

```powershell
dotnet run --project .\FinalBoss_Vendinha\FinalBoss_Vendinha.csproj
```

O banco SQLite `vendinha.db` sera criado automaticamente na pasta de saida da aplicacao.

Ao cadastrar ou editar clientes, informe o CPF ja formatado, por exemplo: `587.125.945-60`.

## Banco de dados

O projeto usa Entity Framework Core com SQLite. O arquivo `schema.sql` contem o script de criacao do banco para consulta, DBeaver ou criacao manual.

No DBeaver, crie uma conexao SQLite apontando para o arquivo `vendinha.db` gerado apos a primeira execucao.

## Regra de divida

Cada cliente pode ter varias vendas penduradas, mas apenas uma divida aberta. Quando uma nova venda e pendurada, o sistema soma o valor ao total da divida aberta do cliente. Ao quitar, a divida fica fechada e uma nova divida podera ser criada em compras futuras.
