using System;
using System.IO;
using YamlDotNet.Serialization;

namespace Global.Apiary.Documentation
{
    public static class YamlConverter
    {
        /// <summary>
        /// Helper method that will take a valid YAML 1.2 string and convert it to a JSON string. 
        /// http://yaml.org/
        /// </summary>
        /// <param name="raw"></param>
        /// <returns></returns>
        public static string YamlToJson(string raw)
        {
            var reader = new StringReader(raw);
            var deserializer = new DeserializerBuilder().Build();
            var yamlObject = deserializer.Deserialize(reader);
            var serializer = new SerializerBuilder()
                .JsonCompatible()
                .Build();
            return serializer.Serialize(yamlObject);
        }
    }
}
