using System;
using System.Collections.Generic;
using System.IO;
using EpicChain.Collector.Models;

namespace EpicChain.Collector.Formats
{
    interface ICoverageFormat
    {
        void WriteReport(IReadOnlyList<ContractCoverage> coverage, Action<string, Action<Stream>> writeAttachement);
    }
}