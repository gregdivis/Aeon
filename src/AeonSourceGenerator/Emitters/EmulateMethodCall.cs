namespace Aeon.SourceGenerator.Emitters
{
    internal readonly ref struct EmulateMethodCall
    {
        public EmulateMethodCall(string name, string arg1, IReadOnlyList<Emitter> argEmitters)
        {
            this.Name = name;
            this.Arg1 = arg1;
            this.ArgEmitters = argEmitters;
        }

        public string Name { get; }
        public string Arg1 { get; }
        public IReadOnlyList<Emitter> ArgEmitters { get; }
    }
}
