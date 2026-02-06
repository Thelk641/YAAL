
using System.Collections.Generic;
using System;

public class Cache_Versions
{
    // version > files downloaded
    public Dictionary<string, List<string>> downloaded = new();
    public Dictionary<string, List<string>> versions = new();
}