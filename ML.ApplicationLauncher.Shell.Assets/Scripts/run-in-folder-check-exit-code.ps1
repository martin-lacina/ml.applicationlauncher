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
    $skipLoggingExitCode = $false
}

process {
    try {
        $commands | ForEach-Object {
            $skipLoggingExitCode = $false
            Push-Location $workdir
            try {
                Write-Host $_ -ForegroundColor Magenta
                Invoke-Expression $_
                $exitCode = $LASTEXITCODE
                if ($null -eq $exitCode)
                {
                    $exitCode = 0
                    $skipLoggingExitCode = $true
                }
            }
            catch {
                $exitCode = -1
                $skipLoggingExitCode = $true
                Write-Error $_
            }
            Pop-Location

            if ($exitCode -ne 0)
            {
                if (-not $skipLoggingExitCode)
                {
                    Write-Host "Exit code: $exitCode" -ForegroundColor Magenta
                }
                Write-Error "Command failed"
                throw "Command failed"
            }
        }
    }
    catch {
        # Nothing to do, just finish
    }
}

end {
    Write-Host '------------------------------------------------------------'
    if (-not $skipLoggingExitCode)
    {
        Write-Host "Exit code: $exitCode" -ForegroundColor Magenta
    }
    Write-Host -NoNewLine 'Press any key to continue...';
    $null = $Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown');
}
