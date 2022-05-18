using System.Text;

namespace asm.lib.macro;

public class Macro
{
    public Macro(IEnumerable<IGenerator> generators, ArgGenerator[] args, VarargsGenerator varargs)
    {
        _generators = generators;
        _args = args;
        _varargs = varargs;
        _argIndex = 0;
    }

    private IEnumerable<IGenerator> _generators;
    private readonly ArgGenerator[] _args;
    private readonly VarargsGenerator _varargs;
    private int _argIndex;

    public string Render()
    {
        var sb = new StringBuilder();
        foreach (var generator in _generators)
        {
            sb.AppendLine(generator.Generate());
        }
        return sb.ToString();
    }

    public void PushArg(IGenerator generator)
    {
        if (_argIndex < _args.Length)
        {
            _args[_argIndex].Internal = generator;
            _argIndex++;
        }
        else
        {
            _varargs.Arguments.Add(new ArgGenerator(generator));
        }
    }

    public void Reset()
    {
        foreach (var arg in _args)
        {
            arg.Internal = new EmptyGenerator();
        }
        _varargs.Arguments.Clear();
        _argIndex = 0;
    }
}
