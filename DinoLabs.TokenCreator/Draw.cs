using CommandLine;
using System.Collections;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DinoLabs.TokenCreator
{
    [Verb("draw", HelpText = "Draw combitations of features for given specification")]
    internal class DrawOptions
    {
        [Option('s', "spec")]
        public string Specification { get; set; }

        [Option('o', "output")]
        public string Output { get; set; }
    }

    public class Collection
    {
        public string Name { get; set; }

        public JsonNode Features { get; set; }

        public int Count { get; set; }
    }

    internal class Draw
    {
        public static int Run(DrawOptions options)
        {
            using var file = File.OpenRead(options.Specification);
            var collection = JsonSerializer.Deserialize<Collection>(file, new JsonSerializerOptions {  PropertyNameCaseInsensitive = true });
            var features = FeatureLoader.Load(collection.Features);
            Generate(features, collection.Name, collection.Count, options.Output);
            return 0;
        }

        private static void Generate(Feature[] features, string name, int count, string output)
        {
            var vectors = new List<Vector> { new Vector(Enumerable.Empty<(string, string)>(), count) };
            foreach (var feature in features)
            {
                vectors = AddFeature(vectors, feature);
            }

            var color = Console.BackgroundColor;
            Console.BackgroundColor = ConsoleColor.Green;
            Console.WriteLine($"Duplicates: {name}: {vectors.Where(x => x.Count > 1).Sum(x => x.Count)}");
            Console.BackgroundColor = color;
            foreach (var vector in vectors.Where(x => x.Count > 1))
            {
                color = Console.BackgroundColor;
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine(vector);
                Console.BackgroundColor = color;
            }
            Console.WriteLine("End of duplicates");
            CheckWeights(features, vectors);

            File.WriteAllText(output, JsonSerializer.Serialize(vectors, new JsonSerializerOptions {  WriteIndented = true }));
        }

        private static void CheckWeights(Feature[] regularFeatures, List<Vector> regular)
        {
            var expected = regularFeatures.ToDictionary(x => x.Name, x => x.Values.ToDictionary(y => y.Value, y => y.Weight));
            var weights = new Dictionary<string, Dictionary<string, int>>();
            foreach (var vector in regular)
            {
                foreach (var key in vector.Key)
                {
                    Dictionary<string, int> featureWeights;
                    if (!weights.TryGetValue(key.Feature, out featureWeights))
                    {
                        featureWeights = new Dictionary<string, int>();
                        weights.Add(key.Feature, featureWeights);
                    }

                    if (!featureWeights.ContainsKey(key.Value))
                    {
                        featureWeights[key.Value] = 1;
                    }
                    else
                    {
                        featureWeights[key.Value]++;
                    }
                }
            }

            foreach (var feature in weights)
            {
                var total = feature.Value.Select(x => x.Value).Sum();
                var expectedTotal = expected[feature.Key].Select(x => x.Value).Sum();
                foreach (var values in feature.Value)
                {
                    var expectedValue = expected[feature.Key][values.Key];
                    Console.WriteLine("{0}:{1}", feature.Key, values.Key);
                    Console.WriteLine("Actual: {0}/{1}: {2}", values.Value, total, values.Value * 100.0m / total);
                    Console.WriteLine("Expected: {0}/{1}: {2}", expectedValue, expectedTotal, expectedValue * 100.0m / expectedTotal);
                }
            }
        }

        private static List<Vector> AddFeature(List<Vector> vectors, Feature feature)
        {
            var newVector = new List<Vector>();
            var featureCycle = new FeatureCycle(feature);
            foreach (var vector in vectors)
            {
                var grouped = vector
                    .Select(key => new Vector(key.Concat(new[] { (feature.Name, featureCycle.Get().Value) }), 1))
                    .GroupBy(v => string.Join("_", v.Key))
                    .Select(x => new Vector(x.First().Key, x.Count()))
                    .ToArray();

                newVector.AddRange(grouped);
            }

            return newVector;
        }
    }

    internal class FeatureLoader
    {
        public static Feature[] Load(JsonNode node)
        {
            return node
                .AsObject()
                .Select(feature => 
                    new Feature(
                        feature.Key, 
                        feature.Value
                            .AsObject()
                            .Select(featureValue => new FeatureValue(featureValue.Key, (int)featureValue.Value.AsValue()))))
                .ToArray();
        }
    }

    public class FeatureCycle
    {
        private readonly Feature feature;
        private int[] featureValues;
        private int index;

        public FeatureCycle(Feature feature)
        {
            this.feature = feature;
            this.featureValues = feature.Values.Select(x => x.Weight).ToArray();
            this.index = 0;
        }

        public FeatureValue Get()
        {
            for (var i = 0; i < this.featureValues.Length; i++)
            {
                var ix = (this.index + i) % this.featureValues.Length;
                if (this.featureValues[ix] > 0)
                {
                    this.featureValues[ix]--;
                    this.index = ix + 1;
                    return this.feature.Values[ix];
                }
            }

            this.index = 0;
            this.featureValues = feature.Values.Select(x => x.Weight).ToArray();
            return Get();
        }
    }

    public class VectorConverter : JsonConverter<Vector>
    {
        public override Vector Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, Vector value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            foreach (var key in value.Key)
            {
                writer.WritePropertyName(key.Feature);
                JsonSerializer.Serialize(writer, key.Value, options);
            }
            writer.WriteEndObject();
        }
    }

    [JsonConverter(typeof(VectorConverter))]
    public class Vector : IEnumerable<List<(string Feature, string Value)>>
    {
        public Vector(IEnumerable<(string Feature, string Value)> key, int count)
        {
            this.Key = key.ToList();
            this.Count = count;
        }

        public List<(string Feature, string Value)> Key { get; set; }

        public int Count { get; set; }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public override string ToString()
        {
            return string.Join(";", Key.Select(x => x.Value)) + ":" + Count;
        }

        public IEnumerator<List<(string Feature, string Value)>> GetEnumerator()
        {
            for (int i = 0; i < this.Count; i++)
            {
                yield return this.Key;
            }
        }
    }

    public class Feature
    {
        public Feature(string name, IEnumerable<FeatureValue> values)
        {
            this.Name = name;
            this.Values = values.ToList();
        }

        public string Name { get; set; }

        public List<FeatureValue> Values { get; set; }
    }

    public class FeatureValue
    {
        public FeatureValue(string value, int weight)
        {
            this.Value = value;
            this.Weight = weight;
        }

        public string Value { get; set; }

        public int Weight { get; set; }
    }
}
