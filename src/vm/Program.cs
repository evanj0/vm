using vm.lib;
using vm.lib.Exceptions;
using vm.lib.Memory;
using vm.asm;
using System.Diagnostics;
using static vm.lib.Interpreter;

var input =
@"

i64.push 1
i64.push 2
i64.push 329847
debug.print_i64
i64.add
i64.push 4
i64.cmp_eq
debug.print_bool
exit 0

";

var state = new Vm()
{
    Ip = 0,
    Stack = new ValueStack(),
    Frames = new Stack<Frame>(),
};

var program = Asm.FromTextFormat(input);

var procTable = new ProcInfo[] { };

var heap = new Heap(initialSize: 2048);

var maxStack = 65_536 * 16;

var output = new ConsoleOutput();

var sw = new Stopwatch();
try
{
    sw.Start();
    Run(ref state, ref heap, maxStack, output, program, procTable, new string[] { });
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