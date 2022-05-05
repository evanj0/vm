namespace vm.asm

module Asm =

    module internal Internal =
    
        type Op =
            | OpCode of vm.lib.Op
            | Call of string

    module internal Text =

        module Parse =

            open FParsec

            module private Internal = 

                let comment: Parser<_, unit> = pstring ";;" .>>. manyCharsTill anyChar (pchar '\n') >>% ()

                let spacesAndComments = many (spaces1 <|> skipMany1 comment)

                let quotedString: Parser<_, unit> = pchar '"' >>. manyCharsTill anyChar (pchar '"')

                let ident: Parser<_, unit> = pchar '#' >>. (asciiLetter <|> digit)

                let pOpGen name args : Parser<_, unit> = pstring name >>. spaces >>. args

                let pOp name inst = pOpGen name spaces >>% inst

                let pOpArg name arg inst = pOpGen name arg |>> inst

                let pOpArgs2 name arg1 arg2 inst = pOpGen name (arg1 .>> spaces .>>. arg2) |>> inst

                let opCode code dataMap inData = Internal.OpCode (vm.lib.Op(code, dataMap inData))

                let unaryOpCode name argParser opcode dataMapping = pOpArg name argParser (opCode opcode dataMapping)

                let nullaryOpCode name opcode = pOp name (Internal.OpCode (vm.lib.Op(opcode)))

                let fromInt32 = vm.lib.Word.FromI32
                let fromInt64 = vm.lib.Word.FromI64

                type Ops = vm.lib.OpCode

                let op = 
                    choice
                        [
                            unaryOpCode "exit" pint32 Ops.Exit fromInt32
                            nullaryOpCode "debug.dump_stack" Ops.DebugDumpStack
                            nullaryOpCode "debug.dump_heap" Ops.DebugDumpHeap

                            // IO

                            nullaryOpCode "io.console.write_str" Ops.IO_Console_WriteString

                            // Values

                            unaryOpCode "i64.push" pint64 Ops.I64Push fromInt64 

                            // Math

                            nullaryOpCode "i64.add" Ops.I64_Add
                            nullaryOpCode "i64.sub" Ops.I64_Sub
                            nullaryOpCode "i64.mul" Ops.I64_Mul
                            nullaryOpCode "i64.div" Ops.I64_Div

                            nullaryOpCode "f64.add" Ops.F64_Add
                            nullaryOpCode "f64.sub" Ops.F64_Sub
                            nullaryOpCode "f64.mul" Ops.F64_Mul
                            nullaryOpCode "f64.div" Ops.F64_Div
                        
                            nullaryOpCode "i64.cmp_eq" Ops.I64_CmpEq
                            nullaryOpCode "i64.cmp_gt" Ops.I64_CmpGt
                            nullaryOpCode "i64.cmp_lt" Ops.I64_CmpLt
                            nullaryOpCode "i64.cmp_ge" Ops.I64_CmpGe
                            nullaryOpCode "i64.cmp_le" Ops.I64_CmpLe
                        
                            nullaryOpCode "f64.cmp_eq" Ops.F64_CmpEq
                            nullaryOpCode "f64.cmp_gt" Ops.F64_CmpGt
                            nullaryOpCode "f64.cmp_lt" Ops.F64_CmpLt
                            nullaryOpCode "f64.cmp_ge" Ops.F64_CmpGe
                            nullaryOpCode "f64.cmp_le" Ops.F64_CmpLe
                        
                            nullaryOpCode "bool.cmp_eq" Ops.Bool_CmpEq
                            nullaryOpCode "bool.cmp_ne" Ops.Bool_CmpNe

                            nullaryOpCode "bool.and" Ops.Bool_And
                            nullaryOpCode "bool.not" Ops.Bool_Not
                            nullaryOpCode "bool.or" Ops.Bool_Or
                        ]

            open Internal

            let program = spacesAndComments >>. many (op .>> spacesAndComments)
        
    open FParsec
    open Internal

    let internal fromTextFormat text =
        match run Text.Parse.program text with
        | Success (ops, _, _) -> 
            ops 
            |> List.map (fun op ->
                match op with
                | OpCode op -> op
                | _ -> failwith "Not Implemented")
            |> Result.Ok 
        | Failure (msg, _, _) -> Result.Error msg

    let FromTextFormat(text: string): vm.lib.Op array = 
        match fromTextFormat text with
        | Result.Ok ops -> List.toArray ops
        | Result.Error msg -> raise (System.Exception(msg))