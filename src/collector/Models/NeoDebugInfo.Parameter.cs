namespace EpicChain.Collector.Models
{
    public partial class EpicChainDebugInfo
    {
        public struct Parameter
        {
            public readonly string Name;
            public readonly string Type;
            public readonly int Index;

            public Parameter(string name, string type, int index)
            {
                Name = name;
                Type = type;
                Index = index;
            }
        }
    }
}
