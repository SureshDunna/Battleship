SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Board](
	[Id] [uniqueidentifier] NOT NULL,
	[RowSize] [int] NOT NULL,
	[ColumnSize] [int] NOT NULL
 CONSTRAINT [PK_Board_Id] PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[Ship](
	[Id] [uniqueidentifier] NOT NULL,
	[Size] [int] NOT NULL,
	[Status] [nvarchar](10) NOT NULL
 CONSTRAINT [PK_Ship_Id] PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[ShipPosition](
	[Id] [uniqueidentifier] NOT NULL,
	[BoardId] [uniqueidentifier] NOT NULL,
	[ShipId] [uniqueidentifier] NOT NULL,
	[RowPosition] [int] NOT NULL,
	[ColumnPosition] [int] NOT NULL
 CONSTRAINT [PK_ShipPosition_Id] PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [IX_ShipPosition_BoardId]
ON [dbo].[ShipPosition]
([BoardId])
WITH (ONLINE = ON)
GO

CREATE TABLE [dbo].[AttackPosition](
	[Id] [uniqueidentifier] NOT NULL,
	[BoardId] [uniqueidentifier] NOT NULL,
	[RowPosition] [int] NOT NULL,
	[ColumnPosition] [int] NOT NULL,
	[AttackStatus] [nvarchar](5) NOT NULL,
	[ShipId] [uniqueidentifier] NULL
 CONSTRAINT [PK_AttackPosition_Id] PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [IX_AttackPosition_BoardId]
ON [dbo].[AttackPosition]
([BoardId])
WITH (ONLINE = ON)
GO