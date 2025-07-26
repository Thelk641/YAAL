# Custom Launcher settings

Using the "Settings" button in the CLMaker window, you can set some launcher-specific settings. These will be read after the General and Async / Slot Settings, and will override them. Launchers comes with the following settings by default :

- launcherName : This is what you'll see on the main window, it's used to identify this particular launcher and can be set to anything that you'd find meaningfull

- gameName : This should be set to the game's name as it appears in yaml. Right now, this is only used if you want to copy a patch (for example, for Factorio, the patch is a mod you need to copy in two different folders), if you do, the launcher will go through every launcher and, if they share this gameName, tell them that a new patch has been put there, letting them know that if they want to open, they'll need to remove it first (this can be useful if, for example, a game's apworld has two completely different version, like a normal one and someone's fork, and you've set two different launchers to each of those branches)

- githubURL : Set this to the /releases page of this game's apworld's dev to be able to automatically check for update and download said update

- filters : This lets you define what, in a release, you'll want to download, instead of downloading every single file. There are two ways to define a filter, you can put the full filename (ex : S3AP.zip) or extensions (ex : .apworld)

You can also set custom variable. Do note that, if you set a variable to "True" or "False", the next time you open the Settings Manager, the text field will be replaced by a dropdown. If that happens and wasn't intended, go into YAAL/ManagedApworlds/LauncherName/launcher.json and edit that setting manually.

# Game or tool ?

At the bottom left of the CLMaker window is a setting to note if a launcher is a game or a tool. Each slot is meant to have only one game, called its base launcher (ex : Minecraft, Factorio etc.), while tools are meant as things that can be openned on any slot (ex : Text Client, Universal Tracker etc.). For versionning, games' version are set per-slot, tools' version are set per-async.

If you're making a tool, there are a few more settings you can use :
- ${base:apworlds} will be replaced with the base launcher's apworld list
- ${baseSetting:settingName} will be replaced by the value of settingName in the base launcher's settings, you do not need to put ${} around setting name, YAAL does it for you

# Instructions

If an instruction has a text field, it can take multiple input separated by a semicolon (";"). The only exception is RegEx, as you might want to replace those, so this one uses "/;" as separator instead, meaning it can't use ${apworld}.

## - Apworld
###  Defines and apply versionning to a file (including but not limited to .apworld)

The "Stricly necessary" tickbox is only relevant if the file can't be found. If ticked, the launcher will error out, if not, it'll ignore this file and move on.

Do note that, when replacing a file to the Slot's version, it will delete the file that was there before. This is the only instruction allowed to do that, and it will only do that to replace a file, it won't ever delete a file it can't immediately replace.

### List limitations
None.

## - Backup
###  Create per-slot versions of files or folders, making per-slot safe environments

The files or folders that you target, if they exist when launching, will be saved on the side and be auto-restored once the auto-restore condition triggers, letting you create per-slot files or folders. No more switching mods around if you've got two different slots with different mods.

For the auto-restore, this provides four options (see Open to learn more about keyed process) :
- Keyed process exit
- Keyed process' output contains X
- Combined : will trigger once the first of the two conditions above triggers
- Timer : auto-restore after X seconds have passed
- Off : please not that this will turn off most of the safeties !

You can also set a "default file", this is your "vanilla" version of the file, the one that will be used on a slot that doesn't have its own version of the backup target yet.

Because of how dangerous moving and renaming file can be, this comes with a few safeties (EXCEPT IF YOU SET AUTO-RESTORE TO OFF). In YAAL/ManagedApworlds, you'll find a backupList.json file which will tell you which file is currently being backedup, and if anything goes wrong while executing a custom launcher, it will try to auto-restore immediately to ensure that no file gets left behind. If it still happens for some reason, you can launch YAAL with the command lines "--restore --exit" to try again to auto-restore. If you need to access old backups, for example if a save got corrupted and you want to get back the one from your previous play session, they're not deleted, just moved to YAAL/Async/AsyncName/SlotName/Backup.old, this does make it take a ton of space if you're backing up heavy files, but I feel like this safety is worth the disk space.

### List limitations

Every text field of this instruction can accept multiple input, with one limitation : if you set more than one default file, you must set one for each of your backup target. Do note that process output will be split using semicolons (";") as the divider between multiple inputs, so make sure it's not in the output you're setting if you only want to auto-restore on all of it being found.

## - Isolate

### Backup and restore custom_worlds and lib/worlds, leaving behind only the bare minimum and this launcher's apworlds, launching Archipelago as fast as possible

The options are the same as Backup, see above for more informations.

### List limitations
None.

## - Open
### Launchs a software, opens a file or URL

While this has two fields, one for the file and one for arguments, you can put the arguments after the file in the same text field if you want to, it'll take care of splitting them automatically. You cannot do it the other way around.

The "Variable name" field lets you create a keyed process. These process can be followed by Backup and Isolate instructions to trigger their respective auto-restore. For this, the order of instructions doesn't matter, Backup and Isolate check if any existing keyed process matches their key and Open checks if any instruction is looking for its key.

This comes with a few limitations :
- While you can use Open to open a URL (and it will open it in your default browser), you cannot key a URL, for technical reasons (YAAL needs the OS to take care of opening a URL, which means it can't read the process output, and tracking the process' exit would be awkward)
- .lnk files are allowed only on Windows, these files are Windows-only anyway, but they're a pain to work with on non-Windows OS so for now, the only solution I've found is to ban you from opening them with YAAL, one day I plan on solving this, but it'll have to wait

### List limitations
If you provide more than one key, you must provide one key per file to open.

## - Patch

### Apply or copys patch files
Applying the patch uses the YAAL.apworld, which you must put in custom_worlds for this to work (it will error out if you don't). The "Isolate apworlds" tickbox does the same thing as the Isolate instruction does, it auto-restores as soon as the patching is done.

Auto-rename is off by default, as some game (ex : Factorio) don't tolerate patch renaming. It is there for Bizhawk : if you wanted to backup your rom game save (just in case they got corrupted at some point), you'd need to get the name of the save, but annoyingly Bizhawk saves names are not always one-to-one with rom names (in particular, it replaces underscores with spaces). Using RegEx to solve this is a solution, but then you'll need to apply it only to the filename and then use another RegEx to put them back together, instead, YAAL lets you automatically renames them to AsyncName.SlotName, which, as long as you don't use underscore in either of those, should be consistent with Bizhawk's saves names.

CopyPatch will first check if a patch exists in the target folder(s), and delete them if they do, ensuring that you only ever have one patch copies there. The information of "what is the name of the last patch copied to X ?" will be saved and transmitted to every other Custom Launcher that share this launcher's gameName.

### List limitations
None.

## - RegEx
### Lets you edit strings or files using Regular Expressions
Not much to say, Regular Expressions are very powerful, but also pretty complicated, be sure that you know what you're doing with them.

### List limitations
This currently doesn't use the standard semicolon (";") as divider, instead using "/;" as the input, replacement and output might contain semicolons themselves.

You are allowed to :
- Give as many source input or source strings as you want
- Give 1 pattern to look for in each source, or one pattern per source, no more, no less
- Give 1 replacement to use for each source, or one replacement per source, no more, no less
- Give 1 output file (everything will be concatenated in it), or at least one per source (if you give more than that, they'll be ignored)
- Give 1 output variable (everything will be concatenated in it), or at least one per source (if you give more than that, they'll be ignored)
