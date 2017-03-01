using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaBattleSDK.Net.Model
{
	public class Turn
	{
		public int? id { get; set; }
		public int?[] shot { get; set; }
		public int? result { get; set; }
		public Opponent opponent { get; set; }

	}

	public class Opponent
	{
		public int?[] shot { get; set; }
		public int? result { get; set; }
	}
}
