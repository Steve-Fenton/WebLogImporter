# Web Log Importer

This app will read all files placed in the following directory, combine them and strip unnecessary headers, and bulk load them into SQL.

    C:\Temp\Logs\In

# One Time Set Up

 - Create a couple of folders... or let the app do it
 - Create a database with a table (script below)

To start with, you'll need the following folders:

    C:\Temp\Logs\In
    C:\Temp\Logs\Out

Running the app for the first time will create these folders... you just need to remember to use them!

And the following SQL database:

	USE [master]
	GO

	CREATE DATABASE [WebLogs]
	GO
	ALTER DATABASE [WebLogs] SET RECOVERY SIMPLE 
	GO

The following script will set up your database... I plan to make this automatic and dynamic based on different logging configurations.

	USE [WebLogs]
	GO

	IF OBJECT_ID('dbo.LogEntry', 'U') IS NOT NULL 
	BEGIN
		DROP TABLE dbo.LogEntry
	END
	GO

	IF OBJECT_ID('dbo.RoundToMinutes', 'FN') IS NOT NULL 
	BEGIN
		DROP FUNCTION dbo.RoundToMinutes
	END
	GO

	SET ANSI_NULLS ON
	GO
	SET QUOTED_IDENTIFIER ON
	GO

	CREATE TABLE [dbo].[LogEntry](
		[date]            DATETIME2       NULL,
		[time]            DATETIME2       NULL,
		[s_ip]            [NVARCHAR](100) NULL,
		[cs_method]       [NVARCHAR](50)  NULL,
		[cs_uri_stem]     [NVARCHAR](MAX) NULL,
		[cs_uri_query]    [NVARCHAR](MAX) NULL,
		[s_port]          [INT]           NULL,
		[cs_username]     [NVARCHAR](MAX) NULL,
		[c_ip]            [NVARCHAR](100) NULL,
		[cs_User_Agent]   [NVARCHAR](MAX) NULL,
		[cs_Referer]      [NVARCHAR](MAX) NULL,
		[cs_host]         [NVARCHAR](MAX) NULL,
		[sc_status]       [INT]           NULL,
		[sc_substatus]    [INT]           NULL,
		[sc_win32_status] [INT]           NULL,
		[time_taken]      [INT]           NULL,
		[X_Forwarded_For] [NVARCHAR](100) NULL,
		INDEX cci CLUSTERED COLUMNSTORE
	) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
	GO

	CREATE FUNCTION dbo.RoundToMinutes(@date DATETIME2, @time DATETIME2, @minutes INT) RETURNS SMALLDATETIME
	AS
	BEGIN
		SELECT @date = CAST(@date AS DATETIME) + CAST(@time AS DATETIME)
		RETURN DATEADD(MINUTE, (DATEDIFF(MINUTE, 0, @date ) / @minutes) * @minutes, 0)
	END
	GO

# Database Access

To keep things simple, the user running the app will need permissions on the database.

# Running the App

Fill the "in" folder with IIS logs, then just run the app.

    > Fenton.WebLogImporter.exe

# Queries

It's just SQL queries now... so knock yourself out...

## X-Forwarded-For Rankings

	SELECT
		[X_Forwarded_For],
		COUNT(1)
	FROM
		LogEntry
	GROUP BY
		[X_Forwarded_For]
	ORDER BY
		COUNT(1) DESC

## Host Rankings

	SELECT
		[cs_host], COUNT(1)
	FROM
		LogEntry
	GROUP BY
		[cs_host]
	ORDER BY
		COUNT(1) DESC

## Errors

	SELECT
		*
	FROM
		LogEntry
	WHERE
		[sc_status] >= 400

## Filter by Date and Time

You can filter by date and time using the following (using "date =" because time is in a separate column, and "time BETWEEN" to grab a range)

	WHERE
		[date] = '2020-03-10'
	AND
		[time] BETWEEN '12:20' AND '12:50'

For example,

	SELECT
		[X_Forwarded_For],
		COUNT(1)
	FROM
		LogEntry
	WHERE
		[date] = '2020-03-10'
	AND
		[time] BETWEEN '12:20' AND '12:50'
	GROUP BY
		[X_Forwarded_For]
	ORDER BY
		COUNT(1) DESC