using vm.lib.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace vm.lib
{
    public ref struct ValueStack
    {
        public Span<Word> Data;
        public int Sp;

        public ValueStack(int size)
        {
            var data = new Word[size];
            Data = new Span<Word>(data);
            Sp = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Word Index(int index)
        {
            if (index >= Data.Length || index < 0)
            {
                throw new StackPointerOutOfRangeException();
            }

            return Data[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Word Peek()
        {
            return Index(Sp - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Word Peek(int index)
        {
            return Index(Sp - 1 - index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(Word word)
        {
            if (Sp < Data.Length)
            {
                Data[Sp] = word;
                Sp++;
            }
            else
            {
                throw new StackOverflowException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Word Pop()
        {
            var word = Peek();
            Sp -= 1;
            return word;
        }
    }
}
