using SeaBattleSDK.Net;
using SeaBattleSDK.Net.Messages;
using SeaBattleSDK.Net.Model;
using SeaBattleSDK.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestSeaBattleSDK
{
	class Program
	{
		private static Serializer serializer;
		private static NetClient client;
		private static int y;
		private static int x;

		private static int? Id { get; set; }


		static void Main(string[] args)
		{
			Console.WriteLine("Test Sea Battle SDK");

			try
			{
				serializer = new Serializer();
				client = new NetClient("88.99.171.92", 11000);

				client.Connected += () =>
				{
					Console.WriteLine("Connected done!");
					var auth = serializer.Serialize(new AuthMessage() { auth = "HelloWorld"});
					client.Send(auth);

				};

				client.DataSended += () =>  Console.WriteLine("Data sended");

				client.Disconnected += reason =>
				{
					Console.WriteLine("Disconnected: " + reason);
				};

				client.DataReceived += Client_DataReceived;

				client.Connect();

				Console.WriteLine("Enter any key!!!");
				Console.ReadKey();
				var bvb = serializer.Serialize(new BvbMessage() { bvb = new Bvb() { place = 0 }});
				client.Send(bvb);

				Console.ReadLine();

			}
			catch (Exception err)
			{
				Console.WriteLine(err.Message);
			}
		}

		private static void Client_DataReceived(byte[] data)
		{
			Console.WriteLine("Data Received: " + data.Length);
			if (data.Length == 0) return;
			var authR = serializer.Deserialize<AuthMessageResult>(data);
			if (authR != null && authR.auth != null)
			{
				Console.WriteLine(authR.auth.id);
				Id = authR.auth.id;
				return;
			}

			var bvbR = serializer.Deserialize<BvbMessageResult>(data);
			if (bvbR != null && bvbR.bvb != null)
			{
				if (bvbR.bvb.ships != null)
				{
					Console.WriteLine("Id:" + bvbR.bvb.id);
					Console.WriteLine("Name:" + bvbR.bvb.name);
					Console.Write("Ships: ");
					ShowShips(bvbR.bvb.ships);
					Console.WriteLine();
				}
				return;
			}
			var turnR = serializer.Deserialize<TurnMessageResult>(data);
			if (turnR != null && turnR.turn != null)
			{
				ShowTurn(turnR.turn);
			}
			var endR = serializer.Deserialize<EndMessageResult>(data);
			if (endR != null && endR.end != null)
			{
				Console.WriteLine("----Winner: " + endR.end.winner + " ----");
				if(endR.end.opponent != null) ShowShips(endR.end.opponent);
				client.DataReceived -= Client_DataReceived;
                Console.ReadKey();
			}
		}

		static void ShowTurn(Turn turn)
		{
			Console.WriteLine("----Turn----");
			if (turn.id != null)
			{
				Console.WriteLine("Id: " + turn.id);
				if (Id == turn.id)
				{
					//Console.WriteLine("Enter any key to start turn!");
					TurnBot();
				}
			}
			if (turn.result != null)
			{
				Console.WriteLine("Result: " + Enum.Parse(typeof(TurnResultType), turn.result.ToString()));
			}
			if (turn.shot != null)
			{
				Console.WriteLine("Shot: Y = " + turn.shot[0] + " X = " + turn.shot[1]);
			}
			if (turn.opponent != null)
			{
				Console.WriteLine("Opponent: " + Enum.Parse(typeof(TurnResultType), turn.opponent.result.ToString()));
				Console.WriteLine("Shot: Y = " + turn.opponent.shot[0] + " X = " + turn.opponent.shot[1]);

			}
			Console.WriteLine("----End turn----");
		}

		private static void TurnBot()
		{
			if (y == 10)
			{
				y = 0;
				x++;
			}
			if (x == 10) x = 0;

			var turn = serializer.Serialize(new TurnMessage() { turn = new Turn() { shot = new int?[] { y++, x } } });
			client.Send(turn);
		}

		static void ShowShips(int?[] ships)
		{
			for (int i = 0; i < 10; i++)
			{
				Console.WriteLine();
				for (int j = 0; j < 10; j++)
				{
					Console.Write(ships[i*10 + j]);
				}
			}
		}
	}
}
