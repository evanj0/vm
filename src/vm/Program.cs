using System.Diagnostics;

using CommandLine;

using vm.lib;
using vm.lib.Exceptions;
using vm.lib.Memory;
using asm.lib;
using static vm.lib.Interpreter;

namespace vm;

public class Program
{
#nullable disable

    public class Options
    {
        [Value(index: 0, Required = true, HelpText = "Input assembly file path.")]
        public string InputPath { get; set; }
    }

    [Verb("debug", HelpText = "Debug mode.")]
    public class DebugOptions
    {
        [Value(index: 0, Required = true, HelpText = "Input assembly file path.")]
        public string InputPath { get; set; }
    }

    [Verb("benchmark", HelpText = "Benchmark mode.")]
    public class BenchmarkOptions
    {
        [Value(index: 0, Required = true, HelpText = "Input assembly file path.")]
        public string InputPath { get; set; }

        [Option('i', "iterations", Required = true, HelpText = "Number of iterations to perform benchmark over.")]
        public int Iterations { get; set; }
    }

#nullable enable

    public static void Main(string[] args)
    {
        Parser.Default.ParseArguments<Options, DebugOptions, BenchmarkOptions>(args)
           .MapResult(
                (Options options) =>
                {

                },
                (DebugOptions options) =>
                {

                },
                (BenchmarkOptions options) =>
                {

                },
                HandleErrors
            );
    }

    private static void Run(Options options)
    {
        var bytes = File.ReadAllBytes(options.InputPath);
        var assembly = Assembly.Deserialize(bytes);
        RunVm(assembly, new ConsoleOutput(), options);
    }

    private static void HandleErrors(IEnumerable<Error> errors)
    {

    }

    public static void RunVm(Assembly assembly, IVmOutput output, Options options)
    {
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

        var sw = new Stopwatch();
        try
        {
            sw.Start();
            Interpreter.Run(ref state, ref heap, maxStack, output, program, procTable, strings);
        }
        catch (VmException e)
        {
            Console.WriteLine($"Runtime Execution Error: {e.Message}");

            if (options.Debug)
            {
                sw.Stop();
                Console.WriteLine("Debugging Info:");
                Console.WriteLine(state.Debug());
            }
        }
        catch (VmHeapException e)
        {
            Console.WriteLine($"Runtime Memory Error: {e.Message}");

            if (options.Debug)
            {
                sw.Stop();
                Console.WriteLine("Debugging Info:");
                Console.WriteLine(state.Debug());
            }
            // TODO print object at pointer
        }
        catch (VmExitException e)
        {
            if (options.Debug)
            {
                sw.Stop();
                Console.WriteLine(e.Message);
            }
        }
        finally
        {
            if (options.Debug)
            {
                sw.Stop();
                Console.WriteLine($"Execution took {sw.ElapsedMilliseconds} milliseconds.");
            }
        }
    }
}