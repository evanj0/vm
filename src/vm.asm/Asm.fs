namespace asm.lib

module Asm =

    module internal Internal =
        
        type Op1 =
            | Op of vm.lib.Op
            | Call of string
            | Label of string
            | Jump of string
            | Jump_True of string

        type Stmt =
            | Op of vm.lib.Op
            | Call of string
            | Arg__Load of string
            | Loc__Load of string
            | Loc__Store of string
            | If of t: Stmt list * e: Stmt list
            | While of cond: Stmt list * body: Stmt list

        type GeneratorToken =
            | Token of string
            | Space
            | Macro of name: string * args: GeneratorToken list list

        type TopLevel =
            | Proc of name: string * parameters: string list * locals: string list * ops: Stmt list
            | Macro of name: string * parameters: string list * body: Stmt list

        type Code = { entryPoint: Stmt list; topLevel: TopLevel list }

    module internal Text =

        module Parse =

            open FParsec

            open Internal

            module private Internal = 

                let comment: Parser<_, unit> = pstring ";;" .>>. manyCharsTill anyChar (pchar '\n') >>% ()

                /// Spaces and comments.
                let sp = many (spaces1 <|> skipMany1 comment)

                let quotedString: Parser<_, unit> = pchar '"' >>. manyCharsTill anyChar (pchar '"')

                let ident: Parser<_, unit> = pchar '#' >>. many1Chars (asciiLetter <|> digit <|> anyOf "_-.[]<>:")

                let pOpGen name args : Parser<_, unit> = pstring name >>. spaces >>. args

                let pOp name inst = pOpGen name spaces >>% inst

                let pOpArg name arg inst = pOpGen name arg |>> inst

                let pOpArgs2 name arg1 arg2 inst = pOpGen name (arg1 .>> spaces .>>. arg2) |>> inst

                let opCode code dataMap inData = Stmt.Op (vm.lib.Op(code, dataMap inData))

                let unaryOpCode name argParser opcode dataMapping = pOpArg name argParser (opCode opcode dataMapping)

                let nullaryOpCode name opcode = pOp name (Stmt.Op (vm.lib.Op(opcode)))

                let pExpr keyword args = attempt (pchar '(' >>. sp >>. pstring keyword >>. sp >>. args .>> sp .>> pchar ')')

                let pArg keyword value = attempt(pchar '.' >>. pstring keyword) >>. sp >>. pchar '(' >>. sp >>. value .>> sp .>> pchar ')'

                let fromInt32 = vm.lib.Word.FromI32
                let fromInt64 = vm.lib.Word.FromI64
                let fromFloat64 = vm.lib.Word.FromF64

                type Ops = vm.lib.OpCode

                let (op, opRef) = createParserForwardedToRef()

                let ops = many (op .>> sp)

                let opImpl = 
                    choice
                        [
                            pExpr "if" (pArg "then" ops .>> sp .>>. pArg "else" ops) |>> Stmt.If
                            pExpr "while" (pArg "cond" ops .>> sp .>>. pArg "do" ops) |>> Stmt.While

                            unaryOpCode "exit" pint32 Ops.Exit fromInt32

                            // Debugging

                            nullaryOpCode "debug.dump_stack" Ops.Debug__Dump_Stack
                            nullaryOpCode "debug.dump_heap" Ops.Debug__Dump_Heap
                            nullaryOpCode "debug.print_i64" Ops.Debug__Print_I64
                            nullaryOpCode "debug.print_f64" Ops.Debug__Print_F64
                            nullaryOpCode "debug.print_bool" Ops.Debug__Print_Bool
                            // pstring "debug.message" >>. sp >>. quotedString |>> 

                            // Values

                            unaryOpCode "i64.push" pint64 Ops.I64__Push fromInt64 
                            unaryOpCode "f64.push" pfloat Ops.F64__Push fromFloat64 

                            // Procs
                            
                            unaryOpCode "call_extern" pint32 Ops.Call_Extern fromInt32
                            pstring "call" >>. sp >>. ident |>> Stmt.Call
                            pstring "loc.push" >>. sp >>. ident |>> Stmt.Loc__Load
                            pstring "arg.push" >>. sp >>. ident |>> Stmt.Arg__Load

                            pstring "loc.store" >>. sp >>. ident |>> Stmt.Loc__Store

                            // Math

                            nullaryOpCode "i64.add" Ops.I64__Add
                            nullaryOpCode "i64.sub" Ops.I64__Sub
                            nullaryOpCode "i64.mul" Ops.I64_Mul
                            nullaryOpCode "i64.div" Ops.I64__Div
                            nullaryOpCode "i64.conv_f64" Ops.I64__Conv_F64

                            nullaryOpCode "f64.add" Ops.F64__Add
                            nullaryOpCode "f64.sub" Ops.F64__Sub
                            nullaryOpCode "f64.mul" Ops.F64__Mul
                            nullaryOpCode "f64.div" Ops.F64__Div
                            nullaryOpCode "f64.conv_i64" Ops.F64__Conv_I64
                        
                            nullaryOpCode "i64.cmp_eq" Ops.I64__Cmp_Eq
                            nullaryOpCode "i64.cmp_gt" Ops.I64__Cmp_Gt
                            nullaryOpCode "i64.cmp_lt" Ops.I64__Cmp_Lt
                            nullaryOpCode "i64.cmp_ge" Ops.I64__Cmp_Ge
                            nullaryOpCode "i64.cmp_le" Ops.I64__Cmp_Le
                        
                            nullaryOpCode "f64.cmp_eq" Ops.F64__Cmp_Eq
                            nullaryOpCode "f64.cmp_gt" Ops.F64__Cmp_Gt
                            nullaryOpCode "f64.cmp_lt" Ops.F64__Cmp_Lt
                            nullaryOpCode "f64.cmp_ge" Ops.F64__Cmp_Ge
                            nullaryOpCode "f64.cmp_le" Ops.F64__Cmp_Le
                        
                            nullaryOpCode "bool.cmp_eq" Ops.Bool__Cmp_Eq
                            nullaryOpCode "bool.cmp_ne" Ops.Bool__Cmp_Ne

                            nullaryOpCode "bool.and" Ops.Bool__And
                            nullaryOpCode "bool.not" Ops.Bool__Not
                            nullaryOpCode "bool.or" Ops.Bool__Or
                        ]
                opRef.Value <- opImpl

            open Internal

            let entryPoint = pArg "entry_point" ops

            let proc = pExpr "proc" (ident .>> sp .>>. many (pArg "param" ident .>> sp) .>>. many (pArg "local" ident .>> sp) .>>. ops) |>> fun (((name, parameters), locals), ops) -> TopLevel.Proc (name, parameters, locals, ops)

            let topLevel = many (proc .>> sp)

            let program = sp >>. entryPoint .>> sp .>>. topLevel .>> sp .>> eof |>> fun (entryPoint, topLevel) -> { Code.entryPoint = entryPoint; Code.topLevel = topLevel }
        
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
    
    type AssemblerException(message: string) = inherit System.Exception(message)
    
    type AssemblerLabelException(label) = inherit AssemblerException(sprintf "Label `%s` could not be found." label)

    type internal AsmBuilder() = class
        let _ops = System.Collections.Generic.List<Op1>()
        let _labelPrefixStack = System.Collections.Generic.Stack<string>()
        let mutable _labelPrefixIndex = 0;
        let _paramMap = System.Collections.Generic.Dictionary<string, int>()
        let mutable _paramIndex = 0;
        let _localMap = System.Collections.Generic.Dictionary<string, int>()
        let mutable _localIndex = 0;
        /// name => label, numParams, numLocals
        let _procMap = System.Collections.Generic.Dictionary<string, string * int * int>()
        /// name => index
        let _procToIndex = System.Collections.Generic.Dictionary<string, int>()
        /// index => name
        let _procs = System.Collections.Generic.List<string>()
        let _strings = System.Collections.Generic.List<string>()
        let CreateLabel(label: string) =
            if _labelPrefixStack.Count = 0 then
                label
            else
                sprintf "%s::%s" (_labelPrefixStack.Peek()) label

        member this.PushLabelPrefix(prefix: string) =
            _labelPrefixStack.Push(prefix)
            this
        member this.PushLabelPrefix() =
            _labelPrefixStack.Push(sprintf "%d" _labelPrefixIndex)
            _labelPrefixIndex <- _labelPrefixIndex + 1
            this
        member this.PopLabelPrefix() =
            _labelPrefixStack.Pop() |> ignore
            this
        member this.Label(label: string) =
            _ops.Add(Label (CreateLabel(label)))
            this
        member this.Jump(label: string) =
            _ops.Add(Jump (CreateLabel(label)))
            this
        member this.JumpTrue(label: string) =
            _ops.Add(Jump_True (CreateLabel(label)))
            this
        member this.Op(op: Op) =
            _ops.Add(Op1.Op op)
            this
        member this.Op(opCode: OpCode, value: Word) =
            _ops.Add(Op1.Op (Op(opCode, value)))
            this
        member this.Op(opCode: OpCode) =
            _ops.Add(Op1.Op (Op(opCode)))
            this
        member this.DebugMessage(message: string) =
            let index = _strings.Count
            _strings.Add(message)
            this.Op(OpCode.Debug__Message, Word.FromI32(index))

        member this.BeginProc(name: string, numParams: int, numLocals: int) =
            let label = sprintf "proc::%s" name
            // update proc table
            _procMap.Add(name, (label, numParams, numLocals))
            // update proc <=> index bimap
            let index = _procs.Count
            _procToIndex.Add(name, index)
            _procs.Add(name)
            // label starts proc
            this.Label(label)
        member this.Param(name: string) =
            _paramMap.Add(name, _paramIndex)
            _paramIndex <- _paramIndex + 1
            this
        member this.Local(name: string) =
            _localMap.Add(name, _localIndex)
            _localIndex <- _localIndex + 1
            this
        member this.EndProc() =
            // clear params and locals
            _paramMap.Clear()
            _localMap.Clear()
            _paramIndex <- 0
            _localIndex <- 0
            this.Op(OpCode.Return)

        member this.Call(name: string) =
            _ops.Add(Op1.Call name)
            this

        member this.Stmts(input: Stmt list): AsmBuilder =
            for stmt in input do
                match stmt with
                | Op op -> 
                    this.Op(op)
                | Call name ->
                    this.Call(name)
                | Arg__Load name ->
                    this.Op(OpCode.Arg__Push, Word.FromI32(_paramMap[name]))
                | Loc__Load name ->
                    this.Op(OpCode.Loc__Push, Word.FromI32(_localMap[name]))
                | Loc__Store name ->
                    this.Op(OpCode.Loc__Store, Word.FromI32(_localMap[name]))
                | If (t, e) ->
                    this.PushLabelPrefix()
                        .JumpTrue("then")
                        .Jump("else")

                        .Label("then")
                        .Stmts(t)
                        .Jump("end")

                        .Label("else")
                        .Stmts(e)
                        .Jump("end")

                        .Label("end")
                        .PopLabelPrefix()
                | While (cond, body) -> 
                    this.PushLabelPrefix()
                        .Label("cond")
                        .Stmts(cond)
                        .JumpTrue("body") // jump over next jump
                        .Jump("end") // hit if prev jump wasnt hit

                        .Label("body")
                        .Stmts(body)
                        .Jump("cond")

                        .Label("end")
                        .PopLabelPrefix()
                |> ignore
            this

        static member FromCode(code: Code): AsmBuilder =
            let asmb = AsmBuilder()
            asmb
                .Jump("entry_point")
                .Label("entry_point")
                .Stmts(code.entryPoint)
                // guard against crashing if user forgets to exit
                .Op(OpCode.Exit, Word.FromI32(0))
                |> ignore
            for topLevel in code.topLevel do
                match topLevel with 
                | Proc (name, parameters, locals, stmts) ->
                    asmb.BeginProc(name, parameters.Length, locals.Length) |> ignore
                    // add params
                    for parameter in parameters do
                        asmb.Param(parameter) |> ignore
                    // add locals
                    for local in locals do
                        asmb.Local(local) |> ignore
                    asmb
                        .Stmts(stmts)
                        .EndProc() |> ignore
            asmb
            
        member this.ToAssembly() =
            let ops = System.Collections.Generic.List<Op>()
            let mutable labels = System.Collections.Generic.Dictionary<string, int>()
            let procTable = System.Collections.Generic.List<ProcInfo>()
            let getAddr(label) = 
                if not (labels.ContainsKey(label)) then
                    raise (AssemblerLabelException(label))
                else 
                    labels[label]
            
            // construct label => index map
            for i = 0 to _ops.Count - 1 do
                let op = _ops[i]
                match op with 
                | Label name -> 
                    labels.Add(name, i)
                | _ -> ()
            
            // convert maps to actual proctable. this adds in correct order recorded in _procToIndex
            for proc in _procs do
                let (label, numParams, numLocals) = _procMap[proc]
                let addr = getAddr(label)
                procTable.Add(ProcInfo(addr, numParams, numLocals))

            // convert to real ops
            for op in _ops do
                match op with
                // passthrough ops
                | Op1.Op op -> ops.Add(op)
                // replace labels with noOps to keep indices the same
                | Op1.Call name ->
                    // need to use this because we want proc index and not address
                    let index = _procToIndex[name]
                    ops.Add(Op(OpCode.Call, Word.FromI32(index)))
                | Label _ -> ops.Add(Op(OpCode.No_Op))
                // ops that use labels
                | Jump_True label ->
                    let addr = getAddr(label)
                    ops.Add(Op(OpCode.Jump_True, Word.FromI32(addr)))
                | Jump label ->
                    let addr = getAddr(label)
                    ops.Add(Op(OpCode.Jump, Word.FromI32(addr)))                    

            Assembly(ops.ToArray(), procTable.ToArray(), _strings.ToArray())

    end
    type CsList<'T> = System.Collections.Generic.List<'T>

    let internal ConvertToOps(input: CsList<Op1>): vm.lib.Op array * vm.lib.ProcInfo array * string array =
        let mutable ops = CsList<vm.lib.Op>()
        let mutable labels = System.Collections.Generic.Dictionary<string, int>()
        let mutable procTable = CsList<vm.lib.ProcInfo>()
        let mutable strings = CsList<string>()

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
            | Op1.Op op -> ops.Add(op)
            | Label _ -> ops.Add(Op(OpCode.No_Op))
            | Jump_True label ->
                let index = getIndex(label)
                ops.Add(Op(OpCode.Jump_True, Word.FromI32(index)))
            | Jump label ->
                let index = getIndex(label)
                ops.Add(Op(OpCode.Jump, Word.FromI32(index)))

        ops.ToArray(), procTable.ToArray(), strings.ToArray()

    type AssemblerParsingException(message) = inherit AssemblerException(message)

    let FromTextFormat(text: string) = 
        match run Text.Parse.program text with
        | Success (code, _, _) -> 
            let asm = AsmBuilder.FromCode(code).ToAssembly()
            asm
        | Failure (msg, _, _) -> raise (AssemblerParsingException(msg))