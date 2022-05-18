using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vm.lib.Memory;

namespace vm.lib.Interop;

public ref struct CsProcedureContext
{
    public Vm Vm { get; set; }
    public Heap Heap { get; set; }
    public int ParamCount { get; set; }

    public Word GetParam(int index)
    {
        return Vm.Stack.Index(Vm.Stack.Sp - ParamCount + index);
    }

    public void Return(int index)
    {
        Vm.Stack.;
    }
}

public interface ICsProcedure
{
    public int ParamCount { get; }
    public void Run();
}

public abstract class CsProcedure : ICsProcedure
{
}