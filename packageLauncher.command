cd "$(dirname "$0")"
if [ -d NitroxRelease ]; then
	rm -r NitroxRelease
fi

rsync -av \
	--exclude="Nitrox Logs" \
	--exclude="screenshots" \
	Nitrox.Launcher/bin/Release/ NitroxRelease

cp launchNitrox.command NitroxRelease
cp whitelistNitrox.command NitroxRelease

cp steam_appid.txt NitroxRelease

find NitroxRelease/runtimes -mindepth 1 -maxdepth 1 ! -name 'osx' ! -name '.' -exec rm -rf {} +

find NitroxRelease -type f -name '.DS_Store' -exec rm -f {} +

zip -r NitroxMacOSPreRelease.zip NitroxRelease
