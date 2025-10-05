

set UV_DIR=%~dp0
set UV_PATH=%UV_DIR%uv.exe
set VENV_DIR=%UV_DIR%..\venv
set REQS_TXT=%UV_DIR%requirements.txt

if not exist "%VENV_DIR%" (
    echo Creating virtual environment...
    "%UV_PATH%" venv "%VENV_DIR%"
)

call "%VENV_DIR%\Scripts\activate.bat"


"%UV_PATH%" pip install -r %REQS_TXT%

PAUSE