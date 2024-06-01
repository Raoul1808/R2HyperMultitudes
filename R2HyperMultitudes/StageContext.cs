using System.IO;
using R2HyperMultitudes.MathParser;

namespace R2HyperMultitudes
{
    public class ModStageContext : IContext
    {
        public double Stage { get; set; }

        public double ResolveVariable(string name)
        {
            if (name.ToLower() == "stage" || name.ToLower() == "x")
                return Stage;
            throw new InvalidDataException($"Unknown variable: {name}");
        }
    }
}
