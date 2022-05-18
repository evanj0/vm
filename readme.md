# bytecode interpreter reference implementation and assembler

## Virtual machine
- foreign function interface that loads plugins from dotnet libraries
- works as a domain specific language or as the base for one

## Assembler
- nested conditionals
- nested while loops
- macros/directives

# Installation
1. install `dotnet cli` from MS website
2. create `vm` directory in user directory (i.e., `mkdir ~/vm/`)
3. run build script with `./publish_install.ps1`. (this script is not very good and sometimes does not work, so it occasionally must be run multiple times)