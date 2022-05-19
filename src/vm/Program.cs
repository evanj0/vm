using CommandLine;
using System.Diagnostics;
using vm.lib;
using vm.lib.Exceptions;
using vm.lib.Interop;
using vm.lib.Memory;

namespace vm;

public class Program
{
#nullable disable

    [Verb("run", isDefault: true, HelpText = "Run program.")]
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
           .WithParsed<Options>(Run)
           .WithParsed<DebugOptions>(RunDebug)
           .WithParsed<BenchmarkOptions>(RunBenchmark)
           .WithNotParsed(HandleErrors);
    }

    private static void Run(Options options)
    {
        new VmInstanceBuilder()
            .WithAssembly(Assembly.DeserializeFromFile(options.InputPath))
            .WithOutput(new ConsoleOutput())
            .WithStackSize(1000000)
            .WithProcedure(20, new std.Process.Sleep())
            .WithProcedure(100, new std.Windowing.CreateWindow())
            .WithProcedure(101, new std.Windowing.IsOpen())
            .WithProcedure(102, new std.Windowing.Clear())
            .WithProcedure(103, new std.Windowing.Display())
            .WithProcedure(104, new std.Windowing.DispatchEvents())
            .WithProcedure(105, new std.Windowing.IsKeyPressed())
            .WithProcedure(120, new std.Windowing.DrawCircle())
            .WithProcedure(121, new std.Windowing.DrawRectangle())
            .Build()
            .Run();
    }

    private static void RunDebug(DebugOptions options)
    {
        new VmInstanceBuilder()
            .WithAssembly(Assembly.DeserializeFromFile(options.InputPath))
            .WithOutput(new ConsoleOutput())
            .WithStackSize(1000000)
            .Build()
            .RunDebug();
    }

    private static void RunBenchmark(BenchmarkOptions options)
    {
        var bmInfo = new VmInstanceBuilder()
            .WithAssembly(Assembly.DeserializeFromFile(options.InputPath))
            .WithOutput(new LoggedOutput())
            .WithStackSize(1000000)
            .Build()
            .RunBenchmark(options.Iterations);
        Console.WriteLine($"Benchmark complete ({bmInfo.Iterations} iterations completed in {bmInfo.TimeInMilliseconds} ms).");
        Console.WriteLine($"Time in milliseconds: {bmInfo.TimeInMillisecondsPerIteration}");
        Console.WriteLine($"Time in microseconds: {bmInfo.TimeInMicrosecondsPerIteration}");
    }

    private static void HandleErrors(IEnumerable<Error> errors)
    {

    }
}

public ref struct VmInstance
{
    public VmInstance(int stackSize, int heapSize, IVmOutput output)
    {
        _state = new Vm()
        {
            Ip = 0,
            Stack = new ValueStack(stackSize),
            Frames = new Stack<Frame>(),
        };
        _heap = new Heap(heapSize);
        _program = new();
        _procTable = new();
        _csProcedures = new();
        _strings = Array.Empty<string>();
        _output = output;
    }

    private Vm _state;
    private Heap _heap;
    private Span<Op> _program;
    private Span<ProcInfo> _procTable;
    private Dictionary<int, ICsProcedure> _csProcedures;
    private string[] _strings;
    private IVmOutput _output;

    public void LoadAssembly(Assembly assembly)
    {
        _program = assembly.Ops;
        _procTable = assembly.ProcTable;
        _strings = assembly.Strings;
    }

    public void LoadCsProcedures(Dictionary<int, ICsProcedure> procedures)
    {
        _csProcedures = procedures;
    }

    public ExitStatus RunWithoutErrorHandling()
    {
        return Interpreter.Run(ref _state, ref _heap, 1000000, _output, _program, _procTable, _strings, _csProcedures);
    }

    public ExitStatus Run()
    {
        try
        {
            return RunWithoutErrorHandling();
        }
        catch (VmException e)
        {
            Console.WriteLine($"Runtime execution error: {e.Message}");
        }
        return new ExitStatus(1);
    }

    public ExitStatus RunDebug()
    {
        try
        {
            RunWithoutErrorHandling();
        }
        catch (VmException e)
        {
            Console.WriteLine($"Runtime execution error: {e.Message}");
            Console.WriteLine("Debugging info:");
            Console.WriteLine(_state.Debug());
        }
        // catch (VmHeapException e) { }
        return new ExitStatus(1);
    }

    public BenchmarkInfo RunBenchmark(int iterations)
    {
        var sw = new Stopwatch();
        try
        {
            sw.Start();
            for (var i = 0; i < iterations; i++)
            {
                RunWithoutErrorHandling();
                Reset();
            }
        }
        catch (VmException e)
        {
            Console.WriteLine($"Runtime execution error: {e.Message}");
            Console.WriteLine("Debugging info:");
            Console.WriteLine(_state.Debug());
        }
        finally
        {
            sw.Stop();
        }
        return new BenchmarkInfo
        {
            Iterations = iterations,
            TimeInMilliseconds = sw.ElapsedMilliseconds,
        };
    }

    public void Reset()
    {
        _state.Ip = 0;
        _state.Stack.Sp = 0;
        _state.Frames.Clear();
    }
}

public class VmInstanceBuilder
{
    public VmInstanceBuilder()
    {
        _output = new ConsoleOutput();
        _stackSize = 1024 * 64 / 8; // 64 kb
        _heapSize = 1024 * 1024; // 1 mb
        _csProcedures = new();
    }

    private Assembly? _assembly;
    private IVmOutput _output;
    private Dictionary<int, ICsProcedure> _csProcedures;
    private int _stackSize;
    private int _heapSize;

    public VmInstanceBuilder WithAssembly(Assembly assembly)
    {
        _assembly = assembly;
        return this;
    }

    public VmInstanceBuilder WithProcedure(int index, ICsProcedure proc)
    {
        _csProcedures.Add(index, proc);
        return this;
    }

    public VmInstanceBuilder WithOutput(IVmOutput output)
    {
        _output = output;
        return this;
    }

    public VmInstanceBuilder WithStackSize(int size)
    {
        _stackSize = size;
        return this;
    }

    public VmInstanceBuilder WithHeapSize(int size)
    {
        _heapSize = size;
        return this;
    }

    public VmInstance Build()
    {
        var vmInstance = new VmInstance(_stackSize, _heapSize, _output);
        if (_assembly is not null)
        {
            vmInstance.LoadAssembly(_assembly);
            vmInstance.LoadCsProcedures(_csProcedures);
        }
        return vmInstance;
    }
}

public class BenchmarkInfo
{
    public int Iterations { get; set; }
    public double TimeInMilliseconds { get; set; }
    public double TimeInMicroseconds => TimeInMilliseconds * 1000;
    public double TimeInNanoseconds => TimeInMicroseconds * 1000;
    public double TimeInMillisecondsPerIteration => TimeInMilliseconds / Iterations;
    public double TimeInMicrosecondsPerIteration => TimeInMicroseconds / Iterations;
}