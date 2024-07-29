using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.IO;

namespace AutoAddScriptsToJson
{
	internal static class JsonUtils
	{
		public static T JsonReadPropertyValue<T>(in string propertyName, string json)
		{
			using (StringReader stringReader = new StringReader(json))
			using (JsonTextReader jsonReader = new JsonTextReader(stringReader))
			{
				while (jsonReader.Read())
				{
					if (jsonReader.TokenType == JsonToken.PropertyName && (string)jsonReader.Value == propertyName)
					{
						jsonReader.Read();

						JsonSerializer serializer = new JsonSerializer();
						try
						{
							return serializer.Deserialize<T>(jsonReader);
						}
						catch (Exception)
						{
							break;
						}
					}
				}
				return default(T);
			}
		}
		public static string JsonWritePropertyValue(in string propertyName, in object value, string json)
		{
			JObject obj = JObject.Parse(json);
			obj[propertyName] = JToken.FromObject(value);
			return obj.ToString();
		}
	}
}
