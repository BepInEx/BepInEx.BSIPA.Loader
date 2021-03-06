# BepInEx.BSIPA.Loader

This project allows you to load BSIPA plugins via BepInEx.

Please note that *no* game .dll files are included in this repository.

### Benefits

- Easy drag & drop install
- No patching of game assemblies performed at all
- Access to powerful preloader patchers, that allow Mono.Cecil editing
- Most native Win32 calls have been replaced with managed equivalents

### Limitations

- Currently not able to load plugins with native libraries
- No mod assistant support. You will have to manually move files from the Staging directory to your installation
- No .NET 3.5 version


## Installation

1. Install [BepInEx](https://bepinex.github.io/bepinex_docs/v5.0/articles/user_guide/installation.html) into your game directory.
2. Download BepInEx.BSIPA.Loader from the releases page, and extract the folder so it overwrites.
3. You're done!

## Build instructions

1. Replace the project references to UnityEngine.dll and UnityEngine.CoreModule.dll to the libraries in your game installation.
2. Build

Build artifacts will be located in the `/bin` directory

### Licenses / Credits

BepInEx.BSIPAVirtualizer is a modified version of BepInEx.IPAVirtualizer from [IPALoaderX](https://github.com/BepInEx/IPALoaderX), which is under the MIT license.

BSIPA.Loader is a modified version of the IPA.Loader project from BSIPA (link pending), which is under the MIT license.