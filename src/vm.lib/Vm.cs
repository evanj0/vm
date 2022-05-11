using vm.lib.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vm.lib
{
    public ref struct Vm
    {
        public ValueStack Stack;
        public Stack<Frame> Frames;
        public int Ip;

        public string Debug()
        {
            var sb = new StringBuilder();
            if (Frames.Count > 0)
            {
                var frame = Frames.Peek();
                sb.AppendLine("[current procedure]");
                sb.AppendLine(DebugProcInfo(frame));
                for (var i = frame.ArgsSp; i < frame.LocalsSp; i++)
                {
                    sb.AppendLine($"arg.{i - frame.ArgsSp} i64={Stack.Data[i].ToI64()}");
                }
                for (var i = frame.LocalsSp; i < frame.Sp; i++)
                {
                    sb.AppendLine($"local.{i - frame.LocalsSp} i64={Stack.Data[i].ToI64()}");
                }
                sb.AppendLine("[call stack]");
                var framesPrinted = 0;
                sb.Append("-> ");
                while (Frames.Count > 0 && framesPrinted < 8)
                {
                    frame = Frames.Pop();
                    sb.AppendLine($"frame.{framesPrinted} ret={frame.ReturnAddr} proc=({DebugProcInfo(frame)})");
                    framesPrinted++;
                }
            }
            return sb.ToString();
        }

        private string DebugProcInfo(Frame frame) =>
            $"numParams={frame.ProcInfo.NumParams} numLocals={frame.ProcInfo.NumLocals} addr={frame.ProcInfo.Addr}";
    }
}
