using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinoLabs.TokenCreator
{
    [Verb("generate", HelpText = "Generate token images based on provided specification")]
    internal class GenerateOptions
    {

    }

    internal class Generate
    {
        public static int Run(GenerateOptions options)
        {
            return 0;
        }
    }
}
