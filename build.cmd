@echo off
cls

.paket\paket.exe restore
if errorlevel 1 (
  exit /b %errorlevel%
)

fake.cmd run build.fsx %*
