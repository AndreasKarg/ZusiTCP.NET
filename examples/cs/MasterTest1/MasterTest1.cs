class ServertTest1
{

	public static void Main()
	{
		System.Console.WriteLine("TCP Master");
		var master = new Zusi_Datenausgabe.ZusiTcpTypeMaster("TestMaster", (System.Threading.SynchronizationContext) null);
		System.Console.WriteLine("Set up Master successfull");
		master.Connect("localhost", 1435);
		while (master.ConnectionState != Zusi_Datenausgabe.ConnectionState.Connected) System.Threading.Thread.Sleep(100);
		System.Console.WriteLine("Connecting successfull");
		int i = 0;
		while(true) 
		{
			i ++;
			System.Single v = (System.Single) (120 * System.Math.Sin((double) i / 360.0 * System.Math.PI));
			System.Console.WriteLine(string.Format("v := {0} km/h", v));
			master.SendSingle(v, 2561);
			System.Threading.Thread.Sleep(100);
			i = i % 360;
		}
	}
}
