cd %1

echo "Installing update..."
echo

timeout 2 > NUL

for %%f in (*) do (
    move /y %%f %2
)

start %3