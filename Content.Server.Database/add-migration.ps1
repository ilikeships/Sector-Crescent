#!/usr/bin/env pwsh

param([String]$name)

if ($name -eq "")
{
    Write-Error "must specify migration name"
    Read-Host -Prompt "Press Enter to exit"
    exit
}

dotnet ef migrations add --context SqliteServerDbContext -o Migrations/Sqlite $name
dotnet ef migrations add --context PostgresServerDbContext -o Migrations/Postgres $name

Read-Host -Prompt "Press Enter to exit"
