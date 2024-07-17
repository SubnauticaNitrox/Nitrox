cd "$(dirname "$0")"
xattr -r -d com.apple.quarantine ./
cp steam_appid.txt ~/Library/Application\ Support/Steam/steamapps/common/Subnautica
