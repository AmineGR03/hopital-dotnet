# Script pour démarrer l'application gestion_hopital
Write-Host "Démarrage de l'application gestion_hopital..." -ForegroundColor Cyan

# Vérifier si le port est déjà utilisé
$port = netstat -ano | findstr :5285
if ($port) {
    Write-Host "Le port 5285 est déjà utilisé. Arrêt des processus..." -ForegroundColor Yellow
    Get-Process | Where-Object {$_.ProcessName -like "*gestion_hopital*"} | Stop-Process -Force -ErrorAction SilentlyContinue
    taskkill /F /IM gestion_hopital.exe 2>$null
    Start-Sleep -Seconds 2
}

# Changer vers le répertoire du projet
Set-Location "gestion_hopital"

# Démarrer l'application
Write-Host "Lancement de dotnet run..." -ForegroundColor Cyan
dotnet run


