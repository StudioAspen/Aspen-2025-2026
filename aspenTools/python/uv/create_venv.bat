@echo off
set UV_PATH=C:\Users\mikin\Documents\Aspen\Aspen-2025-2026\aspenTools\python\uv\uv.exe
set VENV_DIR=C:\Users\mikin\Documents\Aspen\Aspen-2025-2026\aspenTools\python\source\venv

if not exist "%VENV_DIR%" (
    echo Creating virtual environment...
    "%UV_PATH%" venv "%VENV_DIR%"
)