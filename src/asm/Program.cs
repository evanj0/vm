using CommandLine;

using vm.lib;
using asm.lib;

namespace asm;

public class Program
{

#nullable disable

    public class Options
    {
        [Value(index: 0, Required = true, HelpText = "Input file path.")]
        public string InputFile { get; set; }

        [Option('o', "output", Required = true, HelpText = "Output file path.")]
        public string OutputFile { get; set; }
    }

#nullable enable

    public static void Main(string[] args)
    {
         Parser.Default.ParseArguments<Options>(args)
            .WithParsed(Run)
            .WithNotParsed(HandleError);
    }

    private static void Run(Options options)
    {
        var text = File.ReadAllText(options.InputFile);
        (var program, var procTable, var strings) = Asm.FromTextFormat(text);
        var assembly = new Assembly(program, procTable, strings);
        var bytes = assembly.Serialize();
        File.WriteAllBytes(options.OutputFile, bytes);
    }

    public static void HandleError(IEnumerable<Error> errors)
    {
        
    }
}