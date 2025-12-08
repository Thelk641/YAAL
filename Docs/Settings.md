# Settings are variable

From a code point of view this is obvious, but it's also true from a user point of view. In an instruction, if you write ${settingName} and settingName does exist, it will be replaced by its value when the launcher is executed. This is done recursively, so you should be able to have settings containing other settings if you want to.

While writing text may be the best way to input most settings, for some YAAL can switch to better input method, by pressing the cogwheel icon next to a setting it'll try to find said better input (true/false method or color input). Pick "Manual" to go back to text from binary, click the cogwheel again to go back to it from the color input mode.

# Type of Settings and read order

In YAAL, settings are divided in four groups :

- General Settings, set using the "Settings" button on the main window
- Async and Slot settings, via the main window directly
- Launcher Settings, using the "Settings" button in the CLMaker window 
- Temporary Settings, via the RegEx instruction in a launcher

Before executing a custom launcher, the first three categories will be read, **in that order**. This means that if you set a Launcher Setting to the same name as a General Setting, it will override it, letting you for example define a general aplauncher, and then define a different one in a launcher that is using a different Archipelago version. 

If you create a new setting in the general settings manager, it'll be treated as a General Setting, while if you do it on the launcher settings manager, it'll be treated as a Launcher Setting. Settings set by the RegEx instruction override everything else.

# Default settings name

These are all used somewhere in YAAL's code, and therefore can't be erased or renamed. Some are automatically filled for you, to help you while you set your launchers. You can find the list in YAAL by opening the Setting Manager from a CLMaker window.

### General Settings

- aplauncher : necessary for the Patch command to work
- sharpness multiplier : used by the custom theme maker
- zoom : scales every window, defaults to 1

- backgroundColor : used for the entire app's theme
- foregroundColor : used for the entire app's theme
- buttonColor : used for the entire app's theme

These are all shortcuts automatically set for you. Every "lua_" shortcut is replaced by "--lua="apfolder/data/lua/connector_name.lua""
- apfolder : set when aplauncher is changed
- lua_adventure
- lua_bizhawk
- lua_ladx
- lua_mmbn3
- lua_oot
- lua_tloz

### Async Settings

- asyncName
- cheeseURL : if you set a roomURL, automatically provides the cheesetracker link for that room
- password : default to None, which works if no password was set
- room : "archipelago.gg:12345"
- roomAddress : set when room setting is changed (if the room is "archipelago.gg:12345", this will be "archipelago.gg")
- roomPort : set when room setting is changed (if the room is "archipelago.gg:12345", this will be "12345")
- roomURL

### Slot Settings

- baseLauncher (see Tools.md)
- connect : gets automatically replaced by "--connect slotName:password@roomAddress:roomPort"
- patch
- rom : set by the Patch instruction
- slotName
- slotInfo : automatically replaced by "slotname:password@roomAddress:roomPort"
- slotTracker : if the roomURL is set, contains the link to the webtracker for this slot
- version

### Launcher Settings

- apworld : set by going through all the Apworld instructions (plus YAAL.apworld, if you have a Patch instruction), this is a list, see the Custom Launcher documentation for more info on that
- filters
- gameName
- githubURL
- launcherName : set in the CLMaker window directly, not in the settings manager
