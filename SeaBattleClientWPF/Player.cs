using SeaBattleClientWPF.Model;
using SeaBattleSDK.Net;
using SeaBattleSDK.Net.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SeaBattleClientWPF
{
	class Player
	{
		public event Action ChangePlayerField;
		public int CountGame { get; set; }
		public int CurrentGame { get; set; }
		public int CountWin { get; set; }
		public int Id { get; set; }
		public string Name { get; set; }
		public List<Cell> Field { get; set; }

		public Player()
		{
			Clear();
		}

		public void WalkResult(Turn turn)
		{
			var cell = Field.FirstOrDefault(c =>
			{
				return c.Y == turn.opponent.shot[(int)Coordinate.Y] && c.X == turn.opponent.shot[(int)Coordinate.X];
			});
			cell.IsWalk = true;
			switch ((TurnResultType)turn.opponent.result)
			{
				case TurnResultType.Miss:
					cell.IsHit = false;
					break;
				case TurnResultType.Hit:
					cell.IsHit = true;
					break;
				case TurnResultType.Killed:
					cell.IsHit = true;
					break;
			}
			if (ChangePlayerField != null) ChangePlayerField();
		}
		public void Clear()
		{
			Field = new List<Cell>();
			var id = 0;
			for (int y = 0; y < 10; y++)
			{
				for (int x = 0; x < 10; x++)
				{
					Field.Add(new Cell() { Id = id++, Y = y, X = x });
				}
			}
		}
	}
}
