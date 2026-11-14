using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Nitrox.Model.DataStructures;
using Nitrox.Server.Subnautica.Models.Serialization.Json;

namespace Nitrox.Test.Server;

[TestClass]
public class JsonSerializationTest
{
    private const string NEWTONSOFTJSON_TEST = """
                                               {
                                                   "Numbers": [
                                                       {
                                                           "value": "50218e71-53d6-4b2f-85cf-ea95483d139e"
                                                       },
                                                       {
                                                           "value": "1047af72-82b1-492d-92c4-dc10ce807324"
                                                       },
                                                       {
                                                           "value": "f961498e-cfb4-4c7a-9ff2-2ec9f15a8fe5"
                                                       },
                                                       {
                                                           "value": "14038bcf-dffc-464a-8ead-7522cfb9e696"
                                                       },
                                                       {
                                                           "value": "4fc28873-34ae-4e28-b02d-1388c7b6a230"
                                                       }
                                                   ],
                                                   "Lookup": [
                                                       {
                                                           "Key": "Use_Fabricator_Loot",
                                                           "Value": 485.8958
                                                       },
                                                       {
                                                           "Key": "Player_Underwater",
                                                           "Value": 3233.1074
                                                       },
                                                       {
                                                           "Key": "Equip_Flare",
                                                           "Value": 3244.8416
                                                       },
                                                       {
                                                           "Key": "Enter_Seamoth",
                                                           "Value": 5043.631
                                                       },
                                                       {
                                                           "Key": "Pickup_Flare",
                                                           "Value": 20508.432
                                                       }
                                                   ]
                                               }
                                               """;

    private static readonly JsonSerializerOptions options = new() { WriteIndented = true };

    [ClassInitialize]
    public static void Setup(TestContext context)
    {
        options.Converters.Insert(0, new DictionaryLikeNewtonsoftJsonConverter());
        options.Converters.Insert(0, new ListLikeNewtonsoftJsonConverter());
        options.Converters.Insert(0, new NitroxIdConverter());
    }

    [TestMethod]
    public async Task ShouldSerializeAndDeserializeToEquivalent()
    {
        TestTarget a = new();
        a.Numbers.AddRange([new NitroxId(), new NitroxId(), new NitroxId(), new NitroxId(), new NitroxId()]);
        a.Lookup.Add("name", 4);
        a.Lookup.Add("test", 6);
        a.Lookup.Add(Guid.CreateVersion7().ToString("D"), Random.Shared.NextSingle() * 200 + 400);

        using MemoryStream stream = new();
        await JsonSerializer.SerializeAsync(stream, a, options);
        string firstSerialize = Encoding.UTF8.GetString(stream.ToArray());
        stream.Position = 0;
        TestTarget b = await JsonSerializer.DeserializeAsync<TestTarget>(stream, options);
        stream.Position = 0;
        await JsonSerializer.SerializeAsync(stream, b, options);
        string secondSerialize = Encoding.UTF8.GetString(stream.ToArray());

        b.Numbers.Should().BeEquivalentTo(a.Numbers);
        b.Lookup.Should().BeEquivalentTo(a.Lookup);
        firstSerialize.Should().BeEquivalentTo(secondSerialize);
    }

    /// <summary>
    ///     This test can be removed if we remove NewtonsoftJson as a dependency for the server.
    /// </summary>
    [TestMethod]
    public async Task ShouldBeCompatibleWithNewtonsoftJsonFormat()
    {
        using MemoryStream stream = new();
        string firstSerialize = NEWTONSOFTJSON_TEST;
        await stream.WriteAsync(Encoding.UTF8.GetBytes(firstSerialize));
        stream.Position = 0;
        TestTarget b = await JsonSerializer.DeserializeAsync<TestTarget>(stream, options);
        using MemoryStream streamB = new();
        await JsonSerializer.SerializeAsync(streamB, b, options);
        string secondSerialize = Encoding.UTF8.GetString(streamB.ToArray());

        firstSerialize.Should().Contain(@"""Value""");
        firstSerialize.Should().Contain(@"""Key""");
        secondSerialize.Should().NotContain(@"""Value""");
        secondSerialize.Should().NotContain(@"""Key""");
    }

    private record TestTarget
    {
        public ThreadSafeList<NitroxId> Numbers { get; set; } = [];
        public ThreadSafeDictionary<string, float> Lookup { get; set; } = [];
    }
}
