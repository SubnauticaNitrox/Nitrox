# Publishing Nitrox to Flathub and AppImageHub

## Flathub (Flatpak)

1. Fork the [flathub/flathub](https://github.com/flathub/flathub) repository on GitHub.
2. Clone your fork locally:
   ```bash
   git clone https://github.com/<your-username>/flathub.git
   ```
3. Run the publish script:
   ```bash
   NitroxPackaging/publish-flathub.sh /path/to/your/flathub
   ```
4. Open a Pull Request from your fork to the main Flathub repo.
5. Flathub maintainers will review and merge your PR, making Nitrox available on Flathub.

## AppImageHub (AppImage)

1. Build your AppImage using the provided script.
2. Upload the AppImage to your GitHub Release using the publish script:
   ```bash
   NitroxPackaging/publish-appimage.sh <release-tag>
   ```
   (Requires GitHub CLI: https://cli.github.com/)
3. Submit your release URL to [AppImageHub](https://appimage.github.io/submit/) for listing.

---
For automation, these scripts can be added to your CI/CD pipeline after a successful release build.
