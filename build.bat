REM Unit tests
dnx -p test\Toolbox.Auth.UnitTests\Toolbox.Auth.UnitTests test
if not "%errorlevel%"=="0" goto failure

REM Package
mkdir Build
dnu pack "src\Toolbox.Auth\Toolbox.Auth" --configuration Release
if not "%errorlevel%"=="0" goto failure

:success
exit 0

:failure
exit -1