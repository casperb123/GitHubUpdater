@echo off
cd %1
timeout 2 > NUL

for /r %%f in (*) do (
    move /y %%f %2
)

start %3