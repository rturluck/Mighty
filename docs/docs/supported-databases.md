---
title: Supported Databases
layout: default
nav_order: 15
---

# Supported Databases
{: .no_toc }

- TOC
{:toc}

## ADO.NET Database Drivers

Here are the currently supported ADO.NET database drivers in Mighty:

|ADO.NET Provider Name|.NET Framework 4.0+|.NET Core 1.0+|
|:-----|:-----|:-----|:-----|
|System.Data.SqlClient|YES|YES|
|Oracle.ManagedDataAccess.Client|YES|2.0+|
|Oracle.DataAccess.Client|YES|[NO](https://www.oracle.com/technetwork/topics/dotnet/tech-info/odpnet-dotnet-core-sod-3628981.pdf)|
|Npgsql|YES|YES|
|MySql.Data.MySqlClient|YES|YES|
|Devart.Data.MySql|YES|YES|
|System.Data.SQLite|YES|NO|
|Microsoft.Data.Sqlite|NO|YES|

## Parameter Prefixes

For named parameters in Mighty (like `@ProductID` or `:DNAME`) and auto-numbered parameters (like `@0`, `@1` or `:0`, `:1`) the correct [parameter prefix](parameters#auto-numbered-parameters) to use depends on the database:

|Database|Parameter Prefix|
|:-----|:-----:|
|SQL Server|@|
|Oracle|\:|
|PostgreSQL|\:|
|MySQL|@|
|SQLite|@|
