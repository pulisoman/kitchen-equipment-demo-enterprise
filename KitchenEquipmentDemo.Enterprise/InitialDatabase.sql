USE [master]
GO
/****** Object:  Database [KitchenEquipmentDemo.Enterprise]    Script Date: 27/08/2025 3:19:15 pm ******/
CREATE DATABASE [KitchenEquipmentDemo.Enterprise]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'KitchenEquipmentDemo.Enterprise', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER2022\MSSQL\DATA\KitchenEquipmentDemo.Enterprise.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'KitchenEquipmentDemo.Enterprise_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER2022\MSSQL\DATA\KitchenEquipmentDemo.Enterprise_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT, LEDGER = OFF
GO
ALTER DATABASE [KitchenEquipmentDemo.Enterprise] SET COMPATIBILITY_LEVEL = 160
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [KitchenEquipmentDemo.Enterprise].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [KitchenEquipmentDemo.Enterprise] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [KitchenEquipmentDemo.Enterprise] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [KitchenEquipmentDemo.Enterprise] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [KitchenEquipmentDemo.Enterprise] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [KitchenEquipmentDemo.Enterprise] SET ARITHABORT OFF 
GO
ALTER DATABASE [KitchenEquipmentDemo.Enterprise] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [KitchenEquipmentDemo.Enterprise] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [KitchenEquipmentDemo.Enterprise] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [KitchenEquipmentDemo.Enterprise] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [KitchenEquipmentDemo.Enterprise] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [KitchenEquipmentDemo.Enterprise] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [KitchenEquipmentDemo.Enterprise] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [KitchenEquipmentDemo.Enterprise] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [KitchenEquipmentDemo.Enterprise] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [KitchenEquipmentDemo.Enterprise] SET  DISABLE_BROKER 
GO
ALTER DATABASE [KitchenEquipmentDemo.Enterprise] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [KitchenEquipmentDemo.Enterprise] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [KitchenEquipmentDemo.Enterprise] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [KitchenEquipmentDemo.Enterprise] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [KitchenEquipmentDemo.Enterprise] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [KitchenEquipmentDemo.Enterprise] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [KitchenEquipmentDemo.Enterprise] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [KitchenEquipmentDemo.Enterprise] SET RECOVERY FULL 
GO
ALTER DATABASE [KitchenEquipmentDemo.Enterprise] SET  MULTI_USER 
GO
ALTER DATABASE [KitchenEquipmentDemo.Enterprise] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [KitchenEquipmentDemo.Enterprise] SET DB_CHAINING OFF 
GO
ALTER DATABASE [KitchenEquipmentDemo.Enterprise] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [KitchenEquipmentDemo.Enterprise] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [KitchenEquipmentDemo.Enterprise] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [KitchenEquipmentDemo.Enterprise] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
EXEC sys.sp_db_vardecimal_storage_format N'KitchenEquipmentDemo.Enterprise', N'ON'
GO
ALTER DATABASE [KitchenEquipmentDemo.Enterprise] SET QUERY_STORE = ON
GO
ALTER DATABASE [KitchenEquipmentDemo.Enterprise] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 1000, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200, WAIT_STATS_CAPTURE_MODE = ON)
GO
USE [KitchenEquipmentDemo.Enterprise]
GO
/****** Object:  User [KitchenEnterpriseLogin ]    Script Date: 27/08/2025 3:19:15 pm ******/
CREATE USER [KitchenEnterpriseLogin ] FOR LOGIN [KitchenEnterpriseLogin ] WITH DEFAULT_SCHEMA=[dbo]
GO
ALTER ROLE [db_owner] ADD MEMBER [KitchenEnterpriseLogin ]
GO
ALTER ROLE [db_securityadmin] ADD MEMBER [KitchenEnterpriseLogin ]
GO
/****** Object:  Table [dbo].[equipment]    Script Date: 27/08/2025 3:19:16 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[equipment](
	[equipment_id] [int] IDENTITY(1,1) NOT NULL,
	[serial_number] [nvarchar](100) NOT NULL,
	[name] [nvarchar](100) NULL,
	[description] [nvarchar](200) NULL,
	[condition] [varchar](20) NOT NULL,
	[user_id] [int] NULL,
	[site_id] [int] NULL,
	[created_at] [datetime2](7) NOT NULL,
	[created_by] [int] NULL,
	[updated_at] [datetime2](7) NULL,
	[updated_by] [int] NULL,
	[is_deleted] [bit] NOT NULL,
	[row_version] [timestamp] NOT NULL,
 CONSTRAINT [PK_equipment] PRIMARY KEY CLUSTERED 
(
	[equipment_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[site]    Script Date: 27/08/2025 3:19:16 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[site](
	[site_id] [int] IDENTITY(1,1) NOT NULL,
	[user_id] [int] NULL,
	[code] [nvarchar](100) NULL,
	[name] [nvarchar](100) NULL,
	[description] [nvarchar](200) NULL,
	[active] [bit] NOT NULL,
	[created_at] [datetime2](7) NOT NULL,
	[created_by] [int] NULL,
	[updated_at] [datetime2](7) NULL,
	[updated_by] [int] NULL,
	[is_deleted] [bit] NOT NULL,
	[row_version] [timestamp] NOT NULL,
 CONSTRAINT [PK_site] PRIMARY KEY CLUSTERED 
(
	[site_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[site_equipment_history]    Script Date: 27/08/2025 3:19:16 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[site_equipment_history](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[equipment_id] [int] NOT NULL,
	[site_id] [int] NULL,
	[action] [varchar](20) NOT NULL,
	[action_at] [datetime2](7) NOT NULL,
	[action_by] [int] NULL,
 CONSTRAINT [PK_site_equipment_history] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[user]    Script Date: 27/08/2025 3:19:16 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[user](
	[user_id] [int] IDENTITY(1,1) NOT NULL,
	[user_type] [varchar](20) NOT NULL,
	[first_name] [nvarchar](100) NULL,
	[last_name] [nvarchar](100) NULL,
	[email_address] [nvarchar](256) NULL,
	[user_name] [nvarchar](100) NOT NULL,
	[password_hash] [varbinary](64) NOT NULL,
	[password_salt] [varbinary](16) NOT NULL,
	[password_algo] [nvarchar](50) NULL,
	[password_version] [tinyint] NULL,
	[created_at] [datetime2](7) NOT NULL,
	[created_by] [int] NULL,
	[updated_at] [datetime2](7) NULL,
	[updated_by] [int] NULL,
	[is_deleted] [bit] NOT NULL,
	[row_version] [timestamp] NOT NULL,
 CONSTRAINT [PK_user] PRIMARY KEY CLUSTERED 
(
	[user_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[user_registration_request]    Script Date: 27/08/2025 3:19:16 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[user_registration_request](
	[request_id] [int] IDENTITY(1,1) NOT NULL,
	[first_name] [nvarchar](100) NULL,
	[last_name] [nvarchar](100) NULL,
	[email_address] [nvarchar](256) NULL,
	[user_name] [nvarchar](100) NOT NULL,
	[user_type] [nvarchar](20) NULL,
	[password_hash] [varbinary](64) NOT NULL,
	[password_salt] [varbinary](16) NOT NULL,
	[status] [varchar](20) NOT NULL,
	[created_at] [datetime2](7) NOT NULL,
	[reviewed_by] [int] NULL,
	[reviewed_at] [datetime2](7) NULL,
	[review_note] [nvarchar](400) NULL,
 CONSTRAINT [PK_user_registration_request] PRIMARY KEY CLUSTERED 
(
	[request_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Index [IX_equipment_site]    Script Date: 27/08/2025 3:19:16 pm ******/
CREATE NONCLUSTERED INDEX [IX_equipment_site] ON [dbo].[equipment]
(
	[site_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_equipment_user]    Script Date: 27/08/2025 3:19:16 pm ******/
CREATE NONCLUSTERED INDEX [IX_equipment_user] ON [dbo].[equipment]
(
	[user_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UX_equipment_serial]    Script Date: 27/08/2025 3:19:16 pm ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_equipment_serial] ON [dbo].[equipment]
(
	[serial_number] ASC
)
WHERE ([is_deleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_site_user]    Script Date: 27/08/2025 3:19:16 pm ******/
CREATE NONCLUSTERED INDEX [IX_site_user] ON [dbo].[site]
(
	[user_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UX_site_owner_name]    Script Date: 27/08/2025 3:19:16 pm ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_site_owner_name] ON [dbo].[site]
(
	[user_id] ASC,
	[description] ASC
)
WHERE ([is_deleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UX_user_email]    Script Date: 27/08/2025 3:19:16 pm ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_user_email] ON [dbo].[user]
(
	[email_address] ASC
)
WHERE ([is_deleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UX_user_username]    Script Date: 27/08/2025 3:19:16 pm ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_user_username] ON [dbo].[user]
(
	[user_name] ASC
)
WHERE ([is_deleted]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UX_urr_email_pending]    Script Date: 27/08/2025 3:19:16 pm ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_urr_email_pending] ON [dbo].[user_registration_request]
(
	[email_address] ASC
)
WHERE ([status]='Pending')
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UX_urr_username_pending]    Script Date: 27/08/2025 3:19:16 pm ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_urr_username_pending] ON [dbo].[user_registration_request]
(
	[user_name] ASC
)
WHERE ([status]='Pending')
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[equipment] ADD  CONSTRAINT [DF_equipment_created_at]  DEFAULT (sysutcdatetime()) FOR [created_at]
GO
ALTER TABLE [dbo].[equipment] ADD  CONSTRAINT [DF_equipment_is_deleted]  DEFAULT ((0)) FOR [is_deleted]
GO
ALTER TABLE [dbo].[site] ADD  CONSTRAINT [DF_site_active]  DEFAULT ((1)) FOR [active]
GO
ALTER TABLE [dbo].[site] ADD  CONSTRAINT [DF_site_created_at]  DEFAULT (sysutcdatetime()) FOR [created_at]
GO
ALTER TABLE [dbo].[site] ADD  CONSTRAINT [DF_site_is_deleted]  DEFAULT ((0)) FOR [is_deleted]
GO
ALTER TABLE [dbo].[site_equipment_history] ADD  CONSTRAINT [DF_hist_action_at]  DEFAULT (sysutcdatetime()) FOR [action_at]
GO
ALTER TABLE [dbo].[user] ADD  CONSTRAINT [DF_user_created_at]  DEFAULT (sysutcdatetime()) FOR [created_at]
GO
ALTER TABLE [dbo].[user] ADD  CONSTRAINT [DF_user_is_deleted]  DEFAULT ((0)) FOR [is_deleted]
GO
ALTER TABLE [dbo].[user_registration_request] ADD  CONSTRAINT [DF_urr_status]  DEFAULT ('Pending') FOR [status]
GO
ALTER TABLE [dbo].[user_registration_request] ADD  CONSTRAINT [DF_urr_created_at]  DEFAULT (sysutcdatetime()) FOR [created_at]
GO
ALTER TABLE [dbo].[equipment]  WITH CHECK ADD  CONSTRAINT [FK_equipment_site] FOREIGN KEY([site_id])
REFERENCES [dbo].[site] ([site_id])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[equipment] CHECK CONSTRAINT [FK_equipment_site]
GO
ALTER TABLE [dbo].[equipment]  WITH CHECK ADD  CONSTRAINT [FK_equipment_user] FOREIGN KEY([user_id])
REFERENCES [dbo].[user] ([user_id])
GO
ALTER TABLE [dbo].[equipment] CHECK CONSTRAINT [FK_equipment_user]
GO
ALTER TABLE [dbo].[site]  WITH CHECK ADD  CONSTRAINT [FK_site_user] FOREIGN KEY([user_id])
REFERENCES [dbo].[user] ([user_id])
GO
ALTER TABLE [dbo].[site] CHECK CONSTRAINT [FK_site_user]
GO
ALTER TABLE [dbo].[site_equipment_history]  WITH CHECK ADD  CONSTRAINT [FK_hist_equipment] FOREIGN KEY([equipment_id])
REFERENCES [dbo].[equipment] ([equipment_id])
GO
ALTER TABLE [dbo].[site_equipment_history] CHECK CONSTRAINT [FK_hist_equipment]
GO
ALTER TABLE [dbo].[site_equipment_history]  WITH CHECK ADD  CONSTRAINT [FK_hist_site] FOREIGN KEY([site_id])
REFERENCES [dbo].[site] ([site_id])
GO
ALTER TABLE [dbo].[site_equipment_history] CHECK CONSTRAINT [FK_hist_site]
GO
ALTER TABLE [dbo].[user_registration_request]  WITH CHECK ADD  CONSTRAINT [FK_urr_reviewed_by] FOREIGN KEY([reviewed_by])
REFERENCES [dbo].[user] ([user_id])
GO
ALTER TABLE [dbo].[user_registration_request] CHECK CONSTRAINT [FK_urr_reviewed_by]
GO
ALTER TABLE [dbo].[site_equipment_history]  WITH CHECK ADD  CONSTRAINT [CK_hist_action] CHECK  (([action]='Unregister' OR [action]='Register'))
GO
ALTER TABLE [dbo].[site_equipment_history] CHECK CONSTRAINT [CK_hist_action]
GO
USE [master]
GO
ALTER DATABASE [KitchenEquipmentDemo.Enterprise] SET  READ_WRITE 
GO
