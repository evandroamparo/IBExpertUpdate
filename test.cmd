@echo off

set RELEASE=IBExpertUpdate\bin\Release
set EXE=IBExpertUpdate.exe
set DOWNLOAD=setup_personal.exe

cd %RELEASE%

if exist %DOWNLOAD% (
    del %DOWNLOAD%
)

%EXE%

if not exist %DOWNLOAD% (
    echo Download não foi concluído.
    exit /b 1
)