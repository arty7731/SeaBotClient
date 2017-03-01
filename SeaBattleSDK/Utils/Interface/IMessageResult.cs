using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaBattleSDK.Utils.Interface
{
	public interface IMessageResult
	{
		IMessageResult IMessageResult { get; set; }
		void ProcessMessage(byte[] data);
	}
}
