class ServertTest1
{

	public static void Main()
	{
		System.Console.WriteLine("TCP Client");
		var client = new Zusi_Datenausgabe.ZusiTcpTypeClient("Beispielclient", Zusi_Datenausgabe.ClientPriority.High, 
										(System.Threading.SynchronizationContext) null);
		System.Console.WriteLine("Set up Client successfull");
		client.RequestData(2561);
		client.Connect("localhost", 1435);
		while (client.ConnectionState != Zusi_Datenausgabe.ConnectionState.Connected) System.Threading.Thread.Sleep(100);
		System.Console.WriteLine("Connecting successfull");
		client.FloatReceived += On_FloatReceived;
		while(true) System.Threading.Thread.Sleep(100);
	}
	private static void On_FloatReceived(object sender, Zusi_Datenausgabe.DataSet<float> data)
	{
		System.Console.WriteLine(string.Format("v = {0} km/h", data.Value));
	}
}
