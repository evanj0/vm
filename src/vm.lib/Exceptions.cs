using vm.lib.Memory;

namespace vm.lib.Exceptions
{
    public class VmExitException : Exception 
    {
        public VmExitException(int exitCode) : base($"Program exited with status code {exitCode}.") { }
    }

    public class VmException : Exception
    {
        public VmException(string message) : base(message) { }
    }

    public class StackPointerOutOfRangeException : VmException
    {
        public StackPointerOutOfRangeException() : base("Stack pointer was out of range.") { }
    }

    public class InstructionPointerOutOfRangeException : VmException
    {
        public InstructionPointerOutOfRangeException() : base("Instruction pointer was out of range.") { }
    }

    public class ProcedureDoesNotExistException : VmException
    {
        public ProcedureDoesNotExistException(int index) : base($"Procedure at index `{index}` does not exist.") { }
    }

    public class BlockDoesNotExistException : VmException
    {
        public BlockDoesNotExistException(int index) : base($"Block at index `{index}` does not exist.") { }
    }

    public class CallStackUnderflowException : VmException
    {
        public CallStackUnderflowException() : base("The call stack has underflowed.") { }
    }

    public class CallStackOverflowException : VmException
    {
        public CallStackOverflowException(int count) : base($"The call stack has overflowed ({count} calls).") { }
    }

    public class InstructionNotSupportedException : VmException
    {
        public InstructionNotSupportedException(string opName) : base($"Instruction `{opName}` is not supported.")
        {
            OpName = opName;
        }

        public string OpName { get; }
    }

    // Heap

    public class HeapOverflowException : VmException
    {
        public HeapOverflowException() : base($"The managed heap has overflowed.") { }
    }

    public class ObjectHeaderTypeMismatch : VmException
    {
        public ObjectHeaderTypeMismatch(ReferenceType expected, ReferenceType actual) : base($"Expected reference type `{expected}`, but got `{actual}`")
        {
            Expected = expected;
            Actual = actual;
        }

        public ReferenceType Expected { get; }
        public ReferenceType Actual { get; }
    }

    public class IndexOutOfBoundsException : VmException
    {
        public IndexOutOfBoundsException(int index, int length) : base($"Index {index} out of bounds for length {length}")
        {
            Index = index;
            Length = length;
        }

        public int Index { get; }
        public int Length { get; }
    }

    public class InvalidFieldException : VmException
    {
        public InvalidFieldException(int structInfoIndex, int field) : base($"Field at index {field} does not exist for struct at index {structInfoIndex}")
        {
            StructInfoIndex = structInfoIndex;
            Field = field;
        }

        public int StructInfoIndex { get; }
        public int Field { get; }
    }
}
