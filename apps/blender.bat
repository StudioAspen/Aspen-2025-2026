@echo off
setlocal

set "APPS_DIR=%~dp0"
set "APPS_DIR=%APPS_DIR:~0,-1%"  REM remove trailing backslash

for %%I in ("%APPS_DIR%\..") do set "PROJECT_ROOT=%%~fI"
for %%I in ("%APPS_DIR%\..") do set "PROJECT_ROOT=%%~fI"

echo Project Root: %PROJECT_ROOT%

set "UV_DIR=%PROJECT_ROOT%\aspenTools\python\uv\"
set "UV_PATH=%UV_DIR%uv.exe"
set "VENV_DIR=%UV_DIR%..\venv"
set "REQS_TXT=%UV_DIR%requirements.txt"

echo Venv: %VENV_DIR%

if not exist "%VENV_DIR%" (
    echo Creating virtual environment...
    "%UV_PATH%" venv "%VENV_DIR%"
)

call "%VENV_DIR%\Scripts\activate.bat"

"%UV_PATH%" pip install -r "%REQS_TXT%"

REM Set paths relative to project root
set "BLENDER_PATH=%PROJECT_ROOT%\aspenTools\Blender\blender.exe"

REM Optional: Print for verification
echo Project Root: %PROJECT_ROOT%
echo BLENDER_PATH: %BLENDER_PATH%

REM Launch Blender
"%BLENDER_PATH%"