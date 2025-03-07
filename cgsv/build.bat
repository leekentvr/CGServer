@echo off
rem Visual Studio development environment setup
call "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\Common7\Tools\VsDevCmd.bat" -arch=x64

rem Build using NMAKE
nmake /f Makefile.win %*

if errorlevel 1 (
    echo Build failed.
    exit /b 1
)

echo Build completed.
