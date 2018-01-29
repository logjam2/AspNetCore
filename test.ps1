#------------------------------------------------------------------
# Powershell build script to run all tests in test projects - used by appveyor.
#------------------------------------------------------------------

"Configuration: $env:Configuration"
" "

# Helper function, throws when an external executable returns a non-zero exit code.
function Exec([scriptblock]$cmd, [string]$errorMessage = "Error executing command: " + $cmd) {
  & $cmd
  if ($LastExitCode -ne 0) {
    throw $errorMessage
  }
}

$testProjects =  ( "LogJam.Extensions.Logging.Tests" )
foreach ( $testProject in $testProjects) {
  Push-Location ".\test\$testProject"
  try {
      if ($env:Configuration) {
          exec { dotnet xunit -nobuild -configuration $env:Configuration $testProject.csproj }
      }
      else {
          exec { dotnet xunit -nobuild $testProject.csproj }
      }
  }
  finally {
      Pop-Location
  }
}

