# Settings are variable

From a code point of view this is obvious, but it's also true from a user point of view. In an instruction, if you write ${settingName} and settingName does exists, it will be replaced by its value when the launcher is executed. This is done recursively, so you can have settings containing other settings and in fact, one of the default one is already setup that way.

# Type of Settings and read order

In YAAL, settings are divided in four groups :

- General Settings, set using the "Settings" button on the main window
- Async and Slot settings, via the main window directly
- Launcher Settings, using the "Settings" button in the CLMaker window 
- Temporary Settings, via the RegEx instruction in a launcher

Before executing a custom launcher, the first three category will be read, **in that order**. This means that if you set a Launcher Setting to the same name as a General Setting, it will override it, letting you for example define a general aplauncher, and then define a different one in a launcher that is using a different Archipelago version. 

If you create a new setting in the general settings manager, it'll be treated as a General Setting, while if you do it on the launcher settings manager, it'll be treated as a Launcher Setting. Settings set by the RegEx instruction override everything else.

# Default settings name

These are all used somewhere in YAAL's code, and therefore can't be erased or renamed. Some are automatically filled for you, to help you while you set your launchers. You can find the list in YAAL by opening the Setting Manager from a CLMaker window.

### General Settings

- apfolder : automatically set when aplauncher is changed
- aplauncher

### Async Settings

- asyncName
- password : default to None, which works if no password was set
- room
- roomIP : set when room setting is changed (if the room is "archipelago.gg:12345", this will be "archipelago.gg")
- roomPort : set when room setting is changed (if the room is "archipelago.gg:12345", this will be "12345")

### Slot Settings

- baseLauncher (see Tools.md)
- patch
- rom : set by the Patch instruction
- slotName
- slotInfo : defaults to "${slotName}:${password}@${room}"
- version

### Launcher Settings

- apworld : set by going through all the Apworld instructions (plus YAAL.apworld, if you have a Patch instruction), this is a list, see the Custom Launcher documentation for more info on that
- filters
- gameName
- githubURL
- launcherName : set in the CLMaker window directly, not in the settings manager
