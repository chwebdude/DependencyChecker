cd ./buildtab
cmd /c "npm i && npm run build"
cd ../buildtask
cmd /c "npm i && npm run build"
cd ..
dotnet restore
dotnet build -c Release --no-restore
dotnet publish --no-build -o ./buildtask/bin 
tfx extension create --manifest-globs vss-extension.json