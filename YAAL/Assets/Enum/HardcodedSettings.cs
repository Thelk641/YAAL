using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAAL
{
    public enum HardcodedSettings
    {
        //cache_previousSlot
        previous_async,
        previous_slot,

        //cache_customLauncher
        launcherName,
        apworld,

        //cache_async
        asyncName,
        room,
        roomAddress,
        roomPort,
        password,

        //cache_slot
        connect,
        slotName,
        slotInfo,
        patch,
        rom,
        version,

        //folder helpers
        apfolder,
        lua_adventure,
        lua_bizhawk,
        lua_ladx,
        lua_mmbn3,
        lua_oot,
        lua_tolz,
    };
}
