#!/usr/bin/env pwsh

# API Test Script for QIMy CQRS Architecture
# Тестирует все основные endpoints

$baseUrl = "https://localhost:7105"
$insecureSkipCertificateCheck = $true

Write-Host "=== QIMy API CQRS Test ===" -ForegroundColor Cyan
Write-Host "Base URL: $baseUrl`n" -ForegroundColor Cyan

# =====================================================
# Test 1: Get All Clients
# =====================================================
Write-Host "TEST 1: Get All Clients" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/clients" -Method Get -SkipCertificateCheck -ErrorAction Stop
    Write-Host "✅ SUCCESS: Got $(($response | Measure-Object).Count) clients" -ForegroundColor Green
    $response | Select-Object -First 3 | ConvertTo-Json | Write-Host
}
catch {
    Write-Host "❌ FAILED: $_" -ForegroundColor Red
}

# =====================================================
# Test 2: Get All Products
# =====================================================
Write-Host "`nTEST 2: Get All Products" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/products" -Method Get -SkipCertificateCheck -ErrorAction Stop
    Write-Host "✅ SUCCESS: Got $(($response | Measure-Object).Count) products" -ForegroundColor Green
}
catch {
    Write-Host "❌ FAILED: $_" -ForegroundColor Red
}

# =====================================================
# Test 3: Get All Tax Rates
# =====================================================
Write-Host "`nTEST 3: Get All Tax Rates" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/taxrates" -Method Get -SkipCertificateCheck -ErrorAction Stop
    Write-Host "✅ SUCCESS: Got $(($response | Measure-Object).Count) tax rates" -ForegroundColor Green
}
catch {
    Write-Host "❌ FAILED: $_" -ForegroundColor Red
}

# =====================================================
# Test 4: Get All Units
# =====================================================
Write-Host "`nTEST 4: Get All Units" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/units" -Method Get -SkipCertificateCheck -ErrorAction Stop
    Write-Host "✅ SUCCESS: Got $(($response | Measure-Object).Count) units" -ForegroundColor Green
}
catch {
    Write-Host "❌ FAILED: $_" -ForegroundColor Red
}

# =====================================================
# Test 5: Get All Currencies
# =====================================================
Write-Host "`nTEST 5: Get All Currencies" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/currencies" -Method Get -SkipCertificateCheck -ErrorAction Stop
    Write-Host "✅ SUCCESS: Got $(($response | Measure-Object).Count) currencies" -ForegroundColor Green
}
catch {
    Write-Host "❌ FAILED: $_" -ForegroundColor Red
}

# =====================================================
# Test 6: Get All Businesses
# =====================================================
Write-Host "`nTEST 6: Get All Businesses" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/businesses" -Method Get -SkipCertificateCheck -ErrorAction Stop
    Write-Host "✅ SUCCESS: Got $(($response | Measure-Object).Count) businesses" -ForegroundColor Green
}
catch {
    Write-Host "❌ FAILED: $_" -ForegroundColor Red
}

# =====================================================
# Test 7: Get All Accounts
# =====================================================
Write-Host "`nTEST 7: Get All Accounts" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/accounts" -Method Get -SkipCertificateCheck -ErrorAction Stop
    Write-Host "✅ SUCCESS: Got $(($response | Measure-Object).Count) accounts" -ForegroundColor Green
}
catch {
    Write-Host "❌ FAILED: $_" -ForegroundColor Red
}

Write-Host "`n=== All Tests Complete ===" -ForegroundColor Cyan
