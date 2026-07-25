using Il2CppDumper;

namespace Generator.OffsetLines
{
    abstract class Line : ILine, IHasOffset, IHasSection
    {
        public ulong Offset { get; set; }
        public string Text { get; set; }
        public ScriptSection Section { get; set; }
        public abstract int FindOffset(ScriptJson scriptJson);
        public virtual string GetLine(ScriptJson scriptJson, bool relative = true, Il2CppAddressConverter addressConverter = null)
        {
            if (Offset == 0)
            {
                FindOffset(scriptJson);
            }

            if (addressConverter is not null && !relative)
            {
                Offset = addressConverter.RvaToOffset(Offset);
            }

            return string.Format(Text, Offset);
        }
    }
}