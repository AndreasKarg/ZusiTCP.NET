class ServertTest1
{

	public static void Main()
	{
		System.Console.WriteLine("TCP Master");
		var master = new Zusi_Datenausgabe.ZusiTcpTypeMaster("TestMaster", (System.Threading.SynchronizationContext) null);
		System.Console.WriteLine("Set up Master successfull");
		master.ErrorReceived += TCPConnection_ErrorReceived;
		master.Connect("localhost", 1435);
		while (master.ConnectionState != Zusi_Datenausgabe.ConnectionState.Connected) System.Threading.Thread.Sleep(100);
		System.Console.WriteLine("Connecting successfull");
		int i = 0;
		while(true) 
		{
			i ++;
			double cosArg = (double) i / 360.0 * 2 * System.Math.PI;
			System.Single v = (System.Single) (60 * (1 - System.Math.Cos(cosArg)));
			System.Single a = (System.Single) (60 / 36 * System.Math.Sin(cosArg));
			System.Console.WriteLine(string.Format("v := {0} km/h a := {1} m/s^2", v, a));
			master.SendSingle(v, 2561);
			master.SendSingle(a, 2657);
			System.Threading.Thread.Sleep(100);
			i = i % 360;
		}
	}
	private static void TCPConnection_ErrorReceived(object sender, Zusi_Datenausgabe.ZusiTcpException ex)
	{
		System.Console.WriteLine(ex.ToString());
	}
}
