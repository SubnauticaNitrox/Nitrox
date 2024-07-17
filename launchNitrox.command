cd "$(dirname "$0")"
if [ -f "Nitrox.Launcher" ]; then 
	./Nitrox.Launcher
else
	./Nitrox.Launcher/bin/Release/Nitrox.Launcher
fi
