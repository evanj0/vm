.entry_point (

    i64.push 1
    call #proc1
    exit 0

)

(proc #proc1 .param (#x)
    i64.push 1
    arg.push #x
    i64.cmp_eq
    (if 
        .then (
        call #proc2
        )
        .else (
        call #proc3
        )
    )
)

(proc #proc2
    .local (#x)
    i64.push 200
    i64.push 2
    call #proc1
)

(proc #proc3
    .local (#y)
    i64.push 300
    i64.push 1
    call #proc1
)