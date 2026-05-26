PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS Clientes (
    Id INTEGER NOT NULL CONSTRAINT PK_Clientes PRIMARY KEY AUTOINCREMENT,
    Nome TEXT NOT NULL,
    Cpf TEXT NOT NULL CHECK (Cpf GLOB '[0-9][0-9][0-9].[0-9][0-9][0-9].[0-9][0-9][0-9]-[0-9][0-9]'),
    DataNascimento TEXT NOT NULL,
    Email TEXT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS IX_Clientes_Cpf ON Clientes (Cpf);

CREATE TABLE IF NOT EXISTS Dividas (
    Id INTEGER NOT NULL CONSTRAINT PK_Dividas PRIMARY KEY AUTOINCREMENT,
    ClienteId INTEGER NOT NULL,
    ValorOriginal TEXT NOT NULL,
    ValorAtual TEXT NOT NULL,
    TotalJurosAplicados TEXT NOT NULL,
    Paga INTEGER NOT NULL,
    DataCriacao TEXT NOT NULL,
    DataVencimento TEXT NULL,
    DataPagamento TEXT NULL,
    NumeroParcelas INTEGER NOT NULL,
    Observacoes TEXT NULL,
    CONSTRAINT FK_Dividas_Clientes_ClienteId FOREIGN KEY (ClienteId) REFERENCES Clientes (Id) ON DELETE CASCADE
);

CREATE UNIQUE INDEX IF NOT EXISTS IX_Dividas_ClienteId_Aberta ON Dividas (ClienteId) WHERE Paga = 0;

CREATE TABLE IF NOT EXISTS Vendas (
    Id INTEGER NOT NULL CONSTRAINT PK_Vendas PRIMARY KEY AUTOINCREMENT,
    ClienteId INTEGER NOT NULL,
    Descricao TEXT NOT NULL,
    ValorTotal TEXT NOT NULL,
    DataVenda TEXT NOT NULL,
    Pendurada INTEGER NOT NULL,
    DividaId INTEGER NULL,
    CONSTRAINT FK_Vendas_Clientes_ClienteId FOREIGN KEY (ClienteId) REFERENCES Clientes (Id) ON DELETE CASCADE,
    CONSTRAINT FK_Vendas_Dividas_DividaId FOREIGN KEY (DividaId) REFERENCES Dividas (Id) ON DELETE SET NULL
);

CREATE INDEX IF NOT EXISTS IX_Vendas_ClienteId ON Vendas (ClienteId);
CREATE INDEX IF NOT EXISTS IX_Vendas_DividaId ON Vendas (DividaId);

CREATE TABLE IF NOT EXISTS HistoricoJuros (
    Id INTEGER NOT NULL CONSTRAINT PK_HistoricoJuros PRIMARY KEY AUTOINCREMENT,
    DividaId INTEGER NOT NULL,
    PercentualAplicado TEXT NOT NULL,
    ValorJuros TEXT NOT NULL,
    ValorAntes TEXT NOT NULL,
    ValorDepois TEXT NOT NULL,
    AplicadoEm TEXT NOT NULL,
    Motivo TEXT NULL,
    CONSTRAINT FK_HistoricoJuros_Dividas_DividaId FOREIGN KEY (DividaId) REFERENCES Dividas (Id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS IX_HistoricoJuros_DividaId ON HistoricoJuros (DividaId);
