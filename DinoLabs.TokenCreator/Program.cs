using CommandLine;
using DinoLabs.TokenCreator;

Parser.Default.ParseArguments<DrawOptions, GenerateOptions>(args)
    .MapResult(
        (DrawOptions options) => Draw.Run(options),
        (GenerateOptions options) => Generate.Run(options),
        errs => 1);