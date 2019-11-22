# Dependencies

Open PowerShell, switch to the folder where the executable was built and run the following:

```
$path = "<your_path_here>"
$files = gci $path -filter *.dll |  % { $_.FullName }
.\Dependencies.exe $files
```
