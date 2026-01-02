# Script pour arrêter l'application gestion_hopital
Write-Host "Arrêt de l'application gestion_hopital..." -ForegroundColor Yellow

# Arrêter tous les processus gestion_hopital
Get-Process | Where-Object {$_.ProcessName -like "*gestion_hopital*"} | Stop-Process -Force -ErrorAction SilentlyContinue
taskkill /F /IM gestion_hopital.exe 2>$null

# Vérifier le port 5285
$port = netstat -ano | findstr :5285
if ($port) {
    Write-Host "Port 5285 encore utilisé, recherche du processus..." -ForegroundColor Yellow
    $portInfo = $port | Select-String "LISTENING"
    if ($portInfo) {
        $pid = ($portInfo -split '\s+')[-1]
        if ($pid) {
            taskkill /F /PID $pid 2>$null
            Write-Host "Processus $pid arrêté" -ForegroundColor Green
        }
    }
}

Start-Sleep -Seconds 1
Write-Host "Application arrêtée!" -ForegroundColor Green


