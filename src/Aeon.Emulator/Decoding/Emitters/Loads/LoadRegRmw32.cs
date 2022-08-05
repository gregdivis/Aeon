using System;
using System.Reflection;
using System.Reflection.Emit;

#nullable disable

namespace Aeon.Emulator.Decoding.Emitters
{
    internal sealed class LoadRegRmw32 : LoadRegRmw
    {
        public static readonly Func<Processor, uint>[] LoadAddressMethods;
        public static readonly Func<Processor, uint>[] LoadOffsetMethods;

        private static readonly FieldInfo[] RegFields = new[]
        {
            Infos.Processor.PAX,
            Infos.Processor.PCX,
            Infos.Processor.PDX,
            Infos.Processor.PBX,
            null,
            Infos.Processor.PBP,
            Infos.Processor.PSI,
            Infos.Processor.PDI
        };

        public LoadRegRmw32(EmitStateInfo state, int valueSize, bool memoryOnly, bool floatingPoint, bool offsetOnly, bool linearAddressOnly)
            : base(state, valueSize, memoryOnly, floatingPoint, offsetOnly, linearAddressOnly)
        {
        }
        static LoadRegRmw32()
        {
            LoadAddressMethods = new Func<Processor, uint>[24];
            LoadOffsetMethods = new Func<Processor, uint>[24];

            for (int rm = 0; rm < 8; rm++)
            {
                LoadAddressMethods[rm] = LoadMod0(rm, false);
                LoadOffsetMethods[rm] = LoadMod0(rm, true);
            }

            for (int rm = 0; rm < 8; rm++)
            {
                LoadAddressMethods[(1 << 3) | rm] = LoadMod12(1, rm, false);
                LoadOffsetMethods[(1 << 3) | rm] = LoadMod12(1, rm, true);
            }

            for (int rm = 0; rm < 8; rm++)
            {
                LoadAddressMethods[(2 << 3) | rm] = LoadMod12(2, rm, false);
                LoadOffsetMethods[(2 << 3) | rm] = LoadMod12(2, rm, true);
            }
        }

        protected override void LoadPhysicalAddress(LocalBuilder rmLocal, LocalBuilder modLocal)
        {
            // Advance past RM byte.
            IncrementIPPointer(1);

            il.Emit(OpCodes.Ldsfld, typeof(LoadRegRmw32).GetField(nameof(LoadAddressMethods)));
            il.LoadLocal(rmLocal);
            il.LoadLocal(modLocal);
            il.LoadConstant(3);
            il.Emit(OpCodes.Shl);
            il.Emit(OpCodes.Or);
            il.Emit(OpCodes.Ldelem_Ref);

            LoadProcessor();

            il.Emit(OpCodes.Callvirt, typeof(Func<Processor, uint>).GetMethod("Invoke"));
        }
        protected override void LoadAddressOffset(LocalBuilder rmLocal, LocalBuilder modLocal)
        {
            // Advance past RM byte.
            IncrementIPPointer(1);

            il.Emit(OpCodes.Ldsfld, typeof(LoadRegRmw32).GetField(nameof(LoadOffsetMethods)));
            il.LoadLocal(rmLocal);
            il.LoadLocal(modLocal);
            il.LoadConstant(3);
            il.Emit(OpCodes.Shl);
            il.Emit(OpCodes.Or);
            il.Emit(OpCodes.Ldelem_Ref);

            LoadProcessor();

            il.Emit(OpCodes.Callvirt, typeof(Func<Processor, uint>).GetMethod("Invoke"));
        }

        private static Func<Processor, uint> LoadMod0(int rm, bool offsetOnly)
        {
            // arg 0 = processor

            var method = new DynamicMethod(string.Empty, typeof(uint), new[] { typeof(Processor) }, typeof(LoadRegRmw32));
            var il = method.GetILGenerator();

            if (rm != 4)
            {
                if (rm != 5)
                {
                    il.LoadArgument(0);
                    il.Emit(OpCodes.Ldfld, RegFields[rm]);
                    il.Emit(OpCodes.Ldind_U4);
                }
                else
                {
                    // Load Processor.CachedIP, then increment it.
                    il.LoadArgument(0);
                    il.Emit(OpCodes.Ldfld, Infos.Processor.CachedIP);
                    il.Emit(OpCodes.Ldind_U4);

                    IncrementIPPointer(il, 4);
                }

                if (!offsetOnly)
                {
                    LoadBaseAddress(il, () =>
                    {
                        il.LoadArgument(0);
                        il.Emit(OpCodes.Ldfld, Infos.Processor.SegmentBases);
                        il.LoadConstant((int)SegmentIndex.DS * 4);
                        il.Emit(OpCodes.Conv_I);
                        il.Emit(OpCodes.Add);
                    });

                    il.Emit(OpCodes.Add);
                }
            }
            else
            {
                // arg 0 = processor
                il.LoadArgument(0);

                // Load Processor.CachedIP, then increment it.
                il.LoadArgument(0);
                il.Emit(OpCodes.Ldfld, Infos.Processor.CachedIP);
                // arg 1 = SIB
                il.Emit(OpCodes.Ldind_U1);
                IncrementIPPointer(il, 1);

                il.Emit(OpCodes.Tailcall);
                il.Emit(OpCodes.Call, offsetOnly ? Infos.RuntimeCalls.NewLoadSibMod0Offset : Infos.RuntimeCalls.NewLoadSibMod0Address);
            }

            il.Emit(OpCodes.Ret);

            return (Func<Processor, uint>)method.CreateDelegate(typeof(Func<Processor, uint>));
        }
        private static Func<Processor, uint> LoadMod12(int mod, int rm, bool offsetOnly)
        {
            // arg 0 = processor

            var method = new DynamicMethod(string.Empty, typeof(uint), new[] { typeof(Processor) }, typeof(LoadRegRmw32));
            var il = method.GetILGenerator();

            if (rm != 4)
            {
                il.LoadArgument(0);
                il.Emit(OpCodes.Ldfld, RegFields[rm]);
                il.Emit(OpCodes.Ldind_U4);

                // Load Processor.CachedIP, then increment it.
                il.LoadArgument(0);
                il.Emit(OpCodes.Ldfld, Infos.Processor.CachedIP);
                il.Emit(mod == 1 ? OpCodes.Ldind_I1 : OpCodes.Ldind_U4);
                IncrementIPPointer(il, mod == 1 ? 1 : 4);

                il.Emit(OpCodes.Add);

                if (!offsetOnly)
                {
                    var segment = rm != 5 ? SegmentIndex.DS : SegmentIndex.SS;

                    LoadBaseAddress(il, () =>
                        {
                            il.LoadArgument(0);
                            il.Emit(OpCodes.Ldfld, Infos.Processor.SegmentBases);
                            il.LoadConstant((int)segment * 4);
                            il.Emit(OpCodes.Conv_I);
                            il.Emit(OpCodes.Add);
                        });

                    il.Emit(OpCodes.Add);
                }
            }
            else
            {
                // arg 0 = processor
                il.LoadArgument(0);

                // Load Processor.CachedIP, then increment it.
                il.LoadArgument(0);
                il.Emit(OpCodes.Ldfld, Infos.Processor.CachedIP);
                // arg 1 = SIB
                il.Emit(OpCodes.Ldind_U1);
                IncrementIPPointer(il, 1);

                // Load Processor.CachedIP, then increment it.
                il.LoadArgument(0);
                il.Emit(OpCodes.Ldfld, Infos.Processor.CachedIP);
                // arg 2 = displacement
                il.Emit(mod == 1 ? OpCodes.Ldind_I1 : OpCodes.Ldind_U4);
                IncrementIPPointer(il, mod == 1 ? 1 : 4);

                il.Emit(OpCodes.Tailcall);
                il.Emit(OpCodes.Call, offsetOnly ? Infos.RuntimeCalls.NewLoadSibMod12Offset : Infos.RuntimeCalls.NewLoadSibMod12Address);
            }

            il.Emit(OpCodes.Ret);

            return (Func<Processor, uint>)method.CreateDelegate(typeof(Func<Processor, uint>));
        }

        private static void LoadBaseAddress(ILGenerator il, Action loadDefaultBase)
        {
            il.LoadArgument(0);
            il.Emit(OpCodes.Ldfld, Infos.Processor.BaseOverrides);
            il.LoadArgument(0);
            il.Emit(OpCodes.Call, Infos.Processor.SegmentOverride.GetGetMethod());
            il.Emit(OpCodes.Conv_I);
            il.Emit(OpCodes.Sizeof, typeof(uint).MakePointerType());
            il.Emit(OpCodes.Mul);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Ldind_I);
            il.Emit(OpCodes.Dup);

            var gotBase = il.DefineLabel();
            il.Emit(OpCodes.Brtrue_S, gotBase);

            il.Emit(OpCodes.Pop);

            loadDefaultBase();

            il.MarkLabel(gotBase);
            il.Emit(OpCodes.Ldind_U4);
        }

        private static void IncrementIPPointer(ILGenerator il, int n)
        {
            il.LoadArgument(0);
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldfld, Infos.Processor.CachedIP);
            il.LoadConstant(n);
            il.Emit(OpCodes.Conv_I);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Stfld, Infos.Processor.CachedIP);
        }
    }
}
