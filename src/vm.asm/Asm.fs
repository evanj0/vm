namespace asm.lib

module Asm =

    module internal Internal =
        
        type Op2 =
            | Op of vm.lib.Op
            | Label of string
            | Jump of string
            | Jump_True of string
            | Jump_Eq of string

        type Op1 =
            | OpCode of vm.lib.Op
            | Call of string
            | If of t: Op1 list * e: Op1 list

    module internal Text =

        module Parse =

            open FParsec

            open Internal

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

                let pExpr keyword args = pchar '(' >>. spacesAndComments >>. pstring keyword >>. spacesAndComments >>. args .>> spacesAndComments .>> pchar ')'

                let pArg keyword value = pchar '.' >>. pstring keyword >>. spacesAndComments >>. pchar '(' >>. spacesAndComments >>. value .>> spacesAndComments .>> pchar ')'

                let fromInt32 = vm.lib.Word.FromI32
                let fromInt64 = vm.lib.Word.FromI64

                type Ops = vm.lib.OpCode

                let (op, opRef) = createParserForwardedToRef()

                let ops = spacesAndComments >>. many (op .>> spacesAndComments)

                let opImpl = 
                    choice
                        [
                            pExpr "if" (pArg "then" ops .>> spacesAndComments .>>. pArg "else" ops) |>> Op1.If

                            unaryOpCode "exit" pint32 Ops.Exit fromInt32

                            // Debugging

                            nullaryOpCode "debug.dump_stack" Ops.DebugDumpStack
                            nullaryOpCode "debug.dump_heap" Ops.DebugDumpHeap
                            nullaryOpCode "debug.print_i64" Ops.Debug_PrintI64
                            nullaryOpCode "debug.print_bool" Ops.Debug_PrintBool

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
                opRef.Value <- opImpl

            open Internal

            let entryPoint = pArg "entry_point" ops

            let program = spacesAndComments >>. entryPoint .>> spacesAndComments
        
    open FParsec
    open Internal
    
    open vm.lib

    type internal LabelGen() = class
        member val Index: int = 0 with get, set
        member this.Increment() = this.Index <- this.Index + 1
        member this.CreateThen() = sprintf "%d_then" this.Index
        member this.CreateElse() = sprintf "%d_else" this.Index
        member this.CreateEnd()  = sprintf "%d_end"  this.Index
    end

    type internal MutList<'T> = System.Collections.Generic.List<'T>

    let rec internal Convert1to2(input, labelGen: LabelGen): MutList<Op2> =
        let mutable ops = MutList<Op2>()
        for op in input do
            match op with
            | OpCode op -> 
                ops.Add(Op2.Op op)
            | If (t, e) ->
                let thenLabel = labelGen.CreateThen()
                let elseLabel = labelGen.CreateElse()
                let endLabel = labelGen.CreateEnd()
                labelGen.Increment()

                ops.Add(Jump_True thenLabel)
                ops.Add(Jump elseLabel)

                ops.Add(Label thenLabel)
                ops.AddRange(Convert1to2(t, labelGen))
                ops.Add(Jump endLabel)

                ops.Add(Label elseLabel)
                ops.AddRange(Convert1to2(e, labelGen))
                ops.Add(Jump endLabel)

                ops.Add(Label endLabel)
        ops

    type AssemblerException(message: string) = inherit System.Exception(message)

    type AssemblerLabelException(label) = inherit AssemblerException(sprintf "Label `%s` could not be found." label)

    let internal Convert2toOps(input: MutList<Op2>): vm.lib.Op array * vm.lib.ProcInfo array * string array =
        let mutable ops = MutList<vm.lib.Op>()
        let mutable labels = System.Collections.Generic.Dictionary<string, int>()
        let mutable procTable = MutList<vm.lib.ProcInfo>()
        let mutable strings = MutList<string>()

        let getIndex(label) = 
            if not (labels.ContainsKey(label)) then
                raise (AssemblerLabelException(label))
            else 
                labels[label]

        for i = 0 to input.Count - 1 do
            let op = input[i]
            match op with 
            | Label name -> 
                labels.Add(name, i)
            | _ -> ()

        for op in input do
            match op with
            | Op2.Op op -> ops.Add(op)
            | Label _ -> ops.Add(Op(OpCode.NoOp))
            | Jump_True label ->
                let index = getIndex(label)
                ops.Add(Op(OpCode.Jump_True, Word.FromI32(index)))
            | Jump label ->
                let index = getIndex(label)
                ops.Add(Op(OpCode.Jump, Word.FromI32(index)))

        ops.ToArray(), procTable.ToArray(), strings.ToArray()

    let internal fromTextFormat text =
        match run Text.Parse.program text with
        | Success (ops, _, _) -> 
            Convert2toOps(Convert1to2(ops, LabelGen())) |> Result.Ok
        | Failure (msg, _, _) -> Result.Error msg

    type AssemblerParsingException(message) = inherit AssemblerException(message)

    let FromTextFormat(text: string) = 
        match fromTextFormat text with
        | Result.Ok ops -> ops
        | Result.Error msg -> raise (AssemblerParsingException(msg))