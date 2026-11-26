@echo off
dotnet build "%~dp0PaquitoBot.Api\PaquitoBot.Api.csproj" %*
exit /b %ERRORLEVEL%
