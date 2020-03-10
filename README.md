# Web Log Importer

This app will read all files placed in the following directory:

    C:\Temp\Logs\In

The app will combine them,  strip unnecessary headers, and bulk load them into SQL.

# One Time Set Up

 - Create a couple of folders... or let the app do it
 - Create a database with a table (script below)

 ## Folders

To start with, you'll need the following folders:

    C:\Temp\Logs\In
    C:\Temp\Logs\Out

Running the app for the first time will create these folders... you just need to remember to use them!

Place all the log files for import into `C:\Temp\Logs\In`.

## Database

And the following SQL database:

	USE [master]
	GO

	CREATE DATABASE [WebLogs]
	GO
	ALTER DATABASE [WebLogs] SET RECOVERY SIMPLE 
	GO

When the app runs, it will set up the database based on the log files being ingested.

Note! When ingesting files from more than one IIS website, please ensure the log files are configured identically. 
For example, if you add sc-bytes logging, you'll need to ensure it is added to all. Otherwise the files won't match 
on import.

### Database Access

To keep things simple, the user running the app will need permissions on the database.

# Running the App

Fill the "in" folder with IIS logs, then just run the app.

    > Fenton.WebLogImporter.exe

# Queries

It's just SQL queries now... so knock yourself out...

More examples are available from the [Web Log Importer](https://www.stevefenton.co.uk/tag/web-log-importer/) tag on my website.

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