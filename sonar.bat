:: Shell script to execute build and analysis of the Nitrox repository.
:: It requires the SonarScanner for MSBuild.
:: For instructions see https://docs.sonarqube.org/display/SCAN/Analyzing+with+SonarQube+Scanner+for+MSBuild
:: You may need to install additional .NET SDK's and targeting packs for commandline builds.
:: Note: an account with access to the SubnauticaNitrox org on SonarCloud is also required.


@ECHO OFF
IF EXIST "sonar.config.bat" GOTO RunSonar

REM sonar.config.bat does not exist. See sonar.config.example.bat
PAUSE
GOTO END


:RunSonar

CALL "sonar.config.bat"

ECHO Sonar prepare phase
%sonar%SonarScanner.MSBuild.exe begin /k:"%sonarkey%" /d:sonar.organization="%sonarorg%" /d:sonar.host.url="%sonarurl%" /d:sonar.login="%token%"

ECHO Solution build for analysis
%msbuild%MsBuild.exe /t:Rebuild

ECHO Sonar processing phase
%sonar%SonarScanner.MSBuild.exe end /d:sonar.login="%token%"

PAUSE


:END
