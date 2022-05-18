using System.Text;

namespace asm.lib.macro;

public interface IGenerator
{
    public string Generate();
}

public class TextGenerator : IGenerator
{
    public TextGenerator(string text)
    {
        _text = text;
    }

    private string _text;

    public string Generate()
    {
        return _text;
    }
}

public class Generator : IGenerator
{
    public Generator(IEnumerable<IGenerator> generators)
    {
        _generators = generators;
    }

    private IEnumerable<IGenerator> _generators;

    public string Generate()
    {
        var sb = new StringBuilder();
        foreach (var generator in _generators)
        {
            sb.AppendLine(generator.Generate());
        }
        return sb.ToString();
    }
}

public class ArgGenerator : IGenerator
{
    public ArgGenerator(IGenerator internalGenerator)
    {
        Internal = internalGenerator;
    }

    public IGenerator Internal { get; set; }

    public string Generate()
    {
        return Internal.Generate();
    }
}

public class VarargsGenerator : IGenerator
{
    public VarargsGenerator(List<ArgGenerator> arguments)
    {
        Arguments = arguments;
    }

    public List<ArgGenerator> Arguments { get; set; }

    public string Generate()
    {
        var sb = new StringBuilder();
        foreach (var argument in Arguments)
        {
            sb.AppendLine(argument.Generate());
        }
        return sb.ToString();
    }
}

public class EmptyGenerator : IGenerator
{
    public string Generate()
    {
        return string.Empty;
    }
}