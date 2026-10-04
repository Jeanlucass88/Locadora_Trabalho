IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920174019_InitialCreate'
)
BEGIN
    CREATE TABLE [CategoriasVeiculo] (
        [Id] int NOT NULL IDENTITY,
        [Nome] nvarchar(50) NOT NULL,
        [Descricao] nvarchar(250) NULL,
        CONSTRAINT [PK_CategoriasVeiculo] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920174019_InitialCreate'
)
BEGIN
    CREATE TABLE [Clientes] (
        [Id] int NOT NULL IDENTITY,
        [Nome] nvarchar(150) NOT NULL,
        [Cpf] char(11) NOT NULL,
        [Email] nvarchar(254) NOT NULL,
        [Telefone] varchar(20) NULL,
        [DataNascimento] date NOT NULL,
        [NumeroCnh] varchar(11) NOT NULL,
        [ValidadeCnh] date NOT NULL,
        [Ativo] bit NOT NULL DEFAULT CAST(1 AS bit),
        CONSTRAINT [PK_Clientes] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920174019_InitialCreate'
)
BEGIN
    CREATE TABLE [Fabricantes] (
        [Id] int NOT NULL IDENTITY,
        [Nome] nvarchar(100) NOT NULL,
        [PaisOrigem] nvarchar(80) NULL,
        CONSTRAINT [PK_Fabricantes] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920174019_InitialCreate'
)
BEGIN
    CREATE TABLE [Veiculos] (
        [Id] int NOT NULL IDENTITY,
        [FabricanteId] int NOT NULL,
        [CategoriaVeiculoId] int NOT NULL,
        [Modelo] nvarchar(100) NOT NULL,
        [Placa] varchar(8) NOT NULL,
        [Chassi] varchar(17) NOT NULL,
        [AnoFabricacao] int NOT NULL,
        [Quilometragem] int NOT NULL,
        [Ativo] bit NOT NULL DEFAULT CAST(1 AS bit),
        CONSTRAINT [PK_Veiculos] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Veiculos_AnoFabricacao] CHECK ([AnoFabricacao] BETWEEN 1886 AND 2100),
        CONSTRAINT [CK_Veiculos_Quilometragem] CHECK ([Quilometragem] >= 0),
        CONSTRAINT [FK_Veiculos_CategoriasVeiculo_CategoriaVeiculoId] FOREIGN KEY ([CategoriaVeiculoId]) REFERENCES [CategoriasVeiculo] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Veiculos_Fabricantes_FabricanteId] FOREIGN KEY ([FabricanteId]) REFERENCES [Fabricantes] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920174019_InitialCreate'
)
BEGIN
    CREATE TABLE [Alugueis] (
        [Id] int NOT NULL IDENTITY,
        [ClienteId] int NOT NULL,
        [VeiculoId] int NOT NULL,
        [DataRetirada] datetime2 NOT NULL,
        [DataPrevistaDevolucao] datetime2 NOT NULL,
        [DataDevolucao] datetime2 NULL,
        [QuilometragemInicial] int NOT NULL,
        [QuilometragemFinal] int NULL,
        [ValorDiaria] decimal(10,2) NOT NULL,
        [ValorTotal] decimal(12,2) NULL,
        CONSTRAINT [PK_Alugueis] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Alugueis_DataDevolucao] CHECK ([DataDevolucao] IS NULL OR [DataDevolucao] >= [DataRetirada]),
        CONSTRAINT [CK_Alugueis_DevolucaoCompleta] CHECK (([DataDevolucao] IS NULL AND [QuilometragemFinal] IS NULL AND [ValorTotal] IS NULL) OR ([DataDevolucao] IS NOT NULL AND [QuilometragemFinal] IS NOT NULL AND [ValorTotal] IS NOT NULL)),
        CONSTRAINT [CK_Alugueis_PeriodoPrevisto] CHECK ([DataPrevistaDevolucao] > [DataRetirada]),
        CONSTRAINT [CK_Alugueis_QuilometragemFinal] CHECK ([QuilometragemFinal] IS NULL OR [QuilometragemFinal] >= [QuilometragemInicial]),
        CONSTRAINT [CK_Alugueis_QuilometragemInicial] CHECK ([QuilometragemInicial] >= 0),
        CONSTRAINT [CK_Alugueis_ValorDiaria] CHECK ([ValorDiaria] > 0),
        CONSTRAINT [CK_Alugueis_ValorTotal] CHECK ([ValorTotal] IS NULL OR [ValorTotal] >= 0),
        CONSTRAINT [FK_Alugueis_Clientes_ClienteId] FOREIGN KEY ([ClienteId]) REFERENCES [Clientes] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Alugueis_Veiculos_VeiculoId] FOREIGN KEY ([VeiculoId]) REFERENCES [Veiculos] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920174019_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Alugueis_ClienteId_DataRetirada] ON [Alugueis] ([ClienteId], [DataRetirada]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920174019_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Alugueis_VeiculoId_DataRetirada_DataPrevistaDevolucao] ON [Alugueis] ([VeiculoId], [DataRetirada], [DataPrevistaDevolucao]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920174019_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CategoriasVeiculo_Nome] ON [CategoriasVeiculo] ([Nome]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920174019_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Clientes_Cpf] ON [Clientes] ([Cpf]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920174019_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Clientes_Email] ON [Clientes] ([Email]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920174019_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Clientes_NumeroCnh] ON [Clientes] ([NumeroCnh]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920174019_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Fabricantes_Nome] ON [Fabricantes] ([Nome]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920174019_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Veiculos_CategoriaVeiculoId] ON [Veiculos] ([CategoriaVeiculoId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920174019_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Veiculos_Chassi] ON [Veiculos] ([Chassi]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920174019_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Veiculos_FabricanteId] ON [Veiculos] ([FabricanteId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920174019_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Veiculos_Placa] ON [Veiculos] ([Placa]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920174019_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260920174019_InitialCreate', N'8.0.24');
END;
GO

COMMIT;
GO

