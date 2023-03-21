using Il2CppDumper;

namespace Generator.OffsetLines
{
    abstract class Line : ILine, IHasOffset, IHasSection
    {
        public ulong Offset { get; set; }
        public string Text { get; set; }
        public ScriptSection Section { get; set; }
        public abstract int FindOffset(ScriptJson scriptJson);
        public virtual string GetLine(ScriptJson scriptJson)
        {
            if (Offset == 0)
            {
                FindOffset(scriptJson);
            }
            return $"#define {Text} \"0x{Offset:X}\"";
        }
    }
}