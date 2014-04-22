param($installPath, $toolsPath, $package, $project)

$path = [System.IO.Path]
$readmefile = $path::Combine($path::GetDirectoryName($project.FullName), "StyletReadme.txt")
$DTE.ItemOperations.OpenFile($readmefile)

