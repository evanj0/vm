using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace vm.lib;

public struct Frame
{
    public Frame(int returnAddr, int baseSp, ProcInfo procInfo)
    {
        ReturnAddr = returnAddr;
        BaseSp = baseSp;
        ProcInfo = procInfo;
    }
    public int ReturnAddr;

    public int BaseSp;

    public ProcInfo ProcInfo;

    /// <summary>
    /// Same as BaseSp.
    /// </summary>
    public int ArgsSp
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => BaseSp;
    }

    public int LocalsSp
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => BaseSp + ProcInfo.NumParams;
    }

    /// <summary>
    /// sp after args and locals
    /// </summary>
    public int Sp
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => BaseSp + ProcInfo.NumParams + ProcInfo.NumLocals;
    }
}
