using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Aeon.Emulator.Instructions;

#nullable disable

namespace Aeon.Emulator.Decoding
{
    internal sealed class InstructionSetBuilder
    {
        private readonly List<InstructionInfo> opcodes = new List<InstructionInfo>();
        private readonly EmulatorBuilder emulatorBuilder = new EmulatorBuilder();

        /// <summary>
        /// Gets all of the normal one-byte opcodes.
        /// </summary>
        public IEnumerable<InstructionInfo> OneByteOpcodes
        {
            get
            {
                foreach (var info in opcodes)
                {
                    if (!info.IsMultiByte && info.ModRmByte != ModRmInfo.OnlyRm)
                        yield return info;
                }
            }
        }
        /// <summary>
        /// Gets all of the opcodes where part of the opcode is in the ModR/M byte.
        /// </summary>
        public IEnumerable<InstructionInfo> ExtendedOpcodes
        {
            get
            {
                foreach (var info in opcodes)
                {
                    if (info.ModRmByte == ModRmInfo.OnlyRm && !info.IsMultiByte)
                        yield return info;
                }
            }
        }
        /// <summary>
        /// Gets all of the normal two-byte opcodes.
        /// </summary>
        public IEnumerable<InstructionInfo> TwoByteOpcodes
        {
            get
            {
                foreach (var info in opcodes)
                {
                    if (info.IsMultiByte)
                        yield return info;
                }
            }
        }

        public void BuildSet()
        {
            foreach (MethodInfo methodInfo in FindMethods())
            {
                foreach (InstructionInfo inst in GetMethodInstructions(methodInfo))
                    AddInstruction(inst, methodInfo);
            }
        }

        private void AddInstruction(InstructionInfo inst, MethodInfo emulateMethod)
        {
            foreach (var subInst in inst.Expand())
            {
                var newInst = subInst;
                newInst.NewEmulators = emulatorBuilder.GetDelegates(subInst);
                opcodes.Add(newInst);
            }
        }
        private IEnumerable<MethodInfo> FindMethods()
        {
            var allTypes = Assembly.GetCallingAssembly().GetTypes();
            foreach (Type t in allTypes)
            {
                var methods = t.GetMethods(BindingFlags.Static | BindingFlags.Public);
                foreach (var methodInfo in methods)
                {
                    var res = methodInfo.GetCustomAttributes(typeof(OpcodeAttribute), false);
                    if (res != null && res.Length > 0)
                        yield return methodInfo;
                }
            }
        }

        private static IEnumerable<InstructionInfo> GetMethodInstructions(MethodInfo emulateMethod)
        {
            var attrs = emulateMethod.GetCustomAttributes(typeof(OpcodeAttribute), false).Cast<OpcodeAttribute>();
            foreach (OpcodeAttribute attr in attrs)
            {
                var codes = attr.OpcodeFormat.Split('|');
                foreach (string code in codes)
                {
                    var info = InstructionInfo.Parse(code.Trim());
                    info.EmulateMethods = GetEmulatorMethods(emulateMethod, attr);
                    if (attr.Name != null)
                    {
                        info.Name = attr.Name;
                    }
                    else
                    {
                        string className = emulateMethod.DeclaringType.Name;
                        info.Name = className.ToLower();
                    }

                    info.IsPrefix = attr.IsPrefix;

                    yield return info;
                }
            }
        }
        private static MethodInfo[] GetEmulatorMethods(MethodInfo defaultMethod, OpcodeAttribute opcodeAttribute)
        {
            var result = new MethodInfo[4];

            // Make sure the array is populated with sizes supported by the default method.
            if ((opcodeAttribute.OperandSize & 16) == 16 && (opcodeAttribute.AddressSize & 16) == 16)
                result[0] = defaultMethod;
            if ((opcodeAttribute.OperandSize & 32) == 32 && (opcodeAttribute.AddressSize & 16) == 16)
                result[1] = defaultMethod;
            if ((opcodeAttribute.OperandSize & 16) == 16 && (opcodeAttribute.AddressSize & 32) == 32)
                result[2] = defaultMethod;
            if ((opcodeAttribute.OperandSize & 32) == 32 && (opcodeAttribute.AddressSize & 32) == 32)
                result[3] = defaultMethod;

            // Look for altnerate methods defined in the same class.
            var otherMethods = defaultMethod.DeclaringType.GetMethods(BindingFlags.Static | BindingFlags.Public);
            foreach (var method in otherMethods)
            {
                var attrObjects = method.GetCustomAttributes(typeof(AlternateAttribute), false);
                if (attrObjects != null && attrObjects.Length == 1)
                {
                    var alt = (AlternateAttribute)attrObjects[0];
                    if (alt.MethodName == defaultMethod.Name)
                    {
                        if ((alt.OperandSize & 16) == 16 && (alt.AddressSize & 16) == 16)
                            result[0] = method;
                        if ((alt.OperandSize & 32) == 32 && (alt.AddressSize & 16) == 16)
                            result[1] = method;
                        if ((alt.OperandSize & 16) == 16 && (alt.AddressSize & 32) == 32)
                            result[2] = method;
                        if ((alt.OperandSize & 32) == 32 && (alt.AddressSize & 32) == 32)
                            result[3] = method;
                    }
                }
            }

            return result;
        }
    }
}
