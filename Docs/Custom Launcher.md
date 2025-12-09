# General Information

## Settings

Using the "Settings" button in the CLMaker window, you can set some launcher-specific settings. These will be read after the General and Async / Slot Settings, and will override them. Launchers comes with the following settings by default :

- launcherName : This is what you'll see on the main window, it's used to identify this particular launcher and can be set to anything that you'd find meaningful

- gameName : This should be set to the game's name as it appears in yaml. This is used to filter launchers on the main window, but also to ensure two launchers for the same game are aware of each other : if you want to copy a patch (for example, for Factorio, the patch is a mod you need to copy in two different folders) the launcher will go through every launcher and, if they share this gameName, tell them that a new patch has been put there, letting them know that if they want to open, they'll need to remove it first (this can be useful if, for example, a game's apworld has two completely different versions, like a normal one and someone's fork, and you've set two different launchers to each of those branches)

- githubURL : Set this to the /releases page of this game's apworld's dev to be able to automatically check for updates

- filters : This lets you define what, in a release, you'll want to download, instead of downloading every single file. There are two ways to define a filter, you can put the full filename (ex : S3AP.zip) or extensions (ex : .apworld)

You can also set custom variable. Do note that, if you set a variable to "True" or "False", the next time you open the Settings Manager, the text field will be replaced by a dropdown. If that happens and wasn't intended, go into YAAL/ManagedApworlds/LauncherName/launcher.json and edit that setting manually.

## Game or tool ?

At the bottom left of the CLMaker window is a setting to note if a launcher is a game or a tool. Each slot is meant to have only one game, called its base launcher (ex : Minecraft, Factorio etc.), while tools are meant as things that can be opened on any slot (ex : Text Client, Universal Tracker etc.). For more information on tools, see Tools.md.

## Optimisation

In my experience, some apworld tend to make opening client or patching a rom very slow, for that reason I've added a way to optimize your folders : they're renamed and only the necessary file are copied back to a temporary folder. YAAL should be clever enough to just stack multiple optimization on top of each other, and only restore your original folders when the last optimization is finished. If a custom launcher breaks, it should auto-restore to ensure that it doesn't do any damage, but, in case it fails to do so, your folders can be found in your Archipelago folder, just renamed as custom_world_old and worlds_old.

## Conditions and more advanced features

Sadly, YAAL was made as a purely linear tool, instruction A is done, if it succeeds, instruction B is done and so on. There are no conditions, no fork, no jump, it's way easier to make it work like that but YAAL comes with one feature that would let you do this if you wanted to : whenever you open a game, YAAL just launches another instance of itself with the right command line arguments, which you also can do.

I know that "if you want it, do it yourself" is not exactly the answer you might be looking for, but it's sadly the best one I can provide.

## Security

YAAL is not allowed to delete file unless it's to replace them instantly. If you ask it to delete an async for example, it won't, it'll just move it to its "Trash" folder, same thing for slots or versions. That doesn't mean you should trust me or my code : especially when trying a new launcher, please backup your files manually before letting YAAL play with them.

Also, it's been said before but : launchers are a simple json file, they're easy to share, but, if you use someone else's launchers, at least take the time to ensure that you know what it does so you avoid nasty surprises.

# Instructions

If an instruction has a text field, it can take multiple inputs, written as "item1";"item2" and so on. You can add space in-between them, it'll clear them out. If you want to add an empty input, add a space in-between the quotes, " ", or it might bug out (the space will be removed during run time).

## - Apworld
### Function
Defines and apply versioning to a file (including but not limited to .apworld) until a condition is met

### Options and notes
The "Strictly necessary" tickbox is only relevant if the file can't be found. If ticked, the launcher will error out, if not, it'll ignore this file and move on.

Do note that, when replacing a file to the Slot's version, it will delete the file that was there before. This is the only instruction allowed to do that, and it will only do that to replace a file, it won't ever delete a file it can't immediately replace.

### List limitations
None.

## - Backup
### Function
Create per-slot versions of files or folders, making per-slot safe environments

### Options and notes
The files or folders that you target, if they exist when launching, will be saved on the side and be auto-restored once the auto-restore condition triggers, letting you create per-slot files or folders. No more switching mods around if you've got two different slots with different mods.

For the auto-restore, this provides four options (see Open to learn more about keyed process) :
- Keyed process exit
- Keyed process' output contains X
- Combined : will trigger once the first of the two conditions above triggers
- Timer : auto-restore after X seconds have passed
- Off : please not that this will turn off most of the safeties !

You can also set a "default file", this is your "vanilla" version of the file, the one that will be used on a slot that doesn't have its own version of the backup target yet. It will be copied into YAAL/ManagedApworlds/LauncherName/Default first before being used, to have it be safely kept on the side and be easier to cleanup.

Because of how dangerous moving and renaming file can be, this comes with a few safeties (EXCEPT IF YOU SET AUTO-RESTORE TO OFF). In YAAL/ManagedApworlds, you'll find a backupList.json file which will tell you which file is currently being backedup, and if anything goes wrong while executing a custom launcher, it will try to auto-restore immediately to ensure that no file gets left behind. If it still happens for some reason, you can launch YAAL with the command lines "--restore --exit" to try again to auto-restore. If you need to access old backups, for example if a save got corrupted and you want to get back the one from your previous play session, they're not deleted, just moved to YAAL/Async/AsyncName/SlotName/Backup.old, this does make it take a ton of space if you're backing up heavy files, but I feel like this safety is worth the disk space.

### List limitations
You are allowed to :
- Give as many inputs (files or folders to backup) as you want
- Set no default file, 1 default file/folder to use for every input, or one per input, no more, no less
- Set as many keys to look for as you want, it will attach to all process with each of the selected key
- Set as many outputs to look for as you want, the first one to be found will trigger the auto-restore

## - Display
### Function
Lets you show any text (including setting value). "Tag" are just showed as text, "value" are showed as a button that puts its text into your copy/paste buffer. Both the "tag" and "value" field are parsed with settings, so if you put something like ${slotInfo} in there it will be replaced with that slot's information.

### Options and notes
None.

### List limitations
None.

## - Isolate

### Function
Backup and restore custom_worlds and lib/worlds, leaving behind only the bare minimum plus this launcher's apworlds (or this launcher's and the base launcher's if used on a Tool, see Tool.md for more information)), letting you only load relevant apworlds and getting faster load time.

### Options and notes
The options are the same as Backup, see above for more informations.

### List limitations
None.

## - Open
### Function
Launchs a software, opens a file or URL

### Options and notes
While this has two fields, one for the file and one for arguments, you can put the arguments after the file in the same text field if you want to, it'll take care of splitting them automatically. You cannot do it the other way around.

The "Variable name" field lets you create a keyed process. These process can be followed by Backup and Isolate instructions to trigger their respective auto-restore. For this, the order of instructions doesn't matter, Backup and Isolate check if any existing keyed process matches their key and Open checks if any instruction is looking for its key.

This comes with a few limitations :
- if you're trying to open a URL, you can just give it in the first field, without any args, and it'll open. Be aware that, while this will work, it requires the OS to resolve it, which means you can't read its output, and I'm not sure you can look for process exit either, so just to be safe, I've added an error if you try to do so. **This can fail silently**, so, if you want to open a URL and use it as a keyed process to trigger auto-restores, give it the path to a browser and give the URL as an argument instead
- .lnk files are allowed only on Windows, these files are Windows-only anyway, but they're a pain to work with on non-Windows OS so for now, the only solution I've found is to ban you from opening them with YAAL, one day I plan on solving this, but it'll have to wait

Finally, you can ask it to redirect the output of whatever you're opening. This is only useful if you're using a Backup command reading said output, and only works with windowed applications as opening anything that isn't an application requires the OS to handle it (and therefore be responsible for the output). In 99% of case you won't need it.

### List limitations
If you provide more than one key, you must provide one key per file to open.

## - Patch

### Function
Apply or copys patch files

### Options and notes
This instruction has two modes :

- Applying the patch uses the YAAL.apworld, which you must put in custom_worlds for this to work (it will error out if you don't). The "Isolate apworlds" tickbox does the same thing as the Isolate instruction does, except if you tik it, it'll auto-restore as soon as the patching is done, doesn't matter if it is successful or not.

- CopyPatch will first check if a patch exists in the target folder(s), and delete them if they do, ensuring that you only ever have one patch copies there. The information of "what is the last patch copied to X ?" will be saved and transmitted to every other Custom Launcher that share this launcher's gameName, so if you have multiple launchers for the same game, they won't bug each other out.

The option to rename the patch is there for Bizhawk : if you wanted to backup your rom game save (just in case they got corrupted at some point), you'd need to get the name of the save, but annoyingly Bizhawk saves names are not always one-to-one with rom names (in particular, it replaces dashes with spaces). Using RegEx to solve this is a solution, but then you'll need to apply it only to the filename and then use another RegEx to put them back together, instead, YAAL lets you automatically renames them. Sadly, some game don't tolerate you renaming the patch, so for this, just leave it empty and it won't rename them.

If you use the "optimize" option, it will only optimize your apworld folders for the duration of the patch, they'll be restored as soon as it's done.

### List limitations
None.

## - RegEx
### Function
Lets you edit variable or files using Regular Expressions

### Options and notes
Pretty explicit. Here are two useful patterns to get you started :
- "^.*\\([^\\]+)\.gbc$" => will find and extract the fileName before the .gbc extension, set replacement to "$1.SaveRAM" to get bizhawk's save name, or add the full path to bizhawk's save folder before that and you'll get the full path, ready to be used with Backup to ensure a corrupted save doesn't force you to do it all again
- "localhost|archipelago\.gg:\d+" => will find the room address, be it "localhost" or "archipelago.gg:12345", in case you need to find and replace this in a mod file

### List limitations
You are allowed to :
- Give as many source input or source strings as you want
- Give 1 pattern to look for in each source, or one pattern per source, no more, no less
- Give 1 replacement to use for each source, or one replacement per source, no more, no less
- Give 1 output file, or at least one per source (if you give more than that, they'll be ignored)
- Give 1 output variable, or at least one per source (if you give more than that, they'll be ignored)

If you give more than one input and either a single output, or repeat the output multiple time, they'll be simply concatenated with a semicolon (";") in-between each result. Do note that this doesn't automatically adds quotes ("") around results, so if you want to chain them and need them to stay split (for example, reading two different files, making the same edit twice, then making a difference edit on each of them and saving them separately) you must output to different variable as the concatenated result won't match the criteria for automatic split.
