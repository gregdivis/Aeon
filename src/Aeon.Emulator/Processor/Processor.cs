using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Aeon.Emulator.DebugSupport;

namespace Aeon.Emulator
{
    /// <summary>
    /// Contains the current state of the emulated x86 processor.
    /// </summary>
    public sealed class Processor : IRegisterContainer
    {
        private PrefixOverrides overrides;
        private readonly unsafe short** rmOffsets1;
        private readonly unsafe short** rmOffsets2;

        internal Processor()
        {
            unsafe
            {
                gprBlock = (byte*)(Marshal.AllocCoTaskMem((sizeof(uint) * RegisterCount) + InstructionCacheSize));
                for (int i = 0; i < (sizeof(uint) * RegisterCount) + InstructionCacheSize; i++)
                    gprBlock[i] = 0;

                wordRegisterPointers = (void**)Marshal.AllocCoTaskMem(sizeof(void*) * 8).ToPointer();
                byteRegisterPointers = (byte**)Marshal.AllocCoTaskMem(sizeof(byte*) * 8).ToPointer();
                segmentRegisterPointers = (ushort**)Marshal.AllocCoTaskMem(sizeof(ushort*) * 8).ToPointer();
                defaultSegments16 = (uint**)Marshal.AllocCoTaskMem(sizeof(uint*) * 8).ToPointer();
                segmentBases = (uint*)Marshal.AllocCoTaskMem(sizeof(uint) * 8).ToPointer();
                defaultSibSegments32Mod0 = (uint**)Marshal.AllocCoTaskMem(sizeof(uint*) * 8).ToPointer();
                defaultSibSegments32Mod12 = (uint**)Marshal.AllocCoTaskMem(sizeof(uint*) * 8).ToPointer();
                for (int i = 0; i < 8; i++)
                    segmentBases[i] = 0;

                InitializeRegisterOffsets();
                PAX = wordRegisterPointers[0];
                PCX = wordRegisterPointers[1];
                PDX = wordRegisterPointers[2];
                PBX = wordRegisterPointers[3];

                PAH = byteRegisterPointers[4];
                PCH = byteRegisterPointers[5];
                PDH = byteRegisterPointers[6];
                PBH = byteRegisterPointers[7];

                PIP = gprBlock + 32;

                PSP = wordRegisterPointers[4];
                PBP = wordRegisterPointers[5];
                PSI = wordRegisterPointers[6];
                PDI = wordRegisterPointers[7];

                PES = segmentRegisterPointers[0];
                PCS = segmentRegisterPointers[1];
                PSS = segmentRegisterPointers[2];
                PDS = segmentRegisterPointers[3];
                PFS = segmentRegisterPointers[4];
                PGS = segmentRegisterPointers[5];

                baseOverrides = (uint**)Marshal.AllocCoTaskMem(sizeof(uint*) * 8).ToPointer();
                InitializeSegmentOverridePointers();
                InitializeDefaultSegmentPointers();
                debugRegisterBase = (uint*)(gprBlock + 60); // DR0
                this.CachedInstruction = gprBlock + (RegisterCount * sizeof(uint));

                rmOffsets1 = (short**)Marshal.AllocCoTaskMem(sizeof(short*) * 17).ToPointer();
                new Span<IntPtr>(rmOffsets1, 17).Clear();
                rmOffsets1[0] = (short*)this.PBX;
                rmOffsets1[1] = (short*)this.PBX;
                rmOffsets1[2] = (short*)this.PBP;
                rmOffsets1[3] = (short*)this.PBP;
                rmOffsets1[4] = (short*)this.PSI;
                rmOffsets1[5] = (short*)this.PDI;
                rmOffsets1[6] = (short*)this.PBP;
                rmOffsets1[7] = (short*)this.PBX;
                rmOffsets2 = &rmOffsets1[8];
                rmOffsets2[0] = (short*)this.PSI;
                rmOffsets2[1] = (short*)this.PDI;
                rmOffsets2[2] = (short*)this.PSI;
                rmOffsets2[3] = (short*)this.PDI;
                rmOffsets2[4] = (short*)&rmOffsets1[16];
                rmOffsets2[5] = (short*)&rmOffsets1[16];
                rmOffsets2[6] = (short*)&rmOffsets1[16];
                rmOffsets2[7] = (short*)&rmOffsets1[16];
            }
        }
        ~Processor()
        {
            unsafe
            {
                Marshal.FreeCoTaskMem(new IntPtr(rmOffsets1));
                Marshal.FreeCoTaskMem(new IntPtr(segmentBases));
                Marshal.FreeCoTaskMem(new IntPtr(baseOverrides));
                Marshal.FreeCoTaskMem(new IntPtr(wordRegisterPointers));
                Marshal.FreeCoTaskMem(new IntPtr(byteRegisterPointers));
                Marshal.FreeCoTaskMem(new IntPtr(segmentRegisterPointers));
                Marshal.FreeCoTaskMem(new IntPtr(defaultSegments16));
                Marshal.FreeCoTaskMem(new IntPtr(defaultSibSegments32Mod0));
                Marshal.FreeCoTaskMem(new IntPtr(defaultSibSegments32Mod12));
                Marshal.FreeCoTaskMem(new IntPtr(gprBlock));
            }
        }

        private unsafe void InitializeRegisterOffsets()
        {
            wordRegisterPointers[0] = gprBlock; // AX
            byteRegisterPointers[0] = gprBlock; // AL
            byteRegisterPointers[4] = gprBlock + 1; // AH

            wordRegisterPointers[1] = gprBlock + 4; // CX
            byteRegisterPointers[1] = gprBlock + 4; // CL
            byteRegisterPointers[5] = gprBlock + 5; // CH

            wordRegisterPointers[2] = gprBlock + 8; // DX
            byteRegisterPointers[2] = gprBlock + 8; // DL
            byteRegisterPointers[6] = gprBlock + 9; // DH

            wordRegisterPointers[3] = gprBlock + 12; // BX
            byteRegisterPointers[3] = gprBlock + 12; // BL
            byteRegisterPointers[7] = gprBlock + 13; // BH

            wordRegisterPointers[4] = gprBlock + 16; // SP
            wordRegisterPointers[5] = gprBlock + 20; // BP
            wordRegisterPointers[6] = gprBlock + 24; // SI
            wordRegisterPointers[7] = gprBlock + 28; // DI

            // IP = 32

            segmentRegisterPointers[0] = (ushort*)(gprBlock + 36); // ES
            segmentRegisterPointers[1] = (ushort*)(gprBlock + 40); // CS
            segmentRegisterPointers[2] = (ushort*)(gprBlock + 44); // SS
            segmentRegisterPointers[3] = (ushort*)(gprBlock + 48); // DS
            segmentRegisterPointers[4] = (ushort*)(gprBlock + 52); // FS
            segmentRegisterPointers[5] = (ushort*)(gprBlock + 56); // GS
        }
        private unsafe void InitializeSegmentOverridePointers()
        {
            baseOverrides[(int)SegmentRegister.Default] = null;
            baseOverrides[(int)SegmentRegister.ES] = &segmentBases[0];
            baseOverrides[(int)SegmentRegister.CS] = &segmentBases[1];
            baseOverrides[(int)SegmentRegister.SS] = &segmentBases[2];
            baseOverrides[(int)SegmentRegister.DS] = &segmentBases[3];
            baseOverrides[(int)SegmentRegister.FS] = &segmentBases[4];
            baseOverrides[(int)SegmentRegister.GS] = &segmentBases[5];
        }
        private unsafe void InitializeDefaultSegmentPointers()
        {
            defaultSegments16[0] = &segmentBases[3];
            defaultSegments16[1] = &segmentBases[3];
            defaultSegments16[2] = &segmentBases[2];
            defaultSegments16[3] = &segmentBases[2];
            defaultSegments16[4] = &segmentBases[3];
            defaultSegments16[5] = &segmentBases[3];
            defaultSegments16[6] = &segmentBases[2];
            defaultSegments16[7] = &segmentBases[3];

            defaultSibSegments32Mod0[0] = &segmentBases[3];
            defaultSibSegments32Mod0[1] = &segmentBases[3];
            defaultSibSegments32Mod0[2] = &segmentBases[3];
            defaultSibSegments32Mod0[3] = &segmentBases[3];
            defaultSibSegments32Mod0[4] = &segmentBases[2];
            defaultSibSegments32Mod0[5] = &segmentBases[3];
            defaultSibSegments32Mod0[6] = &segmentBases[3];
            defaultSibSegments32Mod0[7] = &segmentBases[3];

            defaultSibSegments32Mod12[0] = &segmentBases[3];
            defaultSibSegments32Mod12[1] = &segmentBases[3];
            defaultSibSegments32Mod12[2] = &segmentBases[3];
            defaultSibSegments32Mod12[3] = &segmentBases[3];
            defaultSibSegments32Mod12[4] = &segmentBases[2];
            defaultSibSegments32Mod12[5] = &segmentBases[2];
            defaultSibSegments32Mod12[6] = &segmentBases[3];
            defaultSibSegments32Mod12[7] = &segmentBases[3];
        }

        #region General Purpose
        /// <summary>
        /// Pointer to the EAX/AX/AL register.
        /// </summary>
        internal readonly unsafe void* PAX;
        /// <summary>
        /// Pointer to the EBX/BX/BL register.
        /// </summary>
        internal readonly unsafe void* PBX;
        /// <summary>
        /// Pointer to the ECX/CX/CL register.
        /// </summary>
        internal readonly unsafe void* PCX;
        /// <summary>
        /// Pointer to the EDX/DX/DL register.
        /// </summary>
        internal readonly unsafe void* PDX;
        /// <summary>
        /// Pointer to the AH register.
        /// </summary>
        internal readonly unsafe byte* PAH;
        /// <summary>
        /// Pointer to the BH register.
        /// </summary>
        internal readonly unsafe byte* PBH;
        /// <summary>
        /// Pointer to the CH register.
        /// </summary>
        internal readonly unsafe byte* PCH;
        /// <summary>
        /// Pointer to the DH register.
        /// </summary>
        internal readonly unsafe byte* PDH;

        /// <summary>
        /// Gets or sets the value of the EAX register.
        /// </summary>
        public ref int EAX
        {
            get { unsafe { return ref *(int*)PAX; } }
        }
        /// <summary>
        /// Gets the value of the EAX register.
        /// </summary>
        uint IRegisterContainer.EAX => (uint)this.EAX;
        /// <summary>
        /// Gets or sets the value of the EBX register.
        /// </summary>
        public ref int EBX
        {
            get { unsafe { return ref *(int*)PBX; } }
        }
        /// <summary>
        /// Gets the value of the EAX register.
        /// </summary>
        uint IRegisterContainer.EBX => (uint)this.EBX;
        /// <summary>
        /// Gets or sets the value of the ECX register.
        /// </summary>
        public ref int ECX
        {
            get { unsafe { return ref *(int*)PCX; } }
        }
        /// <summary>
        /// Gets the value of the EAX register.
        /// </summary>
        uint IRegisterContainer.ECX => (uint)this.ECX;
        /// <summary>
        /// Gets or sets the value of the EDX register.
        /// </summary>
        public ref int EDX
        {
            get { unsafe { return ref *(int*)PDX; } }
        }
        /// <summary>
        /// Gets the value of the EAX register.
        /// </summary>
        uint IRegisterContainer.EDX => (uint)this.EDX;
        /// <summary>
        /// Gets or sets the value of the AX register.
        /// </summary>
        public ref short AX
        {
            get { unsafe { return ref *(short*)PAX; } }
        }
        /// <summary>
        /// Gets or sets the value of the BX register.
        /// </summary>
        public ref short BX
        {
            get { unsafe { return ref *(short*)PBX; } }
        }
        /// <summary>
        /// Gets or sets the value of the CX register.
        /// </summary>
        public ref short CX
        {
            get { unsafe { return ref *(short*)PCX; } }
        }
        /// <summary>
        /// Gets or sets the value of the DX register.
        /// </summary>
        public ref short DX
        {
            get { unsafe { return ref *(short*)PDX; } }
        }

        /// <summary>
        /// Gets or sets the value of the AL register.
        /// </summary>
        public ref byte AL
        {
            get { unsafe { return ref *(byte*)PAX; } }
        }
        /// <summary>
        /// Gets or sets the value of the AH register.
        /// </summary>
        public ref byte AH
        {
            get { unsafe { return ref *PAH; } }
        }
        /// <summary>
        /// Gets or sets the value of the BL register.
        /// </summary>
        public ref byte BL
        {
            get { unsafe { return ref *(byte*)PBX; } }
        }
        /// <summary>
        /// Gets or sets the value of the BH register.
        /// </summary>
        public ref byte BH
        {
            get { unsafe { return ref *PBH; } }
        }
        /// <summary>
        /// Gets or sets the value of the CL register.
        /// </summary>
        public ref byte CL
        {
            get { unsafe { return ref *(byte*)PCX; } }
        }
        /// <summary>
        /// Gets or sets the value of the CH register.
        /// </summary>
        public ref byte CH
        {
            get { unsafe { return ref *PCH; } }
        }
        /// <summary>
        /// Gets or sets the value of the DL register.
        /// </summary>
        public ref byte DL
        {
            get { unsafe { return ref *(byte*)PDX; } }
        }
        /// <summary>
        /// Gets or sets the value of the DH register.
        /// </summary>
        public ref byte DH
        {
            get { unsafe { return ref *PDH; } }
        }
        #endregion

        #region Pointers
        /// <summary>
        /// Pointer to the EBP/BP register.
        /// </summary>
        internal unsafe readonly void* PBP;
        /// <summary>
        /// Pointer to the ESI/SI register.
        /// </summary>
        internal unsafe readonly void* PSI;
        /// <summary>
        /// Pointer to the EDI/DI register.
        /// </summary>
        internal unsafe readonly void* PDI;
        /// <summary>
        /// Pointer to the EIP/IP register.
        /// </summary>
        internal unsafe readonly void* PIP;
        /// <summary>
        /// Pointer to the ESP/SP register.
        /// </summary>
        internal unsafe readonly void* PSP;

        /// <summary>
        /// Gets or sets the value of the EBP register.
        /// </summary>
        public ref uint EBP
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { unsafe { return ref *(uint*)PBP; } }
        }
        uint IRegisterContainer.EBP => this.EBP;
        /// <summary>
        /// Gets or sets the value of the ESI register.
        /// </summary>
        public ref uint ESI
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { unsafe { return ref *(uint*)PSI; } }
        }
        uint IRegisterContainer.ESI => this.ESI;
        /// <summary>
        /// Gets or sets the value of the EDI register.
        /// </summary>
        public ref uint EDI
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { unsafe { return ref *(uint*)PDI; } }
        }
        uint IRegisterContainer.EDI => this.EDI;
        /// <summary>
        /// Gets or sets the value of the EIP register.
        /// </summary>
        public ref uint EIP
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { unsafe { return ref *(uint*)PIP; } }
        }
        /// <summary>
        /// Gets or sets the value of the ESP register.
        /// </summary>
        public ref uint ESP
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { unsafe { return ref *(uint*)PSP; } }
        }
        uint IRegisterContainer.ESP => this.ESP;

        /// <summary>
        /// Gets or sets the value of the BP register.
        /// </summary>
        public ref ushort BP
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { unsafe { return ref *(ushort*)PBP; } }
        }
        /// <summary>
        /// Gets or sets the value of the SI register.
        /// </summary>
        public ref ushort SI
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { unsafe { return ref *(ushort*)PSI; } }
        }
        /// <summary>
        /// Gets or sets the value of the DI register.
        /// </summary>
        public ref ushort DI
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { unsafe { return ref *(ushort*)PDI; } }
        }
        /// <summary>
        /// Gets or sets the value of the IP register.
        /// </summary>
        public ref ushort IP
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { unsafe { return ref *(ushort*)PIP; } }
        }
        /// <summary>
        /// Gets or sets the value of the SP register.
        /// </summary>
        public ref ushort SP
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { unsafe { return ref *(ushort*)PSP; } }
        }
        #endregion

        #region Segment Registers
        /// <summary>
        /// Pointer to the ES register.
        /// </summary>
        internal unsafe readonly ushort* PES;
        /// <summary>
        /// Pointer to the CS register.
        /// </summary>
        internal unsafe readonly ushort* PCS;
        /// <summary>
        /// Pointer to the SS register.
        /// </summary>
        internal unsafe readonly ushort* PSS;
        /// <summary>
        /// Pointer to the DS register.
        /// </summary>
        internal unsafe readonly ushort* PDS;
        /// <summary>
        /// Pointer to the FS register.
        /// </summary>
        internal unsafe readonly ushort* PFS;
        /// <summary>
        /// Pointer to the GS register.
        /// </summary>
        internal unsafe readonly ushort* PGS;

        /// <summary>
        /// Gets or sets the value of the ES register.
        /// </summary>
        public ushort ES
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { unsafe { return *PES; } }
        }
        /// <summary>
        /// Gets or sets the value of the CS register.
        /// </summary>
        public ushort CS
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { unsafe { return *PCS; } }
        }
        /// <summary>
        /// Gets or sets the value of the SS register.
        /// </summary>
        public ushort SS
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { unsafe { return *PSS; } }
        }
        /// <summary>
        /// Gets or sets the value of the DS register.
        /// </summary>
        public ushort DS
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { unsafe { return *PDS; } }
        }
        /// <summary>
        /// Gets or sets the value of the FS register.
        /// </summary>
        public ushort FS
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { unsafe { return *PFS; } }
        }
        /// <summary>
        /// Gets or sets the value of the GS register.
        /// </summary>
        public ushort GS
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { unsafe { return *PGS; } }
        }

        /// <summary>
        /// Gets the current base address associated with the ES register.
        /// </summary>
        public uint ESBase
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { unsafe { return segmentBases[0]; } }
        }
        /// <summary>
        /// Gets the current base address associated with the CS register.
        /// </summary>
        public uint CSBase
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { unsafe { return segmentBases[1]; } }
        }
        /// <summary>
        /// Gets the current base address associated with the SS register.
        /// </summary>
        public uint SSBase
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { unsafe { return segmentBases[2]; } }
        }
        /// <summary>
        /// Gets the current base address associated with the DS register.
        /// </summary>
        public uint DSBase
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { unsafe { return segmentBases[3]; } }
        }
        /// <summary>
        /// Gets the current base address associated with the FS register.
        /// </summary>
        public uint FSBase
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { unsafe { return segmentBases[4]; } }
        }
        /// <summary>
        /// Gets the current base address associated with the GS register.
        /// </summary>
        public uint GSBase
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { unsafe { return segmentBases[5]; } }
        }
        #endregion

        #region Other
        public readonly FlagState Flags = new FlagState();
        /// <summary>
        /// Gets the value of the EFLAGS register.
        /// </summary>
        EFlags IRegisterContainer.Flags => this.Flags.Value;
        /// <summary>
        /// The current segment override prefix.
        /// </summary>
        public SegmentRegister SegmentOverride
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            get => this.overrides.Segment;
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            set => this.overrides.Segment = value;
        }
        /// <summary>
        /// The current instruction repeat prefix.
        /// </summary>
        public RepeatPrefix RepeatPrefix
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            get => this.overrides.Repeat;
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            set => this.overrides.Repeat = value;
        }
        /// <summary>
        /// The value of the CR0 register.
        /// </summary>
        public CR0 CR0;
        /// <summary>
        /// Gets the value of the CR0 register.
        /// </summary>
        CR0 IRegisterContainer.CR0 => this.CR0;
        /// <summary>
        /// The value of the CR2 register.
        /// </summary>
        public uint CR2;
        /// <summary>
        /// The value of the CR3 register.
        /// </summary>
        public uint CR3;
        /// <summary>
        /// Gets the width of the current operands in bits.
        /// </summary>
        public int OperandSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                uint bit = ((uint)this.GlobalSize ^ this.SizeOverride) & 1u;
                return bit == 0 ? 16 : 32;
            }
        }
        /// <summary>
        /// Gets the width of the current addressing mode in bits.
        /// </summary>
        public int AddressSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                uint bit = ((uint)this.GlobalSize ^ this.SizeOverride) & 2u;
                return bit == 0 ? 16 : 32;
            }
        }
        #endregion

        #region FPU
        /// <summary>
        /// The floating-point unit.
        /// </summary>
        public readonly FPU FPU = new FPU();
        #endregion

        #region Internal Properties
        /// <summary>
        /// Gets the current index to use for decoders and emulators.
        /// </summary>
        internal uint SizeModeIndex
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            get => (uint)this.SizeOverride ^ this.GlobalSize;
        }

        /// <summary>
        /// Gets a value indicating whether an instruction prefix is in effect.
        /// </summary>
        internal bool InPrefix
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            get => this.PrefixCount != 0;
        }
        #endregion

        #region Internal Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal uint GetOverrideBase(SegmentIndex defaultSegment)
        {
            unsafe
            {
                uint* address = baseOverrides[(int)this.SegmentOverride];
                if (address != null)
                    return *address;
                else
                    return segmentBases[(int)defaultSegment];
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        internal ushort GetRM16Offset(int rm, ushort displacement)
        {
            unsafe
            {
                return (ushort)(*this.rmOffsets1[rm] + *this.rmOffsets2[rm] + displacement);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal unsafe byte* GetRegisterBytePointer(int rmCode) => byteRegisterPointers[rmCode];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal unsafe void* GetRegisterWordPointer(int rmCode) => wordRegisterPointers[rmCode];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal unsafe ushort* GetSegmentRegisterPointer(int code) => segmentRegisterPointers[code];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal unsafe uint* GetDebugRegisterPointer(int code) => &debugRegisterBase[code];

        /// <summary>
        /// Clears prefix information after an instruction.
        /// </summary>
        /// <remarks>
        /// This method must be called explicitly by instructions with no operands.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        internal void InstructionEpilog() => this.overrides = default;

        internal byte[] GetCurrentState()
        {
            unsafe
            {
                byte[] buffer = new byte[sizeof(uint) * (RegisterCount + 9)];
                Marshal.Copy(new IntPtr(gprBlock), buffer, 0, sizeof(uint) * RegisterCount);
                Marshal.Copy(new IntPtr(segmentBases), buffer, sizeof(uint) * RegisterCount, 8 * sizeof(uint));

                int flagsIndex = sizeof(uint) * (RegisterCount + 8);
                buffer[flagsIndex] = (byte)((uint)this.Flags.Value & 0xFF);
                buffer[flagsIndex + 1] = (byte)(((uint)this.Flags.Value >> 8) & 0xFF);
                buffer[flagsIndex + 2] = (byte)(((uint)this.Flags.Value >> 16) & 0xFF);
                buffer[flagsIndex + 3] = (byte)(((uint)this.Flags.Value >> 24) & 0xFF);

                return buffer;
            }
        }
        internal void SetCurrentState(byte[] state)
        {
            unsafe
            {
                Marshal.Copy(state, 0, new IntPtr(gprBlock), sizeof(uint) * RegisterCount);
                Marshal.Copy(state, sizeof(uint) * RegisterCount, new IntPtr(segmentBases), sizeof(uint) * 8);

                int flagsIndex = sizeof(uint) * (RegisterCount + 8);
                this.Flags.Value = (EFlags)(state[flagsIndex] | (state[flagsIndex + 1] << 8) | (state[flagsIndex + 2] << 16) | (state[flagsIndex + 3] << 24));
            }
        }
        #endregion

        #region Internal Fields
        /// <summary>
        /// Contains operand size (bit 0) and address size (bit 1) overrides set
        /// by instruction prefixes.
        /// </summary>
        internal byte SizeOverride
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            get => this.overrides.Size;
        }
        /// <summary>
        /// Contains operand size (bit 0) and address size (bit 1) for the
        /// processor's default state.
        /// </summary>
        internal byte GlobalSize;
        /// <summary>
        /// The number of instruction prefixes currently in effect.
        /// </summary>
        internal uint PrefixCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            get => this.overrides.Count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        internal void IncrementPrefixCount() => this.overrides.IncrementCount();
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        internal void SetSizeOverrideFlag(byte flag) => this.overrides.SetSizeFlag(flag);

        /// <summary>
        /// Array of pointers to segment registers.
        /// </summary>
        internal unsafe readonly ushort** segmentRegisterPointers;
        /// <summary>
        /// Array of pointers to segment override bases.
        /// </summary>
        internal unsafe readonly uint** baseOverrides;
        /// <summary>
        /// Array of pointers to default 16-bit segment bases.
        /// </summary>
        internal unsafe readonly uint** defaultSegments16;
        /// <summary>
        /// Array of pointers to default 32-bit SIB segment bases for MOD=1 or 2.
        /// </summary>
        internal unsafe readonly uint** defaultSibSegments32Mod12;
        /// <summary>
        /// Array of pointers to default 32-bit SIB segment bases for MOD=0.
        /// </summary>
        internal unsafe readonly uint** defaultSibSegments32Mod0;
        /// <summary>
        /// Array of segment base values.
        /// </summary>
        internal unsafe readonly uint* segmentBases;
        /// <summary>
        /// 16-byte cache of the current instruction.
        /// </summary>
        internal unsafe readonly byte* CachedInstruction;
        /// <summary>
        /// Pointer to the next byte in the cached instruction buffer.
        /// </summary>
        internal unsafe byte* CachedIP;
        /// <summary>
        /// Instruction pointer for the first byte of the current instruction.
        /// </summary>
        internal uint StartEIP;
        /// <summary>
        /// Specifies whether interrupts are disabled for the next instruction.
        /// </summary>
        internal bool TemporaryInterruptMask
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            get => this.overrides.InterruptMask;
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            set => this.overrides.InterruptMask = value;
        }
        #endregion

        /// <summary>
        /// Pointer to first general-purpose register.  Each register is 4 bytes apart.
        /// </summary>
        private unsafe readonly byte* gprBlock;
        private unsafe readonly void** wordRegisterPointers;
        private unsafe readonly byte** byteRegisterPointers;
        private unsafe readonly uint* debugRegisterBase;

        /// <summary>
        /// The number of registers contained in the GPR block.
        /// </summary>
        private const int RegisterCount = 24;
        /// <summary>
        /// The number of bytes used to cache the current instruction.
        /// </summary>
        private const int InstructionCacheSize = 16;

        private struct PrefixOverrides
        {
            public SegmentRegister Segment;
            public byte Size;
            public byte Count;
            private byte repeatAndInterruptMask;

            public RepeatPrefix Repeat
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
                readonly get => (RepeatPrefix)(this.repeatAndInterruptMask & 0b11);
                [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
                set => this.repeatAndInterruptMask = (byte)((this.repeatAndInterruptMask & 0b100u) | (uint)value);
            }
            public bool InterruptMask
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
                readonly get => (this.repeatAndInterruptMask & 0b100) == 0b100;
                [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
                set
                {
                    if (value)
                        this.repeatAndInterruptMask |= 0b100;
                    else
                        this.repeatAndInterruptMask &= 0b011;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public void IncrementCount() => this.Count++;
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public void SetSizeFlag(byte flag) => this.Size |= flag;
        }
    }
}
