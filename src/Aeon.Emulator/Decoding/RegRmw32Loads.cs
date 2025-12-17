using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Aeon.Emulator.Decoding;

/// <summary>
/// Handles 32-bit address resolution.
/// </summary>
/// <remarks>
/// This must be explictly initialized by calling the <see cref="Initialize"/> method.
/// </remarks>
internal static class RegRmw32Loads
{
    private static unsafe delegate*<Processor, uint>* loadAddressMethods;
    private static unsafe delegate*<Processor, uint>* loadOffsetMethods;
    private static bool initialized;

    /// <summary>
    /// Initializes all of the lookups used by this class.
    /// </summary>
    /// <remarks>
    /// This must be called before it's used.
    /// </remarks>
    public static void Initialize()
    {
        if (initialized)
            return;

        unsafe
        {
            loadAddressMethods = (delegate*<Processor, uint>*)NativeMemory.Alloc(48, (nuint)sizeof(nuint));
            loadAddressMethods[(0 * 8) + 0] = &LoadMod0_0;
            loadAddressMethods[(0 * 8) + 1] = &LoadMod0_1;
            loadAddressMethods[(0 * 8) + 2] = &LoadMod0_2;
            loadAddressMethods[(0 * 8) + 3] = &LoadMod0_3;
            loadAddressMethods[(0 * 8) + 4] = &LoadMod0_4;
            loadAddressMethods[(0 * 8) + 5] = &LoadMod0_5;
            loadAddressMethods[(0 * 8) + 6] = &LoadMod0_6;
            loadAddressMethods[(0 * 8) + 7] = &LoadMod0_7;

            loadAddressMethods[(1 * 8) + 0] = &LoadMod1_0;
            loadAddressMethods[(1 * 8) + 1] = &LoadMod1_1;
            loadAddressMethods[(1 * 8) + 2] = &LoadMod1_2;
            loadAddressMethods[(1 * 8) + 3] = &LoadMod1_3;
            loadAddressMethods[(1 * 8) + 4] = &LoadMod1_4;
            loadAddressMethods[(1 * 8) + 5] = &LoadMod1_5;
            loadAddressMethods[(1 * 8) + 6] = &LoadMod1_6;
            loadAddressMethods[(1 * 8) + 7] = &LoadMod1_7;

            loadAddressMethods[(2 * 8) + 0] = &LoadMod2_0;
            loadAddressMethods[(2 * 8) + 1] = &LoadMod2_1;
            loadAddressMethods[(2 * 8) + 2] = &LoadMod2_2;
            loadAddressMethods[(2 * 8) + 3] = &LoadMod2_3;
            loadAddressMethods[(2 * 8) + 4] = &LoadMod2_4;
            loadAddressMethods[(2 * 8) + 5] = &LoadMod2_5;
            loadAddressMethods[(2 * 8) + 6] = &LoadMod2_6;
            loadAddressMethods[(2 * 8) + 7] = &LoadMod2_7;

            loadOffsetMethods = &loadAddressMethods[24];
            loadOffsetMethods[(0 * 8) + 0] = &LoadMod0_0_Offset;
            loadOffsetMethods[(0 * 8) + 1] = &LoadMod0_1_Offset;
            loadOffsetMethods[(0 * 8) + 2] = &LoadMod0_2_Offset;
            loadOffsetMethods[(0 * 8) + 3] = &LoadMod0_3_Offset;
            loadOffsetMethods[(0 * 8) + 4] = &LoadMod0_4_Offset;
            loadOffsetMethods[(0 * 8) + 5] = &LoadMod0_5_Offset;
            loadOffsetMethods[(0 * 8) + 6] = &LoadMod0_6_Offset;
            loadOffsetMethods[(0 * 8) + 7] = &LoadMod0_7_Offset;

            loadOffsetMethods[(1 * 8) + 0] = &LoadMod1_0_Offset;
            loadOffsetMethods[(1 * 8) + 1] = &LoadMod1_1_Offset;
            loadOffsetMethods[(1 * 8) + 2] = &LoadMod1_2_Offset;
            loadOffsetMethods[(1 * 8) + 3] = &LoadMod1_3_Offset;
            loadOffsetMethods[(1 * 8) + 4] = &LoadMod1_4_Offset;
            loadOffsetMethods[(1 * 8) + 5] = &LoadMod1_5_Offset;
            loadOffsetMethods[(1 * 8) + 6] = &LoadMod1_6_Offset;
            loadOffsetMethods[(1 * 8) + 7] = &LoadMod1_7_Offset;

            loadOffsetMethods[(2 * 8) + 0] = &LoadMod2_0_Offset;
            loadOffsetMethods[(2 * 8) + 1] = &LoadMod2_1_Offset;
            loadOffsetMethods[(2 * 8) + 2] = &LoadMod2_2_Offset;
            loadOffsetMethods[(2 * 8) + 3] = &LoadMod2_3_Offset;
            loadOffsetMethods[(2 * 8) + 4] = &LoadMod2_4_Offset;
            loadOffsetMethods[(2 * 8) + 5] = &LoadMod2_5_Offset;
            loadOffsetMethods[(2 * 8) + 6] = &LoadMod2_6_Offset;
            loadOffsetMethods[(2 * 8) + 7] = &LoadMod2_7_Offset;
        }

        initialized = true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint LoadAddress(int rm, int mod, Processor processor)
    {
        unsafe
        {
            return loadAddressMethods[(mod << 3) | rm](processor);
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint LoadOffset(int rm, int mod, Processor processor)
    {
        unsafe
        {
            return loadOffsetMethods[(mod << 3) | rm](processor);
        }
    }

    private static uint LoadMod0_0(Processor p) => RuntimeCalls.GetModRMAddress32(p, 0, 0, false);
    private static uint LoadMod0_0_Offset(Processor p) => RuntimeCalls.GetModRMAddress32(p, 0, 0, true);
    private static uint LoadMod0_1(Processor p) => RuntimeCalls.GetModRMAddress32(p, 0, 1, false);
    private static uint LoadMod0_1_Offset(Processor p) => RuntimeCalls.GetModRMAddress32(p, 0, 1, true);
    private static uint LoadMod0_2(Processor p) => RuntimeCalls.GetModRMAddress32(p, 0, 2, false);
    private static uint LoadMod0_2_Offset(Processor p) => RuntimeCalls.GetModRMAddress32(p, 0, 2, true);
    private static uint LoadMod0_3(Processor p) => RuntimeCalls.GetModRMAddress32(p, 0, 3, false);
    private static uint LoadMod0_3_Offset(Processor p) => RuntimeCalls.GetModRMAddress32(p, 0, 3, true);
    private static uint LoadMod0_4(Processor p) => RuntimeCalls.GetModRMAddress32(p, 0, 4, false);
    private static uint LoadMod0_4_Offset(Processor p) => RuntimeCalls.GetModRMAddress32(p, 0, 4, true);
    private static uint LoadMod0_5(Processor p) => RuntimeCalls.GetModRMAddress32(p, 0, 5, false);
    private static uint LoadMod0_5_Offset(Processor p) => RuntimeCalls.GetModRMAddress32(p, 0, 5, true);
    private static uint LoadMod0_6(Processor p) => RuntimeCalls.GetModRMAddress32(p, 0, 6, false);
    private static uint LoadMod0_6_Offset(Processor p) => RuntimeCalls.GetModRMAddress32(p, 0, 6, true);
    private static uint LoadMod0_7(Processor p) => RuntimeCalls.GetModRMAddress32(p, 0, 7, false);
    private static uint LoadMod0_7_Offset(Processor p) => RuntimeCalls.GetModRMAddress32(p, 0, 7, true);

    private static uint LoadMod1_0(Processor p) => RuntimeCalls.GetModRMAddress32(p, 1, 0, false);
    private static uint LoadMod1_0_Offset(Processor p) => RuntimeCalls.GetModRMAddress32(p, 1, 0, true);
    private static uint LoadMod1_1(Processor p) => RuntimeCalls.GetModRMAddress32(p, 1, 1, false);
    private static uint LoadMod1_1_Offset(Processor p) => RuntimeCalls.GetModRMAddress32(p, 1, 1, true);
    private static uint LoadMod1_2(Processor p) => RuntimeCalls.GetModRMAddress32(p, 1, 2, false);
    private static uint LoadMod1_2_Offset(Processor p) => RuntimeCalls.GetModRMAddress32(p, 1, 2, true);
    private static uint LoadMod1_3(Processor p) => RuntimeCalls.GetModRMAddress32(p, 1, 3, false);
    private static uint LoadMod1_3_Offset(Processor p) => RuntimeCalls.GetModRMAddress32(p, 1, 3, true);
    private static uint LoadMod1_4(Processor p) => RuntimeCalls.GetModRMAddress32(p, 1, 4, false);
    private static uint LoadMod1_4_Offset(Processor p) => RuntimeCalls.GetModRMAddress32(p, 1, 4, true);
    private static uint LoadMod1_5(Processor p) => RuntimeCalls.GetModRMAddress32(p, 1, 5, false);
    private static uint LoadMod1_5_Offset(Processor p) => RuntimeCalls.GetModRMAddress32(p, 1, 5, true);
    private static uint LoadMod1_6(Processor p) => RuntimeCalls.GetModRMAddress32(p, 1, 6, false);
    private static uint LoadMod1_6_Offset(Processor p) => RuntimeCalls.GetModRMAddress32(p, 1, 6, true);
    private static uint LoadMod1_7(Processor p) => RuntimeCalls.GetModRMAddress32(p, 1, 7, false);
    private static uint LoadMod1_7_Offset(Processor p) => RuntimeCalls.GetModRMAddress32(p, 1, 7, true);

    private static uint LoadMod2_0(Processor p) => RuntimeCalls.GetModRMAddress32(p, 2, 0, false);
    private static uint LoadMod2_0_Offset(Processor p) => RuntimeCalls.GetModRMAddress32(p, 2, 0, true);
    private static uint LoadMod2_1(Processor p) => RuntimeCalls.GetModRMAddress32(p, 2, 1, false);
    private static uint LoadMod2_1_Offset(Processor p) => RuntimeCalls.GetModRMAddress32(p, 2, 1, true);
    private static uint LoadMod2_2(Processor p) => RuntimeCalls.GetModRMAddress32(p, 2, 2, false);
    private static uint LoadMod2_2_Offset(Processor p) => RuntimeCalls.GetModRMAddress32(p, 2, 2, true);
    private static uint LoadMod2_3(Processor p) => RuntimeCalls.GetModRMAddress32(p, 2, 3, false);
    private static uint LoadMod2_3_Offset(Processor p) => RuntimeCalls.GetModRMAddress32(p, 2, 3, true);
    private static uint LoadMod2_4(Processor p) => RuntimeCalls.GetModRMAddress32(p, 2, 4, false);
    private static uint LoadMod2_4_Offset(Processor p) => RuntimeCalls.GetModRMAddress32(p, 2, 4, true);
    private static uint LoadMod2_5(Processor p) => RuntimeCalls.GetModRMAddress32(p, 2, 5, false);
    private static uint LoadMod2_5_Offset(Processor p) => RuntimeCalls.GetModRMAddress32(p, 2, 5, true);
    private static uint LoadMod2_6(Processor p) => RuntimeCalls.GetModRMAddress32(p, 2, 6, false);
    private static uint LoadMod2_6_Offset(Processor p) => RuntimeCalls.GetModRMAddress32(p, 2, 6, true);
    private static uint LoadMod2_7(Processor p) => RuntimeCalls.GetModRMAddress32(p, 2, 7, false);
    private static uint LoadMod2_7_Offset(Processor p) => RuntimeCalls.GetModRMAddress32(p, 2, 7, true);
}
