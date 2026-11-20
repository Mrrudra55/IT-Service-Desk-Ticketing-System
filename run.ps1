Write-Host "========================================================" -ForegroundColor Cyan
Write-Host "   Starting IT Service Desk Ticketing System (Full Stack)" -ForegroundColor Cyan
Write-Host "========================================================" -ForegroundColor Cyan

# 1. Start Backend in its own window
Write-Host "[1/2] Launching Backend Web API on http://localhost:5276..." -ForegroundColor Green
Start-Process powershell -ArgumentList "-NoExit", "-Command", "Write-Host 'Backend API Server Running...' -ForegroundColor Green; dotnet run"

# 2. Start Frontend in its own window
Write-Host "[2/2] Launching React Vite Frontend on http://localhost:5173..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "Write-Host 'React Frontend Server Running...' -ForegroundColor Cyan; cd ITServiceDesk.Client; npm run dev"

Write-Host ""
Write-Host "Both services are now running!" -ForegroundColor Green
Write-Host "• Frontend UI: http://localhost:5173" -ForegroundColor White
Write-Host "• Swagger UI:  http://localhost:5276/swagger" -ForegroundColor White

