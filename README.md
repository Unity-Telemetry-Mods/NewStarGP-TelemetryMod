# New Star GP Telemetry Mod

## Instructions

### Normal Install
1. Install BepinEx v5 [link](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.23.2) into your game folder.     

2. Place plugin dll file into `BepinEx/plugins` folder

### UUVR install
1. Download RaiPal [click here](https://github.com/Raicuparta/rai-pal/releases) 
2. Scan for your games, then click on your game.
3. Install **UUVR Mono Modern** (which includes BepinEx)
4. Click the 3 dots next to **UUVR Mono Modern** and then click `Open Mod Folder` (the folder will look like `%APPDATA%\raicuparta\rai-pal\data\installed-mods\[ModId]\`)
5. Place Telemetry Mod dll into `BepinEx/plugins` folder 


## Building
1. Clone this repository

2. This project also references a project named SharedLib (located [here](https://github.com/Unity-Telemetry-Mods/SharedLib))

    - Check out SharedLib folder beside this project

    - folder structure should look like 
    ```
    [This Project]\
    SharedLib\
    ```

3. The following files will need to be copied from

    `New Star GP\release\NSGP_Data\Managed\`

    to
    
    `[Project]\lib`     
    - netstandard.dll
    - Assembly-CSharp_publicized.dll **

4. Open sln file located inside Project Folder

---
** *You will need to publicize the Assembly-CSharp file from game using a tool like [Assembly Publicizer](https://github.com/CabbageCrow/AssemblyPublicizer/releases)*

## Changelog

v 1.0 First Release