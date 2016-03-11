class ServertTest1
{

	public static void Main()
	{
		System.Console.WriteLine("TCP Server");
		var commands = Zusi_Datenausgabe.CommandSet.LoadFromFile("commandset_server.xml");
		var server = new Zusi_Datenausgabe.ZusiTcpServer(commands, null);
		var listIds = new System.Collections.Generic.List<int>();
		foreach(var i in commands.CommandByID)
		{
			listIds.Add(i.Value.ID);
		}
		server.ReplaceAnywayRequested(listIds);
		System.Console.WriteLine("Set up Server successfull");
		server.Start(1435);
		while (!server.IsStarted) System.Threading.Thread.Sleep(100);
		System.Console.WriteLine("Starting successfull");
		server.OnError += delegate(object o, Zusi_Datenausgabe.ZusiTcpException ex) {
			//System.Console.WriteLine(ex.InnerException.Message);
			//System.Console.WriteLine(ex.StackTrace);
			System.Console.WriteLine(ex.ToString());
			//System.Console.WriteLine(ex.InnerException.StackTrace);
				};
		int numCon = 0;
		bool masterConnected = false;
		
		bool firstCancel = true;
		
		System.ConsoleCancelEventHandler shutDownServer = null;
		shutDownServer = delegate(object sender, System.ConsoleCancelEventArgs e)
		{
			if (server.IsStarted)
			{
				System.Console.Write("Shutting Down... ");
				server.Stop();
				if (firstCancel)
					e.Cancel = true;
				firstCancel = false;
				System.Console.WriteLine("OK!");
			}
		};
		
		System.Console.CancelKeyPress += shutDownServer;
		
		while (server.IsStarted)
		{
			System.Threading.Thread.Sleep(100);
			if ((numCon != server.Clients.Count)||((server.Master != null) != masterConnected))
				System.Console.WriteLine(string.Format("{0} Clients connected, Master {1}connected", 
					server.Clients.Count, (server.Master == null) ? "not " : ""));
			numCon = server.Clients.Count;
			masterConnected = (server.Master != null);
		}
		System.Console.WriteLine("Server shut down");
	}
}
