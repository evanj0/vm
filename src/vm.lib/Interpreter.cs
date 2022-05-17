using vm.lib.Exceptions;
using vm.lib.Memory;
using System.Runtime.CompilerServices;

namespace vm.lib;

public static class Interpreter
{
    // TODO implement (jump-table)
    // TODO implement heap operations:
        // TODO allocation
        // TODO data types + runtime type info
        // TODO pattern matching
        // TODO gc?

    public static ExitStatus Run(ref Vm vm, ref Heap heap, int maxStack, IVmOutput output, Span<Op> program, Span<ProcInfo> procTable, string[] strings)
    {
        while (true)
        {
            if (vm.Ip >= program.Length)
            {
                throw new InstructionPointerOutOfRangeException();
            }

            var inst = program[vm.Ip];

            switch (inst.OpCode)
            {
                // Control Flow

                case OpCode.Exit:
                    return new ExitStatus(inst.Data.ToI32());

                case OpCode.No_Op:
                    break;

                case OpCode.Call:
                    {
                        var proc = inst.Data.ToI32();
                        Call(ref vm, maxStack, procTable, proc);
                        break;
                    }

                case OpCode.Return:
                    {
                        var frame = PopCurrentFrame(ref vm);
                        vm.Ip = frame.ReturnAddr;
                        var returnValue = vm.Stack.Pop();
                        vm.Stack.Data[frame.BaseSp] = returnValue;
                        vm.Stack.Sp = frame.BaseSp + 1;
                        break;
                    }

                case OpCode.Jump:
                    vm.Ip = inst.Data.ToI32();
                    break;

                case OpCode.Jump_True:
                    if (vm.Stack.Pop().ToBool() == true)
                    {
                        vm.Ip = inst.Data.ToI32();
                    }
                    break;

                // Debugging

                case OpCode.Debug__Dump_Stack:
                    output.WriteLine("----------- Stack Dump -----------");
                    output.Write(vm.Debug());
                    output.WriteLine("--------- End Stack Dump ---------");
                    output.WriteLine("");
                    break;

                case OpCode.Debug__Print_I64:
                    output.WriteLine($"{vm.Stack.Pop().ToI64()}");
                    break;

                case OpCode.Debug__Print_F64:
                    output.WriteLine($"{vm.Stack.Pop().ToF64()}");
                    break;

                case OpCode.Debug__Print_Bool:
                    output.WriteLine($"{vm.Stack.Pop().ToBool().ToString().ToLower()}");
                    break;

                // Stack Ops

                //  -> i64
                case OpCode.I64__Push:
                    vm.Stack.Push(inst.Data);
                    break;

                case OpCode.F64__Push:
                    vm.Stack.Push(inst.Data);
                    break;

                //  -> bool
                case OpCode.Bool__Push:
                    vm.Stack.Push(inst.Data);
                    break;

                case OpCode.Arg__Push:
                    {
                        var index = PeekCurrentFrame(ref vm).ArgsSp + inst.Data.ToI32();
                        var value = vm.Stack.Index(index);
                        vm.Stack.Push(value);
                        break;
                    }

                case OpCode.Loc__Push:
                    {
                        var index = PeekCurrentFrame(ref vm).LocalsSp + inst.Data.ToI32();
                        var value = vm.Stack.Index(index);
                        vm.Stack.Push(value);
                        break;
                    }
                case OpCode.Loc__Store:
                    {
                        var value = vm.Stack.Pop();
                        var index = PeekCurrentFrame(ref vm).LocalsSp + inst.Data.ToI32();
                        vm.Stack.Data[index] = value;
                        break;
                    }

                // Math

                // i64 i64 -> i64
                case OpCode.I64__Add:
                    {
                        var val2 = vm.Stack.Pop().ToI64();
                        var val1 = vm.Stack.Pop().ToI64();
                        vm.Stack.Push(Word.FromI64(val1 + val2));
                        break;
                    }
                case OpCode.I64__Sub:
                    {
                        var val2 = vm.Stack.Pop().ToI64();
                        var val1 = vm.Stack.Pop().ToI64();
                        vm.Stack.Push(Word.FromI64(val1 - val2));
                        break;
                    }
                case OpCode.I64_Mul:
                    {
                        var val2 = vm.Stack.Pop().ToI64();
                        var val1 = vm.Stack.Pop().ToI64();
                        vm.Stack.Push(Word.FromI64(val1 * val2));
                        break;
                    }

                case OpCode.I64__Conv_F64:
                    {
                        var val = vm.Stack.Pop().ToI64(); // i64
                        vm.Stack.Push(Word.FromF64(val)); // f64
                        break;
                    }


                case OpCode.F64__Add:
                    {
                        var val2 = vm.Stack.Pop().ToF64();
                        var val1 = vm.Stack.Pop().ToF64();
                        vm.Stack.Push(Word.FromF64(val1 + val2));
                        break;
                    }
                case OpCode.F64__Sub:
                    {
                        var val2 = vm.Stack.Pop().ToF64();
                        var val1 = vm.Stack.Pop().ToF64();
                        vm.Stack.Push(Word.FromF64(val1 - val2));
                        break;
                    }
                case OpCode.F64__Mul:
                    {
                        var val2 = vm.Stack.Pop().ToF64();
                        var val1 = vm.Stack.Pop().ToF64();
                        vm.Stack.Push(Word.FromF64(val1 * val2));
                        break;
                    }
                case OpCode.F64__Div:
                    {
                        var val2 = vm.Stack.Pop().ToF64();
                        var val1 = vm.Stack.Pop().ToF64();
                        vm.Stack.Push(Word.FromF64(val1 / val2));
                        break;
                    }

                // i64 i64 -> bool
                case OpCode.I64__Cmp_Eq:
                    {
                        var val1 = vm.Stack.Pop().ToI64();
                        var val2 = vm.Stack.Pop().ToI64();
                        vm.Stack.Push(Word.FromBool(val1 == val2));
                        break;
                    }

                case OpCode.I64__Cmp_Le:
                    {
                        var val2 = vm.Stack.Pop().ToI64();
                        var val1 = vm.Stack.Pop().ToI64();
                        vm.Stack.Push(Word.FromBool(val1 <= val2));
                        break;
                    }

                case OpCode.I64__Cmp_Lt:
                    {
                        var val2 = vm.Stack.Pop().ToI64();
                        var val1 = vm.Stack.Pop().ToI64();
                        vm.Stack.Push(Word.FromBool(val1 < val2));
                        break;
                    }

                case OpCode.F64__Cmp_Lt:
                    {
                        var val2 = vm.Stack.Pop().ToF64();
                        var val1 = vm.Stack.Pop().ToF64();
                        vm.Stack.Push(Word.FromBool(val1 < val2));
                        break;
                    }

                default:
                    throw new InstructionNotSupportedException(inst.OpCode.ToUserString());
            }

            vm.Ip++;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Call(ref Vm vm, int maxStack, Span<ProcInfo> procTable, int proc)
    {
        if (proc >= procTable.Length)
        {
            throw new ProcedureDoesNotExistException(proc);
        }
        if (vm.Frames.Count + 1 > maxStack)
        {
            throw new CallStackOverflowException(vm.Frames.Count);
        }
        // check if sp will be moved below 0. this can happen if not all args are placed on the stack when a proc is called. 
        if (vm.Stack.Sp - procTable[proc].NumParams < 0)
        {
            throw new VmException("Base stack pointer was moved out of range.");
        }
        vm.Frames.Push(new Frame(
            returnAddr: vm.Ip, // return to where execution is now
            baseSp: vm.Stack.Sp - procTable[proc].NumParams, // baseSp starts where sp is now - number of params. this passes the args
            procInfo: procTable[proc]
            ));
        // move ip
        vm.Ip = procTable[proc].Addr;
        // initialize locals
        for (var i = 0; i < procTable[proc].NumLocals; i++)
        {
            vm.Stack.Push(Word.Zero());
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Frame PeekCurrentFrame(ref Vm vm)
    {
        if (vm.Frames.Count == 0)
        {
            throw new CallStackUnderflowException();
        }
        return vm.Frames.Peek();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Frame PopCurrentFrame(ref Vm vm)
    {
        if (vm.Frames.Count == 0)
        {
            throw new CallStackUnderflowException();
        }
        return vm.Frames.Pop();
    }
}

public struct ExitStatus
{
    public ExitStatus(int exitCode)
    {
        ExitCode = exitCode;
    }

    public int ExitCode;
}