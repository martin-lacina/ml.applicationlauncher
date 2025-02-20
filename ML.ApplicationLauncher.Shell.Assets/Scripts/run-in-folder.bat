@echo off
rem Copyright (C) Martin Lacina

rem Usage: run-in-folder-bat <workdir> <command and parameters to run>

for /f "tokens=1*" %%a in ("%*") do (
    set V_WORKDIR=%%a
    set V_COMMAND=%%b
)

echo Processing: %0
echo Command: %V_COMMAND%
echo Work directory: %V_WORKDIR%
echo ------------------------------------------------------------

pushd %V_WORKDIR%%

@echo on

%V_COMMAND%

@echo off
set V_EXITCODE=%errorlevel%
popd

echo[
echo ------------------------------------------------------------
echo Exit code: %V_EXITCODE%
echo[
pause
