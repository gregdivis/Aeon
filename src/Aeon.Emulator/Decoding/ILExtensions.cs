using System;
using System.Reflection.Emit;

namespace Aeon.Emulator.Decoding
{
    internal static class ILExtensions
    {
        public static void StoreLocal(this ILGenerator il, LocalBuilder local)
        {
            switch (local.LocalIndex)
            {
                case 0:
                    il.Emit(OpCodes.Stloc_0);
                    break;

                case 1:
                    il.Emit(OpCodes.Stloc_1);
                    break;

                case 2:
                    il.Emit(OpCodes.Stloc_2);
                    break;

                case 3:
                    il.Emit(OpCodes.Stloc_3);
                    break;

                default:
                    if (local.LocalIndex <= byte.MaxValue)
                        il.Emit(OpCodes.Stloc_S, (byte)local.LocalIndex);
                    else
                        il.Emit(OpCodes.Stloc, local.LocalIndex);
                    break;
            }
        }
        public static void LoadLocal(this ILGenerator il, LocalBuilder local)
        {
            switch (local.LocalIndex)
            {
                case 0:
                    il.Emit(OpCodes.Ldloc_0);
                    break;

                case 1:
                    il.Emit(OpCodes.Ldloc_1);
                    break;

                case 2:
                    il.Emit(OpCodes.Ldloc_2);
                    break;

                case 3:
                    il.Emit(OpCodes.Ldloc_3);
                    break;

                default:
                    if (local.LocalIndex <= byte.MaxValue)
                        il.Emit(OpCodes.Ldloc_S, (byte)local.LocalIndex);
                    else
                        il.Emit(OpCodes.Ldloc, local.LocalIndex);
                    break;
            }
        }
        public static void LoadConstant(this ILGenerator il, int value)
        {
            switch (value)
            {
                case 0:
                    il.Emit(OpCodes.Ldc_I4_0);
                    break;

                case 1:
                    il.Emit(OpCodes.Ldc_I4_1);
                    break;

                case 2:
                    il.Emit(OpCodes.Ldc_I4_2);
                    break;

                case 3:
                    il.Emit(OpCodes.Ldc_I4_3);
                    break;

                case 4:
                    il.Emit(OpCodes.Ldc_I4_4);
                    break;

                case 5:
                    il.Emit(OpCodes.Ldc_I4_5);
                    break;

                case 6:
                    il.Emit(OpCodes.Ldc_I4_6);
                    break;

                case 7:
                    il.Emit(OpCodes.Ldc_I4_7);
                    break;

                case 8:
                    il.Emit(OpCodes.Ldc_I4_8);
                    break;

                case -1:
                    il.Emit(OpCodes.Ldc_I4_M1);
                    break;

                default:
                    if (value >= sbyte.MinValue && value <= sbyte.MaxValue)
                        il.Emit(OpCodes.Ldc_I4_S, (sbyte)value);
                    else
                        il.Emit(OpCodes.Ldc_I4, value);
                    break;
            }
        }
        public static void LoadArgument(this ILGenerator il, int index)
        {
            switch (index)
            {
                case 0:
                    il.Emit(OpCodes.Ldarg_0);
                    break;

                case 1:
                    il.Emit(OpCodes.Ldarg_1);
                    break;

                case 2:
                    il.Emit(OpCodes.Ldarg_2);
                    break;

                case 3:
                    il.Emit(OpCodes.Ldarg_3);
                    break;

                default:
                    if (index <= byte.MaxValue)
                        il.Emit(OpCodes.Ldarg_S, (byte)index);
                    else
                        il.Emit(OpCodes.Ldarg, index);
                    break;
            }
        }
        public static void LoadPointer(this ILGenerator il, IntPtr value)
        {
            if (IntPtr.Size == 4)
                LoadConstant(il, value.ToInt32());
            else if (IntPtr.Size == 8)
                il.Emit(OpCodes.Ldc_I8, value.ToInt64());
            else
                throw new InvalidOperationException();

            il.Emit(OpCodes.Conv_I);
        }
        public static void LoadThis(this ILGenerator il)
        {
            il.Emit(OpCodes.Ldarg_0);
        }
    }
}
