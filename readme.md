# interpreter reference implementation and assembler

## Virtual machine
- foreign function interface for dotnet
- works as a domain specific language or as the base for one

## Assembler
- nested conditionals
- nested while loops
- named local variables and args

# Installation
1. install `dotnet cli` from MS website
2. create `vm` directory in user directory (i.e., `mkdir ~/vm/`)
3. run build script with `./publish_install.ps1`. (this script is not very good and sometimes does not work, so it occasionally must be run multiple times)

# CLI Usage
## Virtual machine
```
run          (Default Verb) Run program.
debug        Debug mode.
benchmark    Benchmark mode.
help         Display more information on a specific command.
version      Display version information.
```
## Assembler
```
-o, --output    Required. Output file path.
value pos. 0    Required. Input file path.
```
