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

	USE [WebLogs]
	GO

	SET ANSI_NULLS ON
	GO
	SET QUOTED_IDENTIFIER ON
	GO

	CREATE TABLE [dbo].[LogEntry](
		[date] datetime2 NULL,
		[time] datetime2 NULL,
		[s_ip] [nvarchar](50) NULL,
		[cs_method] [nvarchar](50) NULL,
		[cs_uri_stem] [nvarchar](max) NULL,
		[cs_uri_query] [nvarchar](max) NULL,
		[s_port] [int] NULL,
		[cs_username] [nvarchar](max) NULL,
		[c_ip] [nvarchar](50) NULL,
		[cs_User_Agent_] [nvarchar](max) NULL,
		[cs_Referer_] [nvarchar](max) NULL,
		[cs_host] [nvarchar](max) NULL,
		[sc_status] [int] NULL,
		[sc_substatus] [int] NULL,
		[sc_win32_status] [int] NULL,
		[time_taken] [int] NULL,
		[X_Forwarded_For] [nvarchar](50) NULL,
		INDEX cci CLUSTERED COLUMNSTORE
	) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
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