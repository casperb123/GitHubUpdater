tskill %1
cd %2

timeout 1 > NUL

for %%f in (*) do (
    move /y %%f %3
)
for /r /d %%d in (*) do (
    move /y %%d %3
)

start "" %4