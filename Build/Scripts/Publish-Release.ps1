[CmdletBinding()]
param (
	$Config = 'Release'
)

begin {
    Write-Host "Publishing applications ($Config)..." -ForegroundColor Magenta
}

process {
	$repoRoot = Join-Path $PSScriptRoot "..\.."
    $publishRoot = Join-Path $repoRoot 'Publish'
    $publishDir = Join-Path $publishRoot 'ML.ApplicationLauncher'
    $workDir = Join-Path $publishRoot 'work'
    $projectRoot =  $repoRoot

    $projects = @("ML.ApplicationLauncher.Shell", "ML.ApplicationLauncher.Shell.Admin")

    
    @($publishDir, $workDir) | ForEach-Object {
		$path = $_
		
		if (Test-Path $path -ErrorAction SilentlyContinue)
		{
			Remove-Item -Path $path -Recurse -Force
		}
		
		New-Item -Path $path -ItemType Directory | Out-Null
	}

    $index = 0
    $count = $projects.Count
    $projects | ForEach-Object {
        $projectFileName = "$_.csproj"
		$project = "$_\$projectFileName"
        $index += 1
        Write-Host "Publishing $project ($index/$count)" -ForegroundColor Magenta

        $projectFilePath = Join-Path $projectRoot $project
		. dotnet publish $projectFilePath -c $Config -o $publishDir
    }

    Remove-Item -Path $workDir -Recurse -Force
}

end {
    Write-Host "Done" -ForegroundColor Magenta
}

