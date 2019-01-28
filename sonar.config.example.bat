:: Save this file as sonar.config.bat and add your configuration parameters.

:: SonarCloud access token. Note you need to have access rights to the SubnauticaNitrox Org.
SET token=

:: Path to MSBuild; leave blank if in PATH
:: Don't forget the trailing \
SET msbuild="c:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\amd64\"

:: Path to Sonar; leave blank if in PATH
:: Don't forget the trailing \
SET sonar=

:: Sonar information
SET sonarkey=subnautica-nitrox
SET sonarorg=subnauticanitrox
SET sonarurl=https://sonarcloud.io
