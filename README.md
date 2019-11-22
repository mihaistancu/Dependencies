# Dependencies

Run the following in PowerShell:

$path = "<your_path_here>"
$files = gci $path -filter *.dll |  % { $_.FullName }
.\Dependencies.exe $files
