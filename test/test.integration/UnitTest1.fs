module test.integration

open NUnit.Framework

let test text pred =
    let bytes = asm.Program.Assemble(text, asm.Program.Options()).Serialize()
    let output = vm.lib.LoggedOutput()
    let mutable vmOptions = vm.Program.Options()
    vmOptions.Debug <- false;
    vm.Program.RunVm(vm.lib.Assembly.Deserialize(bytes), output, vmOptions)
    let outputString = output.ToString()
    printf "%s" (output.ToString())
    Assert.True(pred outputString)

let test1Text = """
.entry_point (

    i64.push 1
    i64.push 2
    i64.add
    i64.push 3
    i64.cmp_eq
    debug.print_bool
    exit 0

)
"""

[<Test>]
let test1() = test test1Text (fun s -> s.ToLower().Trim() = "true")

let test2Text num = $"""
.entry_point (
    
    i64.push {num}
    i64.push 1
    i64.cmp_eq 
    (if 
        .then (
            i64.push 1
        )
        .else (
            i64.push 0
        )
    )
    debug.print_i64
    exit 0

)
"""

[<Test>]
let test2_1() = test (test2Text 1) (fun s -> s.Trim() = "1")
[<Test>]
let test2_2() = test (test2Text 0) (fun s -> s.Trim() = "0")