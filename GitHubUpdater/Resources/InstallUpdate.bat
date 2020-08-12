@echo off
cd %1

timeout 1 > NUL

for %%f in (*) do (
    move /y %%f %2
)

start %3