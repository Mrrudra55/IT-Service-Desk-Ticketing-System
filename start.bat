@echo off
title IT Service Desk Launcher
echo ========================================================
echo    Starting IT Service Desk Ticketing System
echo ========================================================
echo.
echo [1/2] Starting ASP.NET Core Web API on http://localhost:5276...
start "Backend API (ASP.NET Core)" cmd /k "dotnet run"

echo [2/2] Starting React Vite Frontend on http://localhost:5173...
start "Frontend UI (React Vite)" cmd /k "cd ITServiceDesk.Client && npm run dev"

echo.
echo ========================================================
echo  Both services launched in separate windows!
echo  - Frontend: http://localhost:5173
echo  - Swagger API: http://localhost:5276/swagger
echo ========================================================

