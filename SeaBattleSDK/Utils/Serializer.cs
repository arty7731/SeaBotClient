using System;
using System.Text;

namespace SeaBattleSDK.Utils
{
    public class Serializer
    {
		public byte[] Serialize<T>(T data)
		{
			var json = SimpleJson.SerializeObject(data);
			return Encoding.UTF8.GetBytes(json);
		}

		public T Deserialize<T>(byte[] data)
		{
			var json = Encoding.UTF8.GetString(data);
			Console.WriteLine("\t\tJSON: " + json);
			return SimpleJson.DeserializeObject<T>(json);
		}
	}
}
