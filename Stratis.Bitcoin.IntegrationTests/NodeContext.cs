﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using NBitcoin;
using Stratis.Bitcoin.Consensus;
using Stratis.Bitcoin.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Stratis.Bitcoin.IntegrationTests
{
	public class NodeContext : IDisposable
	{
		List<IDisposable> _CleanList = new List<IDisposable>();
		TestDirectory _TestDirectory;
		public NodeContext(string name, Network network, bool clean)
		{
			network = network ?? Network.RegTest;
			this._Network = network;
			this._TestDirectory = TestDirectory.Create(name, clean);
			this._PersistentCoinView = new DBreezeCoinView(network, this._TestDirectory.FolderName);
			this._PersistentCoinView.Initialize().GetAwaiter().GetResult();
            this._CleanList.Add(this._PersistentCoinView);
		}


		private readonly Network _Network;
		public Network Network
		{
			get
			{
				return this._Network;
			}
		}


		private ChainBuilder _ChainBuilder;
		public ChainBuilder ChainBuilder
		{
			get
			{
				return this._ChainBuilder = this._ChainBuilder ?? new ChainBuilder(this.Network);
			}
		}
		
		DBreezeCoinView _PersistentCoinView;
		public DBreezeCoinView PersistentCoinView
		{
			get
			{
				return this._PersistentCoinView;
			}
		}

		public string FolderName
		{
			get
			{
				return this._TestDirectory.FolderName;
			}
		}

		public static NodeContext Create([CallerMemberNameAttribute]string name = null, Network network = null, bool clean = true)
		{
			Logs.Configure(new LoggerFactory().AddConsole(LogLevel.Trace, false));
			return new NodeContext(name, network, clean);
		}

		public void Dispose()
		{
			foreach(var item in this._CleanList)
				item.Dispose();
            this._TestDirectory.Dispose(); //Not into cleanlist because it must run last
		}

		public void ReloadPersistentCoinView()
		{
			this._PersistentCoinView.Dispose();
			this._CleanList.Remove(this._PersistentCoinView);
			this._PersistentCoinView = new DBreezeCoinView(this._Network, this._TestDirectory.FolderName);
			this._PersistentCoinView.Initialize().GetAwaiter().GetResult();
            this._CleanList.Add(this._PersistentCoinView);
		}		
	}
}
