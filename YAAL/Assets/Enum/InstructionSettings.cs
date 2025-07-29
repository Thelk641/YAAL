using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAAL
{
    public enum ApworldSettings
    {
        apworldTarget,
        necessaryFile
    }

    public enum BackupSettings
    {
        processName,
        target,
        outputToLookFor,
        timer,
        modeSelect,
        defaultFile
    }

    public enum IsolateSettings
    {
        processName,
        outputToLookFor,
        timer,
        modeSelect,
    }

    public enum OpenSettings
    {
        args,
        processName,
        path
    }

    public enum PatchSettings
    {
        mode,
        target,
        optimize,
        rename
    }

    public enum RegExSettings
    {
        targetFile,
        targetString,
        modeInput,
        modeOutput,
        regex,
        replacement,
        outputFile,
        outputVar
    }
}
