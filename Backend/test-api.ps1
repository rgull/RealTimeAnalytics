# Test script for Real-Time Sensor Tracking API
Write-Host "Testing Real-Time Sensor Tracking API..." -ForegroundColor Green

# Wait for application to start
Write-Host "Waiting for application to start..." -ForegroundColor Yellow
Start-Sleep 15

# Test sensors endpoint
try {
    Write-Host "Testing /api/sensors endpoint..." -ForegroundColor Cyan
    $sensors = Invoke-RestMethod -Uri "http://localhost:5025/api/sensors"
    Write-Host "✅ Sensors endpoint working! Found $($sensors.Count) sensors" -ForegroundColor Green
    
    if ($sensors.Count -gt 0) {
        Write-Host "Sample sensor: $($sensors[0].Name) - $($sensors[0].Type)" -ForegroundColor White
    }
} catch {
    Write-Host "❌ Sensors endpoint failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test sensor readings endpoint
try {
    Write-Host "`nTesting /api/sensorreadings endpoint..." -ForegroundColor Cyan
    $readings = Invoke-RestMethod -Uri "http://localhost:5025/api/sensorreadings?count=10"
    Write-Host "✅ Sensor readings endpoint working! Found $($readings.Count) readings" -ForegroundColor Green
    
    if ($readings.Count -gt 0) {
        Write-Host "Sample reading: Sensor $($readings[0].sensorId) = $($readings[0].value) at $($readings[0].timestamp)" -ForegroundColor White
    }
} catch {
    Write-Host "❌ Sensor readings endpoint failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test alerts endpoint
try {
    Write-Host "`nTesting /api/alerts endpoint..." -ForegroundColor Cyan
    $alerts = Invoke-RestMethod -Uri "http://localhost:5025/api/alerts?pageSize=5"
    Write-Host "✅ Alerts endpoint working! Found $($alerts.Count) alerts" -ForegroundColor Green
} catch {
    Write-Host "❌ Alerts endpoint failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test dashboard
try {
    Write-Host "`nTesting dashboard..." -ForegroundColor Cyan
    $dashboard = Invoke-WebRequest -Uri "http://localhost:5025/" -UseBasicParsing
    if ($dashboard.StatusCode -eq 200) {
        Write-Host "✅ Dashboard accessible!" -ForegroundColor Green
    }
} catch {
    Write-Host "❌ Dashboard failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n🎉 API testing completed!" -ForegroundColor Green
Write-Host "Dashboard URL: http://localhost:5025" -ForegroundColor Yellow
Write-Host "Swagger URL: http://localhost:5025/swagger" -ForegroundColor Yellow
