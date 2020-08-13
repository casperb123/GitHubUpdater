@echo off
tskill %1
cd %2

timeout 1 > NUL

for %%f in (*) do (
    move /y %%f %3
)

start %4