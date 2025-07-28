set "SCRIPT_DIR=%~dp0"
set "SCRIPT_DIR=%SCRIPT_DIR:~0,-1%"  REM remove trailing backslash

REM Get project root
for %%I in ("%SCRIPT_DIR%\..") do set "PROJECT_ROOT=%%~fI"

REM Set paths relative to project root
set "BLENDER_PATH=%PROJECT_ROOT%\aspenTools\Blender\blender.exe"
set "PYTHON_PATH=%PROJECT_ROOT%\aspenTools\python\source"

REM Optional: Print for verification
echo Project Root: %PROJECT_ROOT%
echo BLENDER_PATH: %BLENDER_PATH%
echo PYTHONPATH: %PYTHONPATH%

REM Launch Blender
"%BLENDER_PATH%"