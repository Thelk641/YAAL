YAAL comes with a few useful command line arguments. They each follow the same pattern, "--name setting". More are planned, but these were the ones I wanted to have for my 1.0.

### "--restore"

Automatically restores both apworlds and any backedup file. Very useful if you somehow got an error during Backup or set it wrong. Note that this doesn't stop the normal launch process, so you can integrate it with the next arguments if you want to restore only on launch, or just use it on its own to automatically restore and then open YAAL normally.

### "--slot *slotName*" and "--async *asyncName*"

Together, these let you start a slot from an async without having to go through the UI as long as the asyncName is correct, and this particular async does contain a slot named slotName.

### "--launcher *launcherName*"

Used on its own, this will open the CLMaker window directly set on this particular launcher. It also has another use : if you use it and also give it an async and slot (see above), you will start this slot using launcher named launcherName.

### "--exit"

Closes the software as soon as its read. Useful if you want to --restore and then immediately quit.

### "--debug"

Prints all my debug.WriteLine() to a file. Might be useful down the line ?
