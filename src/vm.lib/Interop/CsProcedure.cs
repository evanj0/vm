using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vm.lib.Memory;

namespace vm.lib.Interop;

public ref struct CsProcedureContext
{
    public CsProcedureContext(Span<Word> @params)
    {
        Params = @params;
        _returnValue = Word.Zero();
    }

    public Span<Word> Params;
    private Word _returnValue;

    public Word ReturnValue { get => _returnValue; }

    public void Return(Word value)
    {
        _returnValue = value;
    }
}

public interface ICsProcedure
{
    public int ParamCount { get; }
    public void Run(ref CsProcedureContext ctx);
}

public class CsProcedureException : Exception
{
    public CsProcedureException(string message, ICsProcedure throwingProcedure) : base(message)
    {
        ThrowingProcedure = throwingProcedure;
    }

    public ICsProcedure ThrowingProcedure { get; }
}