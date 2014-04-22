param($installPath, $toolsPath, $package, $project)

Write-Host "Hello"
Write-Host $project.FullName
$path = [System.IO.Path]
$readmefile = $path::Combine($path::GetDirectoryName($project.FullName), "StyletReadme.txt")
$DTE.ItemOperations.OpenFile($readmefile)

