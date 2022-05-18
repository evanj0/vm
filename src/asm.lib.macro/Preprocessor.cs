using System.Text;

namespace asm.lib.macro;

public class PreprocessorException : Exception
{
    public PreprocessorException(string message) : base(message)
    {

    }
}
public class Preprocessor
{
    public Preprocessor()
    {
        _macros = new();
        _writeBuffer = new();
        _program = new();
        _localMap = new();
        // writing to program
        _writeBuffer = _program;
    }

    private Dictionary<string, Macro> _macros;
    private MacroBuilder? _macroBuilder;
    private StringBuilder _writeBuffer;

    private StringBuilder _program;
    private Dictionary<string, StringBuilder> _localMap;
    private Macro? _currentMacro;

    private void WriteToProgram()
    {
        _writeBuffer = _program;
    }

    public void Macro(string name)
    {
        if (_macroBuilder is null) _macroBuilder = new MacroBuilder(name);
    }

    public void MacroArg(string name)
    {
        if (_macroBuilder is null) throw new PreprocessorException("No active macro.");
        _macroBuilder.AddArg(name);
    }

    public void MacroPushText(string text)
    {
        if (_macroBuilder is null) throw new PreprocessorException("No active macro.");
        _macroBuilder.PushText(text);
    }

    public void MacroPushArg(string name)
    {
        if (_macroBuilder is null) throw new PreprocessorException("No active macro.");
        _macroBuilder.PushArg(name);
    }

    public void MacroPushVarargs()
    {
        if (_macroBuilder is null) throw new PreprocessorException("No active macro.");
        _macroBuilder.PushVarargs();
    }

    public void MacroEnd()
    {
        if (_macroBuilder is null) throw new PreprocessorException("No active macro.");
        if (_macros.ContainsKey(_macroBuilder.Name)) throw new PreprocessorException($"Macro with name `{_macroBuilder.Name}` already exists.");
        _macros.Add(_macroBuilder.Name, _macroBuilder.Build());
    }

    public void PushText(string text)
    {
        _writeBuffer.AppendLine(text);
    }

    public void PushMacroInit(string name)
    {
        if (!_macros.ContainsKey(name)) throw new PreprocessorException($"No macro with name `{name}` exists.");
        _currentMacro = _macros[name];
    }

    public void PushMacroArg(string text)
    {
        if (_currentMacro is null) throw new PreprocessorException($"No active macro initializtion.");
        _currentMacro.PushArg(new TextGenerator(text));
    }

    public void PushMacroArgLocal(string name)
    {
        if (_currentMacro is null) throw new PreprocessorException($"No active macro initializtion.");
        if (!_localMap.ContainsKey(name)) throw new PreprocessorException($"No local macro variable named `{name}`");
        var text = _localMap[name].ToString();
        _currentMacro.PushArg(new TextGenerator(text));
        _localMap.Remove(name);
    }

    public void SetLocal(string name)
    {
        if (!_localMap.ContainsKey(name)) _localMap.Add(name, new StringBuilder());
        _writeBuffer = _localMap[name];
    }

    public void UnsetLocal()
    {
        WriteToProgram();
    }

    public void PushMacroInvoke()
    {
        if (_currentMacro is null) throw new PreprocessorException($"No active macro initializtion.");
        var text = _currentMacro.Render();
        _currentMacro.Reset();
        _currentMacro = null;
        _writeBuffer.AppendLine(text);
    }

    public string Program()
    {
        return _program.ToString();
    }
}

public class MacroBuilder
{
    public MacroBuilder(string name)
    {
        Name = name;
        _arguments = new();
        _varargs = new(new List<ArgGenerator>());
        _bodyGenerators = new();
    }

    public string Name { get; }
    private Dictionary<string, ArgGenerator> _arguments;
    private VarargsGenerator _varargs;
    private List<IGenerator> _bodyGenerators;

    public void AddArg(string name)
    {
        _arguments.Add(name, new ArgGenerator(new EmptyGenerator()));
    }

    public void PushText(string text)
    {
        _bodyGenerators.Add(new TextGenerator(text));
    }

    public void PushArg(string name)
    {
        if (!_arguments.ContainsKey(name)) throw new PreprocessorException($"Argument `{name}` does not exist.");
        _bodyGenerators.Add(_arguments[name]);
    }

    public void PushVarargs()
    {
        _bodyGenerators.Add(_varargs);
    }

    public Macro Build()
    {
        return new Macro(_bodyGenerators, _arguments.Values.ToArray(), _varargs);
    }
}