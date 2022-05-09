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

    public static void Run(ref Vm vm, ref Heap heap, int maxStack, IVmOutput output, Op[] program, ProcInfo[] procTable, string[] strings)
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
                    throw new VmExitException(inst.Data.ToI32());

                case OpCode.NoOp:
                    break;

                case OpCode.IpSet:
                    vm.Ip = inst.Data.ToI32();
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

                case OpCode.DebugDumpStack:
                    output.WriteLine("----------- Stack Dump -----------");
                    output.Write(vm.Debug());
                    output.WriteLine("--------- End Stack Dump ---------");
                    output.WriteLine("");
                    break;

                case OpCode.Debug_PrintI64:
                    output.WriteLine($"{vm.Stack.Pop().ToI64()}");
                    break;

                case OpCode.Debug_PrintBool:
                    output.WriteLine($"{vm.Stack.Pop().ToBool().ToString().ToLower()}");
                    break;

                // Stack Ops

                //  -> i64
                case OpCode.I64Push:
                    vm.Stack.Push(inst.Data);
                    break;

                //  -> bool
                case OpCode.BoolPush:
                    vm.Stack.Push(inst.Data);
                    break;

                case OpCode.ArgLoad:
                    {
                        var index = PeekCurrentFrame(ref vm).ArgsSp + inst.Data.ToI32();
                        var value = vm.Stack.Index(index);
                        vm.Stack.Push(value);
                        break;
                    }

                case OpCode.LocalLoad:
                    {
                        var index = PeekCurrentFrame(ref vm).LocalsSp + inst.Data.ToI32();
                        var value = vm.Stack.Index(index);
                        vm.Stack.Push(value);
                        break;
                    }

                // Heap

                case OpCode.RecordAlloc:
                    { 
                        var pointer = heap.AllocRecord(inst.Data.ToI32());
                        vm.Stack.Push(pointer.ToWord());
                        break;
                    }

                case OpCode.RecordGetField:
                    {
                        var pointer = vm.Stack.Pop().ToHeapPointer();
                        var value = heap.GetField(pointer, inst.Data.ToI32());
                        vm.Stack.Push(value);
                        break;
                    }

                // ptr * -> 
                case OpCode.RecordSetField:
                    {
                        var value = vm.Stack.Pop();
                        var pointer = vm.Stack.Pop().ToHeapPointer();
                        heap.SetField(pointer, inst.Data.ToI32(), value);
                        break;
                    }

                case OpCode.ClosureAlloc:
                    {
                        var procPointer = inst.Data.ReadU32(0);
                        var numParams = inst.Data.ReadU16(4);
                        var pointer = heap.AllocClosure((int)procPointer, numParams);
                        vm.Stack.Push(pointer.ToWord());
                        break;
                    }

                case OpCode.ClosureSetArg:
                    {
                        var value = vm.Stack.Pop();
                        var pointer = vm.Stack.Pop().ToHeapPointer();
                        var paramIdx = inst.Data.ReadU16(0);
                        heap.SetClosureArg(pointer, paramIdx, value);
                        break;
                    }

                // Math

                // i64 i64 -> i64
                case OpCode.I64_Add:
                    {
                        var val1 = vm.Stack.Pop().ToI64();
                        var val2 = vm.Stack.Pop().ToI64();
                        vm.Stack.Push(Word.FromI64(val1 + val2));
                        break;
                    }

                // i64 i64 -> bool
                case OpCode.I64_CmpEq:
                    {
                        var val1 = vm.Stack.Pop().ToI64();
                        var val2 = vm.Stack.Pop().ToI64();
                        vm.Stack.Push(Word.FromBool(val1 == val2));
                        break;
                    }

                default:
                    throw new InstructionNotSupportedException(inst.OpCode.ToUserString());
            }

            vm.Ip++;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Call(ref Vm vm, int maxStack, ProcInfo[] procTable, int proc)
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
        for (var i = 0; i < procTable[proc].NumParams; i++)
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

