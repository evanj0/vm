using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace vm.lib
{
    public enum OpCode
    {
        // Control Flow

        Exit,
        No_Op,
        Call,
        Call_Extern,
        Return,
        Jump,
        Jump_True,

        // Debugging

        Debug__Dump_Stack,
        Debug__Dump_Heap,
        Debug__Print_I64,
        Debug__Print_F64,
        Debug__Print_Bool,
        Debug__Message,

        // Stack Ops

        I64__Push,
        F64__Push,
        Char__Push,
        Bool__Push,
        Arg__Push,
        Loc__Push,
        Loc__Store,

        // Heap

        StringLoad,
        RecordAlloc,
        RecordGetField,
        RecordSetField,
        ClosureAlloc,
        ClosureSetArg,
        ClosureApply,

        // Math

        I64__Add,
        I64__Sub,
        I64_Mul,
        I64__Div,
        I64__Conv_F64,

        F64__Add,
        F64__Sub,
        F64__Mul,
        F64__Div,

        I64__Cmp_Eq,
        I64__Cmp_Gt,
        I64__Cmp_Lt,
        I64__Cmp_Ge,
        I64__Cmp_Le,

        F64__Cmp_Eq,
        F64__Cmp_Gt,
        F64__Cmp_Lt,
        F64__Cmp_Ge,
        F64__Cmp_Le,

        Bool__Cmp_Eq,
        Bool__Cmp_Ne,

        Bool__Not,
        Bool__And,
        Bool__Or,
    }

    public static class OpCode_Extensions 
    {
        public static string ToUserString(this OpCode opCode)
        {
            return opCode.ToString().ToLower().Replace("__", ".");
        }
    }

    public struct Op
    {
        public Op(OpCode opCode, Word data)
        {
            OpCode = opCode;
            Data = data;
        }

        public Op(OpCode opCode)
        {
            OpCode = opCode;
            Data = Word.Zero();
        }

        public OpCode OpCode;

        public Word Data;
    }
}
