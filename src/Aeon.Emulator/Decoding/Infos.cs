using System.Reflection;
using System.Runtime.Intrinsics.X86;

namespace Aeon.Emulator.Decoding
{
    internal static class Infos
    {
        public static class Processor
        {
            #region Fields
            public static readonly FieldInfo PAX = typeof(Emulator.Processor).GetField(nameof(Emulator.Processor.PAX), BindingFlags.Instance | BindingFlags.NonPublic);
            public static readonly FieldInfo PBX = typeof(Emulator.Processor).GetField(nameof(Emulator.Processor.PBX), BindingFlags.Instance | BindingFlags.NonPublic);
            public static readonly FieldInfo PCX = typeof(Emulator.Processor).GetField(nameof(Emulator.Processor.PCX), BindingFlags.Instance | BindingFlags.NonPublic);
            public static readonly FieldInfo PDX = typeof(Emulator.Processor).GetField(nameof(Emulator.Processor.PDX), BindingFlags.Instance | BindingFlags.NonPublic);

            public static readonly FieldInfo PAH = typeof(Emulator.Processor).GetField(nameof(Emulator.Processor.PAH), BindingFlags.Instance | BindingFlags.NonPublic);
            public static readonly FieldInfo PBH = typeof(Emulator.Processor).GetField(nameof(Emulator.Processor.PBH), BindingFlags.Instance | BindingFlags.NonPublic);
            public static readonly FieldInfo PCH = typeof(Emulator.Processor).GetField(nameof(Emulator.Processor.PCH), BindingFlags.Instance | BindingFlags.NonPublic);
            public static readonly FieldInfo PDH = typeof(Emulator.Processor).GetField(nameof(Emulator.Processor.PDH), BindingFlags.Instance | BindingFlags.NonPublic);

            public static readonly FieldInfo PBP = typeof(Emulator.Processor).GetField(nameof(Emulator.Processor.PBP), BindingFlags.Instance | BindingFlags.NonPublic);
            public static readonly FieldInfo PSI = typeof(Emulator.Processor).GetField(nameof(Emulator.Processor.PSI), BindingFlags.Instance | BindingFlags.NonPublic);
            public static readonly FieldInfo PDI = typeof(Emulator.Processor).GetField(nameof(Emulator.Processor.PDI), BindingFlags.Instance | BindingFlags.NonPublic);
            public static readonly FieldInfo PSP = typeof(Emulator.Processor).GetField(nameof(Emulator.Processor.PSP), BindingFlags.Instance | BindingFlags.NonPublic);

            public static readonly FieldInfo PIP = typeof(Emulator.Processor).GetField(nameof(Emulator.Processor.PIP), BindingFlags.Instance | BindingFlags.NonPublic);

            public static readonly PropertyInfo CSBase = typeof(Emulator.Processor).GetProperty(nameof(Emulator.Processor.CSBase), BindingFlags.Instance | BindingFlags.Public);

            public static readonly FieldInfo FPU = typeof(Emulator.Processor).GetField(nameof(Emulator.Processor.FPU), BindingFlags.Instance | BindingFlags.Public);

            public static readonly FieldInfo CachedIP = typeof(Emulator.Processor).GetField(nameof(Emulator.Processor.CachedIP), BindingFlags.Instance | BindingFlags.NonPublic);
            public static readonly FieldInfo CachedInstruction = typeof(Emulator.Processor).GetField(nameof(Emulator.Processor.CachedInstruction), BindingFlags.Instance | BindingFlags.NonPublic);
            public static readonly FieldInfo StartEIP = typeof(Emulator.Processor).GetField(nameof(Emulator.Processor.StartEIP), BindingFlags.Instance | BindingFlags.NonPublic);

            public static readonly FieldInfo BaseOverrides = typeof(Emulator.Processor).GetField(nameof(Emulator.Processor.baseOverrides), BindingFlags.Instance | BindingFlags.NonPublic);
            public static readonly FieldInfo DefaultSegments16 = typeof(Emulator.Processor).GetField(nameof(Emulator.Processor.defaultSegments16), BindingFlags.Instance | BindingFlags.NonPublic);
            public static readonly FieldInfo DefaultSibSegments32Mod12 = typeof(Emulator.Processor).GetField(nameof(Emulator.Processor.defaultSibSegments32Mod12), BindingFlags.Instance | BindingFlags.NonPublic);
            public static readonly FieldInfo SegmentBases = typeof(Emulator.Processor).GetField(nameof(Emulator.Processor.segmentBases), BindingFlags.Instance | BindingFlags.NonPublic);
            #endregion

            #region Properties
            public static readonly PropertyInfo SizeModeIndex = typeof(Emulator.Processor).GetProperty(nameof(Emulator.Processor.SizeModeIndex), BindingFlags.Instance | BindingFlags.NonPublic);
            public static readonly PropertyInfo SegmentOverride = typeof(Emulator.Processor).GetProperty(nameof(Emulator.Processor.SegmentOverride), BindingFlags.Instance | BindingFlags.Public);
            #endregion

            #region Methods
            public static readonly MethodInfo InstructionEpilog = typeof(Emulator.Processor).GetMethod(nameof(Emulator.Processor.InstructionEpilog), BindingFlags.Instance | BindingFlags.NonPublic);
            public static readonly MethodInfo GetSegmentRegisterPointer = typeof(Emulator.Processor).GetMethod(nameof(Emulator.Processor.GetSegmentRegisterPointer), BindingFlags.Instance | BindingFlags.NonPublic);
            public static readonly MethodInfo GetRegisterWordPointer = typeof(Emulator.Processor).GetMethod(nameof(Emulator.Processor.GetRegisterWordPointer), BindingFlags.Instance | BindingFlags.NonPublic);
            public static readonly MethodInfo GetRegisterBytePointer = typeof(Emulator.Processor).GetMethod(nameof(Emulator.Processor.GetRegisterBytePointer), BindingFlags.Instance | BindingFlags.NonPublic);
            public static readonly MethodInfo GetDebugRegisterPointer = typeof(Emulator.Processor).GetMethod(nameof(Emulator.Processor.GetDebugRegisterPointer), BindingFlags.Instance | BindingFlags.NonPublic);
            public static readonly MethodInfo GetOverrideBase = typeof(Emulator.Processor).GetMethod(nameof(Emulator.Processor.GetOverrideBase), BindingFlags.Instance | BindingFlags.NonPublic);
            public static readonly MethodInfo GetRM16Offset = typeof(Emulator.Processor).GetMethod(nameof(Emulator.Processor.GetRM16Offset), BindingFlags.Instance | BindingFlags.NonPublic);
            #endregion
        }

        public static class FPU
        {
            #region Methods
            public static readonly MethodInfo GetRegisterValue = typeof(Emulator.FPU).GetMethod(nameof(Emulator.FPU.GetRegisterValue), BindingFlags.Instance | BindingFlags.Public);
            #endregion
        }

        public static class VirtualMachine
        {
            #region Fields
            public static readonly FieldInfo Processor = typeof(Emulator.VirtualMachine).GetField(nameof(Emulator.VirtualMachine.Processor), BindingFlags.Instance | BindingFlags.Public);
            public static readonly FieldInfo PhysicalMemory = typeof(Emulator.VirtualMachine).GetField(nameof(Emulator.VirtualMachine.PhysicalMemory), BindingFlags.Instance | BindingFlags.Public);
            #endregion

            #region Methods
            public static readonly MethodInfo WriteSegmentRegister = typeof(Emulator.VirtualMachine).GetMethod(nameof(Emulator.VirtualMachine.WriteSegmentRegister), BindingFlags.Instance | BindingFlags.Public);
            public static readonly MethodInfo UpdateSegment = typeof(Emulator.VirtualMachine).GetMethod(nameof(Emulator.VirtualMachine.UpdateSegment), BindingFlags.Instance | BindingFlags.NonPublic);
            #endregion
        }

        public static class PhysicalMemory
        {
            #region Fields
            public static readonly FieldInfo RawView = typeof(Emulator.PhysicalMemory).GetField(nameof(Emulator.PhysicalMemory.RawView), BindingFlags.Instance | BindingFlags.NonPublic);
            #endregion

            #region Methods
            public static readonly MethodInfo GetByte = typeof(Emulator.PhysicalMemory).GetMethod(nameof(Emulator.PhysicalMemory.GetByte), new[] { typeof(uint) });
            public static readonly MethodInfo SetByte = typeof(Emulator.PhysicalMemory).GetMethod(nameof(Emulator.PhysicalMemory.SetByte), new[] { typeof(uint), typeof(byte) });

            public static readonly MethodInfo GetUInt16 = typeof(Emulator.PhysicalMemory).GetMethod(nameof(Emulator.PhysicalMemory.GetUInt16), new[] { typeof(uint) });
            public static readonly MethodInfo SetUInt16 = typeof(Emulator.PhysicalMemory).GetMethod(nameof(Emulator.PhysicalMemory.SetUInt16), new[] { typeof(uint), typeof(ushort) });

            public static readonly MethodInfo GetUInt32 = typeof(Emulator.PhysicalMemory).GetMethod(nameof(Emulator.PhysicalMemory.GetUInt32), new[] { typeof(uint) });
            public static readonly MethodInfo SetUInt32 = typeof(Emulator.PhysicalMemory).GetMethod(nameof(Emulator.PhysicalMemory.SetUInt32), new[] { typeof(uint), typeof(uint) });

            public static readonly MethodInfo GetUInt64 = typeof(Emulator.PhysicalMemory).GetMethod(nameof(Emulator.PhysicalMemory.GetUInt64), new[] { typeof(uint) });
            public static readonly MethodInfo SetUInt64 = typeof(Emulator.PhysicalMemory).GetMethod(nameof(Emulator.PhysicalMemory.SetUInt64), new[] { typeof(uint), typeof(ulong) });

            public static readonly MethodInfo GetReal32 = typeof(Emulator.PhysicalMemory).GetMethod(nameof(Emulator.PhysicalMemory.GetReal32), new[] { typeof(uint) });
            public static readonly MethodInfo SetReal32 = typeof(Emulator.PhysicalMemory).GetMethod(nameof(Emulator.PhysicalMemory.SetReal32), new[] { typeof(uint), typeof(float) });

            public static readonly MethodInfo GetReal64 = typeof(Emulator.PhysicalMemory).GetMethod(nameof(Emulator.PhysicalMemory.GetReal64), new[] { typeof(uint) });
            public static readonly MethodInfo SetReal64 = typeof(Emulator.PhysicalMemory).GetMethod(nameof(Emulator.PhysicalMemory.SetReal64), new[] { typeof(uint), typeof(double) });

            public static readonly MethodInfo GetReal80 = typeof(Emulator.PhysicalMemory).GetMethod(nameof(Emulator.PhysicalMemory.GetReal80), new[] { typeof(uint) });
            public static readonly MethodInfo SetReal80 = typeof(Emulator.PhysicalMemory).GetMethod(nameof(Emulator.PhysicalMemory.SetReal80), new[] { typeof(uint), typeof(Real10) });

            public static readonly MethodInfo FetchInstruction = typeof(Emulator.PhysicalMemory).GetMethod(nameof(Emulator.PhysicalMemory.FetchInstruction), BindingFlags.Instance | BindingFlags.NonPublic);
            #endregion
        }

        public static class RuntimeCalls
        {
            #region Methods
            public static readonly MethodInfo NewLoadSibMod0Address = typeof(Decoding.RuntimeCalls).GetMethod(nameof(Decoding.RuntimeCalls.NewLoadSibMod0Address));
            public static readonly MethodInfo NewLoadSibMod12Address = typeof(Decoding.RuntimeCalls).GetMethod(nameof(Decoding.RuntimeCalls.NewLoadSibMod12Address));
            public static readonly MethodInfo NewLoadSibMod0Offset = typeof(Decoding.RuntimeCalls).GetMethod(nameof(Decoding.RuntimeCalls.NewLoadSibMod0Offset));
            public static readonly MethodInfo NewLoadSibMod12Offset = typeof(Decoding.RuntimeCalls).GetMethod(nameof(Decoding.RuntimeCalls.NewLoadSibMod12Offset));
            public static readonly MethodInfo GetMoffsAddress32 = typeof(Decoding.RuntimeCalls).GetMethod(nameof(Decoding.RuntimeCalls.GetMoffsAddress32), BindingFlags.Static | BindingFlags.Public);
            public static readonly MethodInfo GetModRMAddress32 = typeof(Decoding.RuntimeCalls).GetMethod(nameof(Decoding.RuntimeCalls.GetModRMAddress32), BindingFlags.Static | BindingFlags.Public);
            public static readonly MethodInfo ThrowException = typeof(Decoding.RuntimeCalls).GetMethod(nameof(Decoding.RuntimeCalls.ThrowException), BindingFlags.Static | BindingFlags.Public);
            #endregion
        }

        public static class Intrinsics
        {
            public static readonly MethodInfo BitFieldExtract = typeof(Bmi1).GetMethod(nameof(Bmi1.BitFieldExtract), new[] { typeof(uint), typeof(ushort) });
        }
    }
}
