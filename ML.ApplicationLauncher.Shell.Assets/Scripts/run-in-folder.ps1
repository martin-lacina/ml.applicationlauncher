[CmdletBinding()]
param (
    [string]$Base64Context
)

begin {
    $context = [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($Base64Context)) | ConvertFrom-Json
    $title = $context.Title
    $workdir = $context.WorkingDirectory
    $commands = $context.Commands
    Write-Host "Processing: $title" -ForegroundColor Magenta
    Write-Host "Working Directory: $workdir"
    Write-Host "Commands:"
    $commands | ForEach-Object { "  $_" }
    Write-Host '------------------------------------------------------------'
    $exitCode = 0
}

process {
    $commands | ForEach-Object {
        Push-Location $workdir
        Invoke-Expression $_
        $exitCode = $LASTEXITCODE
        Pop-Location
    }
}

end {
    Write-Host '------------------------------------------------------------'
    Write-Host "Exit code: $exitCode" -ForegroundColor Magenta
    Write-Host -NoNewLine 'Press any key to continue...';
    $null = $Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown');
}
