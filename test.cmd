@echo off

set EXE=IBExpertUpdate\bin\Release\IBExpertUpdate.exe
set DOWNLOAD=IBExpertUpdate\bin\Release\setup_personal.exe

if exist %DOWNLOAD% (
    del %DOWNLOAD%
)

%EXE%

if not exist %DOWNLOAD% (
    echo Download não foi concluído.
    exit /b 1
)