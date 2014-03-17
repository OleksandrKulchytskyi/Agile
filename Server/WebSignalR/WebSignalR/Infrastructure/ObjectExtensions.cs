﻿using Newtonsoft.Json;
using System.IO;

namespace WebSignalR.Infrastructure
{
	public static class ObjectExtensions
	{
		public static string ToJson(this object obj)
		{
			if (obj == null)
				return "{}";
			JsonSerializer js = JsonSerializer.Create(new JsonSerializerSettings());
			using (var jw = new StringWriter())
			{
				js.Serialize(jw, obj);
				return jw.ToString();
			}
		}

		public static string ToJsonCamel(this object obj)
		{
			if (obj == null)
				return "{}";

			JsonSerializerSettings serializerSettings = new JsonSerializerSettings
			{
				ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
			};

			return JsonConvert.SerializeObject(obj, Formatting.None, serializerSettings);
		}
	}
}