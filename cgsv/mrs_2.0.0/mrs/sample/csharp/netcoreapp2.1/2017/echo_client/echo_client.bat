@echo off

set PROJ_NAME=echo_client

if "%1"=="debug" (
    call :debug_build
) else if "%1"=="release" (
    call :release_build
) else if "%1"=="start" (
    call :start_debug
) else if "%1"=="startd" (
    call :start_debug
) else if "%1"=="startr" (
    call :start_release
) else (
    echo *.bat [option]
    echo debug   :debugビルド
    echo release :releaseビルド
    echo start   :debug実行
    echo startd  :debug実行
    echo startr  :release実行
)

exit /b

rem debugビルド
:debug_build
    call :build Debug
exit /b

rem releaseビルド
:release_build
    call :build Release
exit /b

rem ビルド実行
:build
    dotnet restore %PROJ_NAME%.csproj
    dotnet publish %PROJ_NAME%.csproj --configuration %1
    copy ..\..\..\..\..\library\mrs\windows\enet_uv_openssl_1.1.1\2017\MT\x64\%1\mrs.dll .\bin\x64\%1\netcoreapp2.1\publish
exit /b

rem debug実行
:start_debug
    call :run x64 Debug
exit /b

rem release実行
:start_release
    call :run x64 Release
exit /b

rem 実行
:run
    dotnet .\bin\%1\%2\netcoreapp2.1\publish\%PROJ_NAME%.dll
exit /b
