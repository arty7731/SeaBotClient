using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaBattleClientWPF.Model
{
	public class Cell
	{
		public int Id { get; set; }
		public int Y { get; set; }
		public int X { get; set; }
		public bool IsShip { get; set; }
		public bool IsHit { get; set; }
		public bool IsWalk { get; set; }
	}
}
