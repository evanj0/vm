using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vm.lib.Interop;

namespace vm.std;

public static class Process
{
    public class Sleep : ICsProcedure
    {
        public int ParamCount => 1;

        public bool ReturnsValue => false;

        public void Run(ref CsProcedureContext ctx)
        {
            var time = ctx.Params[0].ToI64();
            Thread.Sleep((int)time);
        }
    }
}
