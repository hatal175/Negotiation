
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 05/17/2015 21:47:26
-- Generated from EDMX file: C:\Users\Inbal\documents\visual studio 2013\Projects\Negotiation\Negotiation\Models\Negotiation.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [NegotiationDb];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_NegotiationActionUserId]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[NegotiationActionSet] DROP CONSTRAINT [FK_NegotiationActionUserId];
GO
IF OBJECT_ID(N'[dbo].[FK_UserUserRole]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[UserSet] DROP CONSTRAINT [FK_UserUserRole];
GO
IF OBJECT_ID(N'[dbo].[FK_GameUser]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[UserSet] DROP CONSTRAINT [FK_GameUser];
GO
IF OBJECT_ID(N'[dbo].[FK_GameNegotiationAction]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[NegotiationActionSet] DROP CONSTRAINT [FK_GameNegotiationAction];
GO
IF OBJECT_ID(N'[dbo].[FK_GameGameDomain]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[GameSet] DROP CONSTRAINT [FK_GameGameDomain];
GO
IF OBJECT_ID(N'[dbo].[FK_UserStrategy]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[UserSet] DROP CONSTRAINT [FK_UserStrategy];
GO
IF OBJECT_ID(N'[dbo].[FK_GameDomainDomainVariant]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[DomainVariantSet] DROP CONSTRAINT [FK_GameDomainDomainVariant];
GO
IF OBJECT_ID(N'[dbo].[FK_GameDomainConfigGameDomain]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[GameDomainConfigSet] DROP CONSTRAINT [FK_GameDomainConfigGameDomain];
GO
IF OBJECT_ID(N'[dbo].[FK_UserUserData]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[UserDataSet] DROP CONSTRAINT [FK_UserUserData];
GO
IF OBJECT_ID(N'[dbo].[FK_StrategyConfigStrategy]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[StrategyConfigSet] DROP CONSTRAINT [FK_StrategyConfigStrategy];
GO
IF OBJECT_ID(N'[dbo].[FK_GameDomainConfigDomainVariantAi]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[GameDomainConfigSet] DROP CONSTRAINT [FK_GameDomainConfigDomainVariantAi];
GO
IF OBJECT_ID(N'[dbo].[FK_GameDomainConfigDomainVariantHuman]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[GameDomainConfigSet] DROP CONSTRAINT [FK_GameDomainConfigDomainVariantHuman];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[NegotiationActionSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[NegotiationActionSet];
GO
IF OBJECT_ID(N'[dbo].[UserSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[UserSet];
GO
IF OBJECT_ID(N'[dbo].[UserRoleSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[UserRoleSet];
GO
IF OBJECT_ID(N'[dbo].[GameSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[GameSet];
GO
IF OBJECT_ID(N'[dbo].[GameDomainSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[GameDomainSet];
GO
IF OBJECT_ID(N'[dbo].[StrategySet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[StrategySet];
GO
IF OBJECT_ID(N'[dbo].[DomainVariantSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[DomainVariantSet];
GO
IF OBJECT_ID(N'[dbo].[GameDomainConfigSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[GameDomainConfigSet];
GO
IF OBJECT_ID(N'[dbo].[UserDataSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[UserDataSet];
GO
IF OBJECT_ID(N'[dbo].[StrategyConfigSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[StrategyConfigSet];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'NegotiationActionSet'
CREATE TABLE [dbo].[NegotiationActionSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Type] tinyint  NOT NULL,
    [RemainingTime] time  NOT NULL,
    [Value] nvarchar(max)  NOT NULL,
    [GameId] nvarchar(255)  NOT NULL,
    [UserId] nvarchar(255)  NOT NULL
);
GO

-- Creating table 'UserSet'
CREATE TABLE [dbo].[UserSet] (
    [Id] nvarchar(255)  NOT NULL,
    [Type] tinyint  NOT NULL,
    [GameId] nvarchar(255)  NOT NULL,
    [StrategyId] int  NULL,
    [Score] int  NULL,
    [UserRole_Id] int  NOT NULL
);
GO

-- Creating table 'UserRoleSet'
CREATE TABLE [dbo].[UserRoleSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Description] nvarchar(max)  NOT NULL,
    [Variant] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'GameSet'
CREATE TABLE [dbo].[GameSet] (
    [Id] nvarchar(255)  NOT NULL,
    [GameDomainId] int  NOT NULL,
    [StartTime] datetime  NOT NULL,
    [Endtime] datetime  NULL
);
GO

-- Creating table 'GameDomainSet'
CREATE TABLE [dbo].[GameDomainSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [DomainXML] nvarchar(max)  NOT NULL,
    [Name] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'StrategySet'
CREATE TABLE [dbo].[StrategySet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [DllPath] nvarchar(max)  NOT NULL,
    [StrategyName] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'DomainVariantSet'
CREATE TABLE [dbo].[DomainVariantSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [VariantXML] nvarchar(max)  NOT NULL,
    [GameDomainId] int  NOT NULL,
    [Name] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'GameDomainConfigSet'
CREATE TABLE [dbo].[GameDomainConfigSet] (
    [Id] int  NOT NULL,
    [AiSide] nvarchar(max)  NOT NULL,
    [AiVariant] nvarchar(max)  NOT NULL,
    [HumanSide] nvarchar(max)  NOT NULL,
    [HumanVariant] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'UserDataSet'
CREATE TABLE [dbo].[UserDataSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Gender] tinyint  NOT NULL,
    [AgeRange] tinyint  NOT NULL,
    [Education] tinyint  NOT NULL,
    [DegreeField] nvarchar(max)  NULL,
    [Country] nvarchar(max)  NOT NULL,
    [Name] nvarchar(max)  NOT NULL,
    [StudentId] bigint  NOT NULL,
    [University] nvarchar(max)  NOT NULL,
    [UserUserData_UserData_Id] nvarchar(255)  NOT NULL
);
GO

-- Creating table 'StrategyConfigSet'
CREATE TABLE [dbo].[StrategyConfigSet] (
    [Id] int  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id] in table 'NegotiationActionSet'
ALTER TABLE [dbo].[NegotiationActionSet]
ADD CONSTRAINT [PK_NegotiationActionSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'UserSet'
ALTER TABLE [dbo].[UserSet]
ADD CONSTRAINT [PK_UserSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'UserRoleSet'
ALTER TABLE [dbo].[UserRoleSet]
ADD CONSTRAINT [PK_UserRoleSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'GameSet'
ALTER TABLE [dbo].[GameSet]
ADD CONSTRAINT [PK_GameSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'GameDomainSet'
ALTER TABLE [dbo].[GameDomainSet]
ADD CONSTRAINT [PK_GameDomainSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'StrategySet'
ALTER TABLE [dbo].[StrategySet]
ADD CONSTRAINT [PK_StrategySet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'DomainVariantSet'
ALTER TABLE [dbo].[DomainVariantSet]
ADD CONSTRAINT [PK_DomainVariantSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'GameDomainConfigSet'
ALTER TABLE [dbo].[GameDomainConfigSet]
ADD CONSTRAINT [PK_GameDomainConfigSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'UserDataSet'
ALTER TABLE [dbo].[UserDataSet]
ADD CONSTRAINT [PK_UserDataSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'StrategyConfigSet'
ALTER TABLE [dbo].[StrategyConfigSet]
ADD CONSTRAINT [PK_StrategyConfigSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [UserId] in table 'NegotiationActionSet'
ALTER TABLE [dbo].[NegotiationActionSet]
ADD CONSTRAINT [FK_NegotiationActionUserId]
    FOREIGN KEY ([UserId])
    REFERENCES [dbo].[UserSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_NegotiationActionUserId'
CREATE INDEX [IX_FK_NegotiationActionUserId]
ON [dbo].[NegotiationActionSet]
    ([UserId]);
GO

-- Creating foreign key on [UserRole_Id] in table 'UserSet'
ALTER TABLE [dbo].[UserSet]
ADD CONSTRAINT [FK_UserUserRole]
    FOREIGN KEY ([UserRole_Id])
    REFERENCES [dbo].[UserRoleSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_UserUserRole'
CREATE INDEX [IX_FK_UserUserRole]
ON [dbo].[UserSet]
    ([UserRole_Id]);
GO

-- Creating foreign key on [GameId] in table 'UserSet'
ALTER TABLE [dbo].[UserSet]
ADD CONSTRAINT [FK_GameUser]
    FOREIGN KEY ([GameId])
    REFERENCES [dbo].[GameSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_GameUser'
CREATE INDEX [IX_FK_GameUser]
ON [dbo].[UserSet]
    ([GameId]);
GO

-- Creating foreign key on [GameId] in table 'NegotiationActionSet'
ALTER TABLE [dbo].[NegotiationActionSet]
ADD CONSTRAINT [FK_GameNegotiationAction]
    FOREIGN KEY ([GameId])
    REFERENCES [dbo].[GameSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_GameNegotiationAction'
CREATE INDEX [IX_FK_GameNegotiationAction]
ON [dbo].[NegotiationActionSet]
    ([GameId]);
GO

-- Creating foreign key on [GameDomainId] in table 'GameSet'
ALTER TABLE [dbo].[GameSet]
ADD CONSTRAINT [FK_GameGameDomain]
    FOREIGN KEY ([GameDomainId])
    REFERENCES [dbo].[GameDomainSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_GameGameDomain'
CREATE INDEX [IX_FK_GameGameDomain]
ON [dbo].[GameSet]
    ([GameDomainId]);
GO

-- Creating foreign key on [StrategyId] in table 'UserSet'
ALTER TABLE [dbo].[UserSet]
ADD CONSTRAINT [FK_UserStrategy]
    FOREIGN KEY ([StrategyId])
    REFERENCES [dbo].[StrategySet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_UserStrategy'
CREATE INDEX [IX_FK_UserStrategy]
ON [dbo].[UserSet]
    ([StrategyId]);
GO

-- Creating foreign key on [GameDomainId] in table 'DomainVariantSet'
ALTER TABLE [dbo].[DomainVariantSet]
ADD CONSTRAINT [FK_GameDomainDomainVariant]
    FOREIGN KEY ([GameDomainId])
    REFERENCES [dbo].[GameDomainSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_GameDomainDomainVariant'
CREATE INDEX [IX_FK_GameDomainDomainVariant]
ON [dbo].[DomainVariantSet]
    ([GameDomainId]);
GO

-- Creating foreign key on [Id] in table 'GameDomainConfigSet'
ALTER TABLE [dbo].[GameDomainConfigSet]
ADD CONSTRAINT [FK_GameDomainConfigGameDomain]
    FOREIGN KEY ([Id])
    REFERENCES [dbo].[GameDomainSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [UserUserData_UserData_Id] in table 'UserDataSet'
ALTER TABLE [dbo].[UserDataSet]
ADD CONSTRAINT [FK_UserUserData]
    FOREIGN KEY ([UserUserData_UserData_Id])
    REFERENCES [dbo].[UserSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_UserUserData'
CREATE INDEX [IX_FK_UserUserData]
ON [dbo].[UserDataSet]
    ([UserUserData_UserData_Id]);
GO

-- Creating foreign key on [Id] in table 'StrategyConfigSet'
ALTER TABLE [dbo].[StrategyConfigSet]
ADD CONSTRAINT [FK_StrategyConfigStrategy]
    FOREIGN KEY ([Id])
    REFERENCES [dbo].[StrategySet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------