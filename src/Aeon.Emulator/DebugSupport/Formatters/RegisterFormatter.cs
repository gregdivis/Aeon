namespace Aeon.Emulator.DebugSupport
{
    internal static class RegisterFormatter
    {
        public static string Format(CodeRegister register) => register.ToString().ToLower();
    }
}
