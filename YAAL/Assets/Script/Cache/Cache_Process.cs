using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAAL
{
    public class Cache_Process
    {
        public string key = "";
        public bool redirectOutput = false;
        private Process process;

        public Cache_Process(Process newProcess)
        {
            process = newProcess;
        }

        public void Start()
        {
            process.Start();
            if (ProcessManager.IsExecutable(process.StartInfo.FileName) && redirectOutput) 
            {
                process.BeginOutputReadLine();
            }
        } 
        

        public void Setup(string newKey)
        {
            if(newKey == null || newKey == "")
            {
                newKey = "Default";
            }
            this.key = newKey;
        }

        public Process? GetProcess()
        {
            return process;
        }

        public string GetKey()
        {
            return key;
        }
    }
}
