using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using SeaBattleSDK.Net;
using SeaBattleSDK.Utils;
using SeaBattleSDK.Net.Model;
using SeaBattleSDK.Net.Messages;
using SeaBattleClientWPF.Model;
using System.Threading;

namespace SeaBattleClientWPF
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private Serializer serializer;
		private NetClient client;
		private BotMenager botMenager;
		private Player player;

		private bool IsRestart { get; set; }

		public MainWindow()
		{
			InitializeComponent();

			botMenager = new BotMenager();
			serializer = new Serializer();
			player = new Player();
		}

		private void Client_DataReceived(byte[] data)
		{
			if (data.Length == 0) return;
			var authR = serializer.Deserialize<AuthMessageResult>(data);
			if (authR != null && authR.auth != null)
			{
				player.Id = authR.auth.id.Value;
				return;
			}

			var bvbR = serializer.Deserialize<BvbMessageResult>(data);
			if (bvbR != null && bvbR.bvb != null)
			{
				if (bvbR.bvb.ships != null)
				{
					for (int i = 0; i < player.Field.Count; i++)
					{
						if (bvbR.bvb.ships[i] != 0)
						{
							player.Field[i].IsShip = true;
						}
					}
					UpdateField(gPlayerField, player.Field);
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
				Dispatcher.Invoke(() =>
				{
					if (endR.end.winner == player.Id)
					{
						player.CountWin++;
					}
					btnStartGame.Visibility = Visibility.Collapsed;
					btnConnect.Visibility = Visibility.Visible;
					client.Disconnect();
					player.CurrentGame++;
					if (player.CountGame > player.CurrentGame)
					{
						IsRestart = true;
						Connect();
					}
					else
					{
						IsRestart = false;
						var mes = string.Format("<h1>All: {0}, Win: {1}, Lose: {2}</h1>", player.CountGame, player.CountWin, player.CountGame - player.CountWin);
						//Message.SendMessage(tbMail.Text, "Battles result", mes);
					}
				});
			}
		}

		private void UpdateField(Grid field, List<Cell> cells)
		{
			Dispatcher.Invoke(() => 
			{
				foreach (var cell in field.Children)
				{
					if ((cell as Button) == null) continue;
					(cell as Button).Background = Brushes.LightGray;
					(cell as Button).Content = "";

				}
				foreach (var cell in field.Children)
				{
					if ((cell as Button) == null) continue;
					var cl = cells.Find(c => c.Id == (int)(cell as Button).Tag && c.IsShip);
					if (cl != null)
					{
						(cell as Button).Background = Brushes.DarkGreen;
					}
				}
				foreach (var cell in field.Children)
				{
					if ((cell as Button) == null) continue;
					var cl = cells.Find(c => c.Id == (int)(cell as Button).Tag);
					if (cl != null)
					{
						if (cl.IsWalk)
						{
							(cell as Button).Content = "O";
						}
						if (cl.IsHit)
						{
							(cell as Button).Background = Brushes.DarkGreen;
							(cell as Button).Content = "X";
						}
					}
				}
			});
		}
		private void CreateField(Grid field, List<Cell> cells)
		{
			field.Children.Clear();
			for (int i = 0; i < 11; i++)
			{
				field.ColumnDefinitions.Add(new ColumnDefinition());
				field.RowDefinitions.Add(new RowDefinition());
			}
			var id = 0;
			for (int y = 0; y < 10; y++)
			{
				for (int x = 0; x < 10; x++)
				{
					var cell = new Button();
					cell.Tag = cells[id].Id++;
					Grid.SetColumn(cell, y + 1);
					Grid.SetRow(cell, x + 1);
					field.Children.Add(cell);
				}
			}

			var ch = new Label();
			ch.Content = "#";
			ch.Background = Brushes.PaleGreen;
			Grid.SetColumn(ch, 0);
			Grid.SetRow(ch, 0);
			field.Children.Add(ch);

			for (int i = 0; i < 10; i++)
			{
				var number = new Label();
				number.Content = i + 1;
				number.Background = Brushes.PaleGreen;
				Grid.SetColumn(number, 0);
				Grid.SetRow(number, i + 1);
				field.Children.Add(number);

				var character = new Label();
				character.Content = char.ConvertFromUtf32(65 + i);
				character.Background = Brushes.PaleGreen;
				Grid.SetColumn(character, i + 1);
				Grid.SetRow(character, 0);
				field.Children.Add(character);
			}
		}
		private void ShowTurn(Turn turn)
		{
			if (turn.id != null)
			{
				Console.WriteLine("Id: " + turn.id);
				if (player.Id == turn.id)
				{
					//Thread.Sleep(500);
					var turnMes = serializer.Serialize(new TurnMessage() { turn = botMenager.LinnerWalk() });
					client.Send(turnMes);
				}
			}
			if (turn.result != null)
			{
				botMenager.WalkResult((TurnResultType)turn.result);
			}
			if (turn.opponent != null)
			{
				Dispatcher.Invoke(() => 
				{
				});
				player.WalkResult(turn);
			}
		}
		private void btnStartGame_Click(object sender, RoutedEventArgs e)
		{
			int count;
			if (int.TryParse(tbBattles.Text, out count)) player.CountGame = count;
			else player.CountGame = 1;
			StartGame();
		}

		private void btnConnect_Click(object sender, RoutedEventArgs e)
		{
			Connect();
		}
		private void StartGame(int place = 0)
		{
			var bvb = serializer.Serialize(new BvbMessage() { bvb = new Bvb() { place = 0 } });
			client.Send(bvb);
			player.Clear();
			botMenager.Clear();

			UpdateField(gEnemyField, botMenager.Field);
			UpdateField(gPlayerField, player.Field);
			btnStartGame.Visibility = Visibility.Collapsed;
		}
		private void Connect()
		{
			try
			{
				int port;
				if (string.IsNullOrEmpty(tbHost.Text) || !int.TryParse(tbPort.Text, out port)) return;

				client = new NetClient(tbHost.Text, port);
				gPlayerField.ColumnDefinitions.Clear();
				gEnemyField.ColumnDefinitions.Clear();
				gPlayerField.RowDefinitions.Clear();
				gEnemyField.RowDefinitions.Clear();
				CreateField(gPlayerField, player.Field);
				CreateField(gEnemyField, botMenager.Field);

				client.Connected += () =>
				{
					var auth = serializer.Serialize(new AuthMessage() { auth = "HelloWorld" });
					client.Send(auth);
				};
				client.DataSended += () => { };
				client.Disconnected += reason =>
				{
				};
				client.DataReceived += Client_DataReceived;
				client.Connect();
				btnConnect.Visibility = Visibility.Collapsed;
				btnStartGame.Visibility = Visibility.Visible;
				player.ChangePlayerField += () => UpdateField(gPlayerField, player.Field);
				botMenager.ChangeEnemyField += () => UpdateField(gEnemyField, botMenager.Field);
				if (IsRestart)
				{
					Thread.Sleep(1000);
					StartGame();
				}
			}
			catch (Exception err)
			{
				MessageBox.Show(err.Message);
			}
		}
	}
}
