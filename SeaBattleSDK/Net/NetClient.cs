using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SeaBattleSDK.Net
{
	public class NetClient
	{
		public event Action Connected;
		public event Action<string> Disconnected;
		public event Action DataSended;
		public event Action<byte[]> DataReceived;

		public bool IsConnected { get { return client.Connected; } }

		private bool IsStarted { get; set; }

		private TcpClient client;
		private IPEndPoint address;

		public NetClient(string ip, int port)
		{
			IsStarted = true;
			address = new IPEndPoint(IPAddress.Parse(ip), port);
			client = new TcpClient();
		}

		public void Connect()
		{
			client.BeginConnect(address.Address, address.Port, ConnectCallback, null);
		}

		private void ConnectCallback(IAsyncResult res)
		{
			client.EndConnect(res);
			Connected();
			Receive();
		}

		public void Send(byte[] data)
		{
			var len = BitConverter.GetBytes(data.Length);
			if (BitConverter.IsLittleEndian) Array.Reverse(len);
			var mes = new byte[data.Length + len.Length];
			Array.Copy(len, mes, len.Length);
			Array.Copy(data, 0, mes, len.Length, data.Length);
			client.GetStream().BeginWrite(mes, 0, mes.Length, SendCallback, null);

		}

		private void SendCallback(IAsyncResult res)
		{
			client.GetStream().EndWrite(res);
			DataSended();
		}

		private void Receive()
		{
			try
			{
				var size = new byte[4];
				client.GetStream().BeginRead(size, 0, 4, ReceiveSizeCallback, size);
			}
			catch (Exception err)
			{
			}

		}

		private void ReceiveSizeCallback(IAsyncResult res)
		{
			try
			{
				client.GetStream().EndRead(res);
				var sizeData = (byte[])res.AsyncState;
				if (BitConverter.IsLittleEndian) Array.Reverse(sizeData);

				var size = BitConverter.ToInt32(sizeData, 0);
				var data = new byte[size];
				client.GetStream().BeginRead(data, 0, size, ReceiveDataCallback, data);
			}
			catch (Exception err)
			{

				throw;
			}

		}

		private void ReceiveDataCallback(IAsyncResult res)
		{
			try
			{
				client.GetStream().EndRead(res);
				var data = (byte[])res.AsyncState;
				if(DataReceived != null) DataReceived(data);
				if(IsStarted) Receive();
			}
			catch (Exception err)
			{

			}

		}

		public void Disconnect()
		{
			IsStarted = false;
			client.Close();
			Disconnected("Disconnect by client");
		}

	}
}
