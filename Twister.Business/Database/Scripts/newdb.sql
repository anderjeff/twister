USE [master]
GO
/****** Object:  Database [Twister]    Script Date: 7/9/2018 11:47:01 AM ******/
CREATE DATABASE [Twister] ON  PRIMARY 
( NAME = N'Twister', FILENAME = N'c:\SqlData\Twister.mdf' , SIZE = 17408KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
 LOG ON 
( NAME = N'Twister_log', FILENAME = N'c:\SqlData\Twister_log.ldf' , SIZE = 6272KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)
GO
ALTER DATABASE [Twister] SET COMPATIBILITY_LEVEL = 100
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [Twister].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [Twister] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [Twister] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [Twister] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [Twister] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [Twister] SET ARITHABORT OFF 
GO
ALTER DATABASE [Twister] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [Twister] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [Twister] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [Twister] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [Twister] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [Twister] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [Twister] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [Twister] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [Twister] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [Twister] SET  DISABLE_BROKER 
GO
ALTER DATABASE [Twister] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [Twister] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [Twister] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [Twister] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [Twister] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [Twister] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [Twister] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [Twister] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [Twister] SET  MULTI_USER 
GO
ALTER DATABASE [Twister] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [Twister] SET DB_CHAINING OFF 
GO
USE [Twister]
GO
/****** Object:  Table [dbo].[BenchOperator]    Script Date: 7/9/2018 11:47:02 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BenchOperator](
	[ClockId] [nvarchar](15) NOT NULL,
	[FirstName] [nvarchar](50) NULL,
	[LastName] [nvarchar](50) NULL,
	[InactiveDate] [datetime] NULL,
 CONSTRAINT [PK_BenchOperator] PRIMARY KEY CLUSTERED 
(
	[ClockId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CalibratedParts]    Script Date: 7/9/2018 11:47:02 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CalibratedParts](
	[PartNumber] [nvarchar](160) NOT NULL,
	[Revision] [nvarchar](12) NOT NULL,
	[DateCalibrated] [datetime] NULL,
	[CalibratedBy] [nvarchar](15) NULL,
	[NominalCwDeflection] [decimal](6, 2) NULL,
	[NominalCcwDeflection] [decimal](6, 2) NULL,
	[CwTestTorque] [int] NULL,
	[CcwTestTorque] [int] NULL,
	[Username] [nvarchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[Revision] ASC,
	[PartNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DataPoint]    Script Date: 7/9/2018 11:47:02 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DataPoint](
	[TestId] [nvarchar](6) NOT NULL,
	[SampleTime] [time](7) NOT NULL,
	[Torque] [int] NULL,
	[Angle] [decimal](5, 2) NULL,
 CONSTRAINT [PK_DataPoint] PRIMARY KEY CLUSTERED 
(
	[TestId] ASC,
	[SampleTime] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Logging]    Script Date: 7/9/2018 11:47:02 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Logging](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Timestamp] [datetime] NULL,
	[Thread] [nvarchar](50) NULL,
	[Level] [nvarchar](50) NULL,
	[Source] [nvarchar](300) NULL,
	[Message] [nvarchar](4000) NULL,
	[Exception] [nvarchar](4000) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TestDirections]    Script Date: 7/9/2018 11:47:02 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TestDirections](
	[Direction] [nvarchar](5) NOT NULL,
	[Description] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_TestDirections] PRIMARY KEY CLUSTERED 
(
	[Direction] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TestTemplate]    Script Date: 7/9/2018 11:47:02 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TestTemplate](
	[Id] [int] NOT NULL,
	[TestDescription] [nvarchar](50) NULL,
	[ClockwiseTorque] [int] NOT NULL,
	[CounterclockwiseTorque] [int] NOT NULL,
	[RunSpeed] [int] NOT NULL,
	[MoveSpeed] [int] NOT NULL,
 CONSTRAINT [PK_TestTemplate] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TorqueTest]    Script Date: 7/9/2018 11:47:02 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TorqueTest](
	[TestId] [nvarchar](6) NOT NULL,
	[TestTemplateId] [int] NOT NULL,
	[EmployeeNumber] [nvarchar](15) NOT NULL,
	[WorkId] [nvarchar](15) NOT NULL,
	[TestDate] [date] NULL,
	[StartTime] [time](7) NULL,
	[FinishTime] [time](7) NULL,
	[Direction] [nvarchar](5) NULL,
 CONSTRAINT [PK_TorqueTest] PRIMARY KEY CLUSTERED 
(
	[TestId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[BenchOperator] ADD  CONSTRAINT [DF_BenchOperator_Inactive]  DEFAULT ((0)) FOR [InactiveDate]
GO
ALTER TABLE [dbo].[TestTemplate] ADD  CONSTRAINT [DF_TestTemplate_ClockwiseTorque]  DEFAULT ((0)) FOR [ClockwiseTorque]
GO
ALTER TABLE [dbo].[TestTemplate] ADD  CONSTRAINT [DF_TestTemplate_CounterclockwiseTorque]  DEFAULT ((0)) FOR [CounterclockwiseTorque]
GO
ALTER TABLE [dbo].[TestTemplate] ADD  CONSTRAINT [DF_TestTemplate_RunSpeed]  DEFAULT ((1)) FOR [RunSpeed]
GO
ALTER TABLE [dbo].[TestTemplate] ADD  CONSTRAINT [DF_TestTemplate_MoveSpeed]  DEFAULT ((1)) FOR [MoveSpeed]
GO
ALTER TABLE [dbo].[TorqueTest] ADD  CONSTRAINT [DF_TorqueTest_Direction]  DEFAULT (N'BD') FOR [Direction]
GO
ALTER TABLE [dbo].[DataPoint]  WITH CHECK ADD  CONSTRAINT [FK_DataPoint_TorqueTest] FOREIGN KEY([TestId])
REFERENCES [dbo].[TorqueTest] ([TestId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[DataPoint] CHECK CONSTRAINT [FK_DataPoint_TorqueTest]
GO
ALTER TABLE [dbo].[TorqueTest]  WITH CHECK ADD  CONSTRAINT [FK_TorqueTest_BenchOperator] FOREIGN KEY([EmployeeNumber])
REFERENCES [dbo].[BenchOperator] ([ClockId])
GO
ALTER TABLE [dbo].[TorqueTest] CHECK CONSTRAINT [FK_TorqueTest_BenchOperator]
GO
ALTER TABLE [dbo].[TorqueTest]  WITH CHECK ADD  CONSTRAINT [FK_TorqueTest_TestDirections] FOREIGN KEY([Direction])
REFERENCES [dbo].[TestDirections] ([Direction])
GO
ALTER TABLE [dbo].[TorqueTest] CHECK CONSTRAINT [FK_TorqueTest_TestDirections]
GO
ALTER TABLE [dbo].[TorqueTest]  WITH CHECK ADD  CONSTRAINT [FK_TorqueTest_TestTemplate] FOREIGN KEY([TestTemplateId])
REFERENCES [dbo].[TestTemplate] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TorqueTest] CHECK CONSTRAINT [FK_TorqueTest_TestTemplate]
GO
/****** Object:  StoredProcedure [dbo].[procLog_Insert]    Script Date: 7/9/2018 11:47:02 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- the stored procedure used by log4net to do my logging.
CREATE PROC [dbo].[procLog_Insert]
    @log_date DATETIME
  , @log_thread NVARCHAR(50)
  , @log_level NVARCHAR(50)
  , @log_source NVARCHAR(300)
  , @log_message NVARCHAR(4000)
  , @exception NVARCHAR(4000)
AS
    INSERT  INTO dbo.Logging
            ( Timestamp
            , Thread
            , Level
            , Source
            , Message
            , Exception
            )
    VALUES  ( @log_date
            , @log_thread
            , @log_level
            , @log_source
            , @log_message
            , @exception
            )
GO
/****** Object:  StoredProcedure [dbo].[up_DeleteCalibration]    Script Date: 7/9/2018 11:47:02 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[up_DeleteCalibration]
    @PartNumber NVARCHAR(160)
  , @Revision NVARCHAR(12)
AS
    DELETE  dbo.CalibratedParts
    WHERE   PartNumber = @PartNumber
            AND Revision = @Revision
GO
/****** Object:  StoredProcedure [dbo].[up_GetBenchOperatorByClockId]    Script Date: 7/9/2018 11:47:02 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[up_GetBenchOperatorByClockId] @ClockId NVARCHAR(15)
AS
    SELECT  b.ClockId
          , b.FirstName
          , b.LastName
    FROM    dbo.BenchOperator b
    WHERE   b.ClockId = @ClockId


GO
/****** Object:  StoredProcedure [dbo].[up_GetTestById]    Script Date: 7/9/2018 11:47:02 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[up_GetTestById] @testId NVARCHAR(6)
AS -- just want to see if there is a record with a 
	-- matching TestId value, and if there is, return 
	-- that record.
    SELECT  TestId
          , TestTemplateId
          , EmployeeNumber
          , WorkId
          , TestDate
          , StartTime
          , FinishTime
    FROM    dbo.TorqueTest
    WHERE   TestId = @testId

GO
/****** Object:  StoredProcedure [dbo].[up_LoadPreviousCalibration]    Script Date: 7/9/2018 11:47:02 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[up_LoadPreviousCalibration]
    @PartNumber NVARCHAR(160)
  , @Revision NVARCHAR(12)
AS
    SELECT  PartNumber
          , Revision
          , DateCalibrated
          , CalibratedBy
          , NominalCwDeflection
          , NominalCcwDeflection
          , CwTestTorque
          , CcwTestTorque
    FROM    dbo.CalibratedParts
    WHERE   PartNumber = @PartNumber
            AND Revision = @Revision
GO
/****** Object:  StoredProcedure [dbo].[up_LoadTestSettings]    Script Date: 7/9/2018 11:47:02 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[up_LoadTestSettings] @templateId INT
AS
    SELECT  tt.Id
          , tt.TestDescription
          , tt.ClockwiseTorque
          , tt.CounterclockwiseTorque
          , tt.RunSpeed
          , tt.MoveSpeed
    FROM    dbo.TestTemplate tt
    WHERE   tt.Id = @templateId


INSERT INTO dbo.TestTemplate
        ( Id
        , TestDescription
        , ClockwiseTorque
        , CounterclockwiseTorque
        , RunSpeed
        , MoveSpeed
        )
VALUES  ( 1, 'Steering Shaft Test - +/- 4,000 in-lbs', 4000, -4000, 1, 1 )
GO
/****** Object:  StoredProcedure [dbo].[up_NoDemandYetHasOrderPointPurchasedPart]    Script Date: 7/9/2018 11:47:02 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--EXEC salesManager.up_GetTelephoneSalesInformation1

CREATE PROC [dbo].[up_NoDemandYetHasOrderPointPurchasedPart]
AS
    DECLARE @OnHand TABLE
        (
          PartId NVARCHAR(160) ,
          Description NVARCHAR(250) ,
          OrderPoint DECIMAL(14, 4) ,
          QtyOnHand DECIMAL(14, 4)
        )

    INSERT  INTO @OnHand
            SELECT  ip.ID AS PartId ,
                    ISNULL(ip.DESCRIPTION, '') AS Description ,
                    ip.ORDER_POINT ,
                    SUM(ipl.QTY_ON_HAND) AS OnHand
            FROM    dbo.INVENTORY_PART AS ip
                    INNER JOIN dbo.INVENTORY_PART_LOCATION AS ipl ON ip.ID = ipl.PART_ID
            WHERE   ip.PRODUCT_CODE = 'P'
            GROUP BY ip.ID ,
                    ip.DESCRIPTION ,
                    ip.ORDER_POINT 

-- now get the current demand for each part, and join it with the @OnHand table.

    DECLARE @CurrentDemand TABLE
        (
          PartId NVARCHAR(160) PRIMARY KEY ,
          Demand DECIMAL(14, 4)
        )

    INSERT  INTO @CurrentDemand
            SELECT  ip.ID ,
                    SUM(ISNULL(WoDemand.Demand, 0)) + SUM(ISNULL(SoDemand.Due,
                                                              0)) AS Demand
            FROM    dbo.INVENTORY_PART ip
                    LEFT JOIN ( SELECT  Material.SUBORD_PART_ID AS PartId ,
                                        SUM(Material.CALC_QTY
                                            - Material.ISSUED_QTY) AS Demand
                                FROM    dbo.SHOPFLOOR_WORK_ORDER AS Wo
                                        JOIN dbo.SHOPFLOOR_MATERIAL AS Material ON Material.WORK_ID = Wo.WORK_ID
                                WHERE   Wo.ORDER_STATUS = 'Released'
                                        AND Material.CALC_QTY
                                        - Material.ISSUED_QTY > 0
                                GROUP BY Material.SUBORD_PART_ID
                              ) WoDemand ON ip.ID = WoDemand.PartId
                    LEFT JOIN ( SELECT  Sol.PART_ID AS PartId ,
                                        SUM(Sol.ORDER_QTY - Sol.SHIPPED_QTY) AS Due
                                FROM    dbo.SALES_SALES_ORDER AS So
                                        JOIN dbo.SALES_ORDER_LINE AS Sol ON So.ID = Sol.SALES_ID
                                WHERE   So.ORDER_STATUS = 'Released'
                                        AND Sol.LINE_STATUS = 'Open'
                                        AND Sol.ORDER_QTY - Sol.SHIPPED_QTY > 0
                                GROUP BY Sol.PART_ID
                              ) SoDemand ON ip.ID = SoDemand.PartId
            GROUP BY ip.ID
            ORDER BY ip.ID


    SELECT  oh.PartId ,
            oh.Description ,
            oh.OrderPoint ,
            oh.QtyOnHand ,
            cd.Demand
    FROM    @OnHand oh
            LEFT JOIN @CurrentDemand cd ON cd.PartId = oh.PartId
    WHERE   cd.Demand <= 0-- NO DEMAND
            AND oh.OrderPoint > 0 --STILL HAS AN ORDER POINT

GO
/****** Object:  StoredProcedure [dbo].[up_SaveCalibratedParts]    Script Date: 7/9/2018 11:47:02 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROC [dbo].[up_SaveCalibratedParts]
    @PartNumber NVARCHAR(160)
  , @Revision NVARCHAR(12)
  , @DateCalibrated DATETIME
  , @CalibratedBy NVARCHAR(15)
  , @NominalCwDeflection DECIMAL(6, 2)
  , @NominalCcwDeflection DECIMAL(6, 2)
  , @CwTestTorque INT
  , @CcwTestTorque INT
  , @Username NVARCHAR(255)
AS
    IF EXISTS ( SELECT  *
                FROM    dbo.CalibratedParts
                WHERE   PartNumber = @PartNumber
                        AND Revision = @Revision )
        BEGIN
            UPDATE  dbo.CalibratedParts
            SET     DateCalibrated = @DateCalibrated
                  , CalibratedBy = @CalibratedBy
                  , NominalCwDeflection = @NominalCwDeflection
                  , NominalCcwDeflection = @NominalCcwDeflection
                  , CwTestTorque = @CwTestTorque
                  , CcwTestTorque = @CcwTestTorque
				  , Username = @Username
            WHERE   PartNumber = @PartNumber
                    AND Revision = @Revision
        END
    ELSE
        BEGIN
            INSERT  INTO dbo.CalibratedParts
                    ( PartNumber
                    , Revision
                    , DateCalibrated
                    , CalibratedBy
                    , NominalCwDeflection
                    , NominalCcwDeflection
                    , CwTestTorque
                    , CcwTestTorque
					, Username
		            )
            VALUES  ( @PartNumber
                    , @Revision
                    , @DateCalibrated
                    , @CalibratedBy
                    , @NominalCwDeflection
                    , @NominalCcwDeflection
                    , @CwTestTorque
                    , @CcwTestTorque
					, @Username
		            )
        END
GO
/****** Object:  StoredProcedure [dbo].[up_SaveDataPoint]    Script Date: 7/9/2018 11:47:02 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



/* 
    Save a single data point from a test.
*/
CREATE PROCEDURE [dbo].[up_SaveDataPoint]
    @TestId NVARCHAR(6)
  , @SampleTime TIME(7)
  , @Torque INT
  , @Angle DECIMAL(5, 2)
AS
    IF EXISTS ( SELECT  *
                FROM    dbo.DataPoint
                WHERE   TestId = @TestId
                        AND SampleTime = @SampleTime )
        BEGIN
            UPDATE  dbo.DataPoint
            SET     TestId = @TestId
                  , SampleTime = @SampleTime
                  , Torque = @Torque
                  , Angle = @Angle
            WHERE   TestId = @TestId
                    AND SampleTime = @SampleTime
        END
    ELSE
        BEGIN
            INSERT  INTO dbo.DataPoint
                    ( TestId
                    , SampleTime
                    , Torque
                    , Angle
                    )
            VALUES  ( @TestId
                    , @SampleTime
                    , @Torque
                    , @Angle
                    )
        END



GO
/****** Object:  StoredProcedure [dbo].[up_SaveTestSettings]    Script Date: 7/9/2018 11:47:02 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[up_SaveTestSettings]
       @testId INT
     , @runSpeed INT
     , @moveSpeed INT
AS
       UPDATE   dbo.TestTemplate
       SET      RunSpeed = @runSpeed
              , MoveSpeed = @moveSpeed
       WHERE    Id = @testId
GO
/****** Object:  StoredProcedure [dbo].[up_SaveTwisterTest]    Script Date: 7/9/2018 11:47:02 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*  
    Save a TorqueTest, UPDATE if exists, 
    if not INSERT
*/
CREATE PROC [dbo].[up_SaveTwisterTest]
    @TestId NVARCHAR(6)
  , @TestTemplateId INT
  , @EmployeeNumber NVARCHAR(15)
  , @WorkId NVARCHAR(15)
  , @TestDate DATE
  , @StartTime TIME(7)
  , @FinishTime TIME(7)
  , @Direction NVARCHAR(5)
AS
    IF EXISTS ( SELECT  *
                FROM    dbo.TorqueTest
                WHERE   TestId = @TestId )
        BEGIN
            UPDATE  dbo.TorqueTest
            SET     TestId = @TestId
                  , TestTemplateId = @TestTemplateId
                  , EmployeeNumber = @EmployeeNumber
                  , WorkId = @WorkId
                  , TestDate = @TestDate
                  , StartTime = @StartTime
                  , FinishTime = @FinishTime
				  , Direction = @Direction
            WHERE   TestId = @TestId
        END
    ELSE
        BEGIN
            INSERT  INTO dbo.TorqueTest
                    ( TestId
                    , TestTemplateId
                    , EmployeeNumber
                    , WorkId
                    , TestDate
                    , StartTime
                    , FinishTime
					, Direction
		            )
            VALUES  ( @TestId
                    , @TestTemplateId
                    , @EmployeeNumber
                    , @WorkId
                    , @TestDate
                    , @StartTime
                    , @FinishTime
					, @Direction
		            )
        END

GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'The clock number for the employee, values are set by VJS, in SHOPFLOOR_DIRECT_EMPLOYEE.CLOCK_NO, though the two tables are not linked.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'BenchOperator', @level2type=N'COLUMN',@level2name=N'ClockId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'The bench operator''s first name.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'BenchOperator', @level2type=N'COLUMN',@level2name=N'FirstName'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'The bench operator''s last name.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'BenchOperator', @level2type=N'COLUMN',@level2name=N'LastName'
GO
USE [master]
GO
ALTER DATABASE [Twister] SET  READ_WRITE 
GO
