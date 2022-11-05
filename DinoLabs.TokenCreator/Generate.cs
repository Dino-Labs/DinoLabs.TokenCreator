using CommandLine;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DinoLabs.TokenCreator
{
    public enum MissingImageMode
    {
        Break,
        Warn,
        Ignore,
    }

    [Verb("generate", HelpText = "Generate token images based on provided specification")]
    internal class GenerateOptions
    {
        [Option('l', "layers")]
        public string Layers { get; set; }

        [Option('p', "path")]
        public string Path { get; set; }

        [Option('t', "tokens")]
        public string Tokens { get; set; }

        [Option('m', "mode", Default = MissingImageMode.Break)]
        public MissingImageMode Mode { get; set; }
    }

    internal class Generate
    {
        public static async Task<int> Run(GenerateOptions options)
        {
            Directory.CreateDirectory(Path.Combine(options.Path, "output"));
            var layers = LoadLayers(options.Layers);
            Parallel.ForEach(LoadTokens(options.Tokens), (token, state, index) =>
            {
                CreateToken(index, token, layers, options.Path, options.Mode);
            });
            return 0;
        }

        private static void CreateToken(long index, IDictionary<string, string> token, string[] layers, string path, MissingImageMode mode)
        {
            var layerPath = GetLayerPath(layers[0], token, path);
            using var @base = Image.Load(layerPath);
            foreach (var layer in layers.Skip(1))
            {
                layerPath = GetLayerPath(layer, token, path);

                try
                {
                    using var layerImg = Image.Load(layerPath);
                    @base.Mutate(operation => operation.DrawImage(layerImg, 1));
                }
                catch (IOException)
                {
                    switch (mode)
                    {
                        case MissingImageMode.Break:
                            Console.Error.WriteLine("Missing layer " + layerPath);
                            throw;
                            break;
                        case MissingImageMode.Warn:
                            Console.WriteLine("Missing layer " + layerPath);
                            break;
                        case MissingImageMode.Ignore:
                            break;
                        default:
                            break;
                    }
                }
            }

            @base.SaveAsPng(Path.Combine(path, "output", $"{index:D5}.png"));
        }

        private static string GetLayerPath(string layer, IDictionary<string, string> token, string path)
        {
            var regex = new Regex(@"({[^{]*?)\w(?=\})}");
            var replaced = regex.Replace(layer, (match) =>
            {
                string property = match.ToString();
                return token[property.Substring(1, property.Length -2)];
            });

            return Path.Combine(path, replaced);
        }

        private static string[] LoadLayers(string layers)
        {
            using var file = File.OpenRead(layers);
            return JsonSerializer.Deserialize<string[]>(file);
        }

        private static List<Dictionary<string, string>> LoadTokens(string tokens)
        {
            var file = File.OpenRead(tokens);
            return JsonSerializer.Deserialize<List<Dictionary<string, string>>>(file);
        }
    }
}
