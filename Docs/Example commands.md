# Open Instruction

Remember that Open treats both its "file to open" and "arguments" field as a single one, so you can cut these between the two or just put them in the "file to open" field.

## Archipelago clients

*${aplauncher} -- "client name" ${connect}*

(write the client name as it's shown on the Archipelago Launcher, so "Bizhawk Client", "Starcraft 2 Client" etc.)

## Bizhawk (generic)

*/path/to/EmuHawk.exe ${lua_bizhawk} ${rom}*

If you need another lua file, exchange "lua_bizhawk" with "lua_sni" for example. The "lua_" shortcuts are automatically generated based on your aplauncher setting, there's one per lua file in /data/lua/ plus "lua_sni" for /SNI/lua/Connector.lua.

## Poptracker

Command line args : https://github.com/black-sliver/PopTracker/blob/master/doc/commandline.txt

"Just use --list-installed to list all of your current packs, and launch the program with --load-pack <uid> to launch Poptracker with the one you want!

Before that, you can also edit PopTracker.json with RegEx to change the slot and address, so it's automatically filled when you open the AP prompt!" (thanks Grayson for this knowledge !)

## Yacht Dice

*https://yacht-dice-ap.netlify.app/?hostport=${room}&name=${slotName}&go=y&p=AP*

# RegEx Instruction

## Find the room

In the "regular expression" field : *localhost|archipelago.gg:\d+*

## Find a patched rom's save file

Bizhawk sadly isn't friendly with us when trying to backup patched game save, thanksfully it's only necessary if you're scared of hard lock or corruption, but still. Here's how to do it :

First, find the SaveRAM folder for this particular console, for example Bizhawk/Gameboy/SaveRAM, then add in a RegEx instruction :

In the "regular expression" field : *^.*\([^\\]+).gbc$*
In the "replacement" field : *path/to/SaveRAM/$1.SaveRAM*
Store it in a variable, give it the name you want, for example "save".

Then, add a second RegEx instruction which takes your variable as input :

In the "regular expression" field : *_*
In the "replacement field" : *" "*
Store it in a variable, you can reuse the same as before if you want to, we don't need its original value anymore.

Now if you backup ${yourVariable} (so in this example, ${save}), it'll backup that game's save file !
