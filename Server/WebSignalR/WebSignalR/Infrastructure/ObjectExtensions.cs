using Newtonsoft.Json;
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

	}
}