using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vm.lib
{
    public struct ProcInfo
    {
        public ProcInfo(int addr, int numParams, int numLocals)
        {
            Addr = addr;
            NumParams = numParams;
            NumLocals = numLocals;
        }

        public int Addr;

        public int NumParams;

        public int NumLocals;
    }
}
