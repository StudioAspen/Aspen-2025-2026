@echo off
setlocal

set "APPS_DIR=%~dp0"
set "APPS_DIR=%APPS_DIR:~0,-1%"
for %%I in ("%APPS_DIR%\..") do set "PROJECT_ROOT=%%~fI"

set "UV_DIR=%PROJECT_ROOT%\aspenTools\python\uv\"
set "UV_PATH=%UV_DIR%uv.exe"
set "PY_DIR=%PROJECT_ROOT%\aspenTools\python\"
set "VENV_DIR=%PY_DIR%aspenVenv"
set "REQS_TXT=%UV_DIR%requirements.txt"
set "VENV_PY=%VENV_DIR%\Scripts\python.exe"

rem hard kill any old default venv folder
if exist "%PY_DIR%venv" rmdir /s /q "%PY_DIR%venv"

if not exist "%VENV_DIR%" (
  "%UV_PATH%" venv "%VENV_DIR%" --python 3.11
)

rem Force installs into aspenVenv regardless of activation
"%UV_PATH%" pip install --python "%VENV_PY%" -r "%REQS_TXT%"

call "%VENV_DIR%\Scripts\activate.bat"

set "BLENDER_PATH=%PROJECT_ROOT%\aspenTools\Blender\blender.exe"
"%BLENDER_PATH%"