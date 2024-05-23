using System.Runtime.InteropServices;

namespace p3rpc.expShare;
internal unsafe class Native
{

    [StructLayout(LayoutKind.Explicit)]
    internal struct UGlobalWork
    {
        [FieldOffset(0xa378)]
        internal int Date;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct ABtlPhaseResult
    {
        [FieldOffset(0x2c6)]
        internal short NumInParty;

        [FieldOffset(0x2b2)]
        internal fixed short Party[10];

        [FieldOffset(0x2cc)]
        internal fixed int EarnedExp[11];
    }

    internal enum PartyMember
    {
        None = -1,
        Hero = 0,
        Yukari = 19,
        Junpei = 19,
        Akihiko = 52,
        Mitsuru = 73,
        Fuuka = 73,
        Aigis = 114,
        Ken = 149,
        Koromaru = 129,
        Shinjiro = 115
    }

    internal const int ShinjiroDeath = 186;
    internal static IEnumerable<T> GetValues<T>()
    {
        return Enum.GetValues(typeof(T)).Cast<T>();
    }

    internal delegate void SetupPartyExpDelegate(ABtlPhaseResult* result);
    internal delegate UGlobalWork* GetUGlobalWorkDelegate();
}
