using System.Diagnostics;

using CommandLine;

using vm.lib;
using vm.lib.Exceptions;
using vm.lib.Memory;
using asm.lib;
using static vm.lib.Interpreter;

namespace vm;

class Program
{
#nullable disable

    public class Options
    {
        [Value(index: 0, Required = true, HelpText = "Input assembly file path.")]
        public string InputPath { get; set; }
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
        var bytes = File.ReadAllBytes(options.InputPath);
        var assembly = Assembly.Deserialize(bytes);

        var program = assembly.Ops;
        var procTable = assembly.ProcTable;
        var strings = assembly.Strings;

        var state = new Vm()
        {
            Ip = 0,
            Stack = new ValueStack(),
            Frames = new Stack<Frame>(),
        };

        var heap = new Heap(initialSize: 2048);

        var maxStack = 65_536 * 16;

        var output = new ConsoleOutput();

        var sw = new Stopwatch();
        try
        {
            sw.Start();
            Interpreter.Run(ref state, ref heap, maxStack, output, program, procTable, strings);
        }
        catch (VmException e)
        {
            Console.WriteLine($"Runtime Execution Error: {e.Message}");
            Console.WriteLine("Debugging Info:");
            Console.WriteLine(state.Debug());
        }
        catch (VmHeapException e)
        {
            Console.WriteLine($"Runtime Memory Error: {e.Message}");
            Console.WriteLine("Debugging Info:");
            Console.WriteLine(state.Debug());
            // TODO print object at pointer
        }
        catch (VmExitException e)
        {
            Console.WriteLine();
            Console.WriteLine(e.Message);
            Console.WriteLine();
        }
        finally
        {
            sw.Stop();
            Console.WriteLine($"Execution took {sw.ElapsedMilliseconds} milliseconds.");
        }
    }

    public static void HandleError(IEnumerable<Error> errors)
    {

    }

    public static void Main1(string[] args)
    {
        var input =
@"
.entry_point (

    i64.push 1
    i64.push 2
    i64.push 329847
    debug.print_i64
    i64.add
    i64.push 3
    i64.cmp_eq
    debug.print_bool
    exit 0

)
";

        var state = new Vm()
        {
            Ip = 0,
            Stack = new ValueStack(),
            Frames = new Stack<Frame>(),
        };

        (var program, var procTable, var strings) = Asm.FromTextFormat(input);

        var heap = new Heap(initialSize: 2048);

        var maxStack = 65_536 * 16;

        var output = new ConsoleOutput();

        var sw = new Stopwatch();
        try
        {
            sw.Start();
            Interpreter.Run(ref state, ref heap, maxStack, output, program, procTable, strings);
        }
        catch (VmException e)
        {
            Console.WriteLine($"Runtime Execution Error: {e.Message}");
            Console.WriteLine("Debugging Info:");
            Console.WriteLine(state.Debug());
        }
        catch (VmHeapException e)
        {
            Console.WriteLine($"Runtime Memory Error: {e.Message}");
            Console.WriteLine("Debugging Info:");
            Console.WriteLine(state.Debug());
            // TODO print object at pointer
        }
        catch (VmExitException e)
        {
            Console.WriteLine();
            Console.WriteLine(e.Message);
            Console.WriteLine();
        }
        finally
        {
            sw.Stop();
            Console.WriteLine($"Execution took {sw.ElapsedMilliseconds} milliseconds.");
        }
    }
}