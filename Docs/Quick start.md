# How to use YAAL

To launch YAAL you don't need to unzip anything, the entire software is in a single file. If it can't find its userSetting.json file in the same folder it will create it and every folder it needs, so if you ever need a brand new install, just run it in an empty folder and it'll take care of the rest.

To use YAAL you need two things, neither is provided by me, but each are defined in the software itself :
- Launchers
- Slots to connect to

## Launchers

A launcher is a list of instructions which can be of different types :
- Apworld : ensure you're using the right version of the .apworld or any other version'd files
- Backup : create per-slot version of file or folder
- Display : show infos with an easy way to copy and paste them
- Isolate : optimize your apworld folder for faster load
- Open : lets you open anything, file, folder or URL
- Patch : apply or copy a patch file
- RegEx : edit text in files or variable

See [Custom Launchers](https://github.com/Thelk641/YAAL/blob/master/Docs/Custom%20Launcher.md) for more detailed information.

## Slots

To add a slot, first add an async, then add slots in it. If you add the room's URL to the async, slots will come with automatic patch downloading, cheese and web tracker URL, on top of being chosen from a drop-down menu instead of typing it manually. If you do not desire to go through the automated path, nearly all of this can be done manually in the UI, and the rest can be done via editing the multiworld.json file in the Async/AsyncName folder.

Once you've got your slot(s), you can pick which launcher and version is used for it, and you're done ! Press the left play button to try to launch the launcher, or use the play button in a box to launch the selected tool for that slot.
