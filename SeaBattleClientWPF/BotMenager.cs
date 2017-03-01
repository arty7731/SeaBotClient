using SeaBattleSDK.Net.Model;
using SeaBattleSDK.Net;
using SeaBattleClientWPF.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaBattleClientWPF
{
	public class BotMenager
	{
		public event Action ChangeEnemyField;
		public List<Cell> Field { get; private set; }
		public int? CellId { get; private set; }

		public BotMenager()
		{
			Clear();
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

		public Turn LinnerWalk()
		{
			var cell = Field.FirstOrDefault(c => !c.IsWalk);
			CellId = cell.Id;
			var turn = new Turn();
			turn.shot = new int?[2];
			turn.shot[(int)Coordinate.Y] = cell.Y;
			turn.shot[(int)Coordinate.X] = cell.X;

			return turn;
		}
		public void WalkResult(TurnResultType result)
		{
			Field.FirstOrDefault(c => c.Id == CellId).IsWalk = true;
			switch (result)
			{
				case TurnResultType.Miss:
					Field.FirstOrDefault(c => c.Id == CellId).IsHit = false;
					break;
				case TurnResultType.Hit:
					Field.FirstOrDefault(c => c.Id == CellId).IsHit = true;
					break;
				case TurnResultType.Killed:
					Field.FirstOrDefault(c => c.Id == CellId).IsHit = true;
					break;
			}
			if (ChangeEnemyField != null) ChangeEnemyField();
		}
	}
}
