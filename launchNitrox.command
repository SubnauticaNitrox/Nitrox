cd "$(dirname "$0")"
if [ -e "Nitrox.Launcher" ]; then 
	Nitrox.Launcher
else
	Nitrox.Launcher/bin/Release/Nitrox.Launcher
fi
