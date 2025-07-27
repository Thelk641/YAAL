When playing Archipelago games, there might be some non-games you'd like to be able to open like the Text Client or Universal Tracker. For these, instead of having to switch slot's game around, YAAL lets you define "tools", "meta-launchers" with versions set per-async instead of per-slot, that can be used on any slot. Creating tools is done in the same window as other custom launchers, you just need to set the launcher type to "Tool" in the bottom left corner.

To help you with your tool creation, YAAL also brings some meta-settings :
- ${base:apworld} : replaced by the list of apworld of the base launcher, the one set as this slot's game
- ${baseSetting:settingName} : replaced by the value of settingName in the base launcher

The Isolate instruction will automatically include the base apworlds if used on a Tool.

Tools don't launch the base launcher while launching, so you can't access a base launcher's keyed process or get the output of a RegEx instruction from the base launcher in a Tool. 
