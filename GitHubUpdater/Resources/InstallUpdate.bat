@echo off

SET "var="&for /f "delims=0123456789" %%i in ("%1") do set var=%%i

if defined var (
    taskkill /f /im "%1.exe"
)
else (
    taskkill /f /pid %1
)

cd %2
timeout 1 > NUL

for %%f in (*) do (
    move /y %%f %3
)
for /r /d %%d in (*) do (
    move /y %%d %3
)

start "" %4