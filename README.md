# Like A Domino | WPF Edition

A modern, tokyo-neon themed domino game built with WPF (Windows Presentation Foundation) featuring immersive audio, sleek tokyo-neon inspired visuals, and support for 2-4 players.

## 🚀 Getting Started

### Prerequisites
- Windows 10/11
- .NET Framework 4.8 or higher
- Visual Studio 2019+ (for development)

### Dependencies
- **NAudio**: Audio playback and processing
- **WPF**: User interface framework

### Installation
1. Clone the repository:
   ```bash
   git clone [repository-url]
   cd DominoWPF
   ```

2. Restore NuGet packages:
   ```bash
   nuget restore
   ```

3. Build the solution:
   ```bash
   msbuild DominoWPF.sln
   ```

4. Run the application:
   ```bash
   ./bin/Debug/DominoWPF.exe
   ```

## 🎯 How to Play

### Game Setup
1. Launch the application
2. Select number of players (2-4)
3. Enter player names
4. Set maximum score to win
5. Click "PLAY!" to start

### Gameplay
- Players take turns placing dominoes on either end of the chain
- Dominoes must match the values at the chain ends
- Use "Place Left" or "Place Right" buttons to position your domino
- First player to reach the maximum score wins
- Game automatically handles blocked situations

### Controls
- **Mouse**: Click dominoes to select, use placement buttons
- **Audio Settings**: Click 🔊 icon to adjust volume levels

## 🎨 Customization

### Adding Voice Lines
1. Create folder structure: `Sounds/player[N]/[voice_type]/`
2. Add audio files (.wav, .mp3, .aiff)
3. Voice types: "win", "lose", "turn", "block", etc.
4. Files are automatically detected and randomly selected

### Modifying Themes
- Edit brush resources in XAML files
- Customize gradients in `Window.Resources`
- Adjust glow effects and animations
- Replace background images and textures

### Audio Configuration
```csharp
// Adjust default volumes
audioManager.MasterBgmVolume = 0.5f;   // 50% BGM
audioManager.MasterSfxVolume = 1.0f;   // 100% SFX  
audioManager.MasterVoiceVolume = 1.0f; // 100% Voice
```

## 🔧 Development

### Building from Source
1. Open `DominoWPF.sln` in Visual Studio
2. Ensure NAudio NuGet package is installed
3. Add custom fonts to `/fonts/` directory
4. Configure audio assets in `/sounds/` structure
5. Build and run (F5)

### Class Diagram
See `diagram/diagram.mmd` for complete class relationships and interfaces.

### Code Style
- Follow C# naming conventions
- Use interfaces for core game components
- Implement proper disposal for audio resources
- Maintain XAML formatting standards

## 📝 License

This project is created for educational purposes. Custom fonts and audio assets may have their own licensing requirements.

## 🐛 Known Issues

- Audio files must be in supported formats (WAV, MP3, AIFF)
- Custom fonts must be properly installed/embedded
- Windows-only compatibility due to WPF dependency
- Margin stuff

## 🙏 Special Thanks

This project uses fan assets purely for non-commercial and educational purposes. Credit goes to:

* 🎮 **Valve** — L4D2 character voices
* 🎵 **Atlus** — Persona 3 Reload OST
* 🔊 **FromSoftware** — Sekiro UI sound effects
* 🐉 **SEGA** — Yakuza 0 character lines, fonts, logo and wallpaper
