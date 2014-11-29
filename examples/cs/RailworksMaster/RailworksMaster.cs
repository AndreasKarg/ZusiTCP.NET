class ServertTest1
{

	public static void Main()
	{
		System.Console.WriteLine("Railworks <-> Zusi Master");
		var master = new Railworks_GetData.RwZusiConverter();
		System.Console.WriteLine(master.RailworksPath);
		master.ErrorReceived += TCPConnection_ErrorReceived;
		master.RailworksPath = "dat"; //uncomment this line to run the application in it's sandbox.
		master.SingleItems.Add("Current Speed", 2561); //seems to be in km/h
		master.SingleItems.Add("Speed", 2561);
		master.SingleTransformation.Add("Speed", 3.6f); //seems to be in m/s, transform to km/h
		master.SingleItems.Add("Current Speed Limit (CSL)", 2660); //seems to be in km/h
		master.SingleItems.Add("Current Speed Limit", 2660); //seems to be in km/h
		master.SingleItems.Add("SpeedLimit", 2660);
		master.SingleTransformation.Add("SpeedLimit", 3.6f); //seems to be in m/s, transform to km/h
		master.SingleItems.Add("Next Speed Limit (NSL)", 2661); //seems to be in km/h
		master.SingleItems.Add("Next Speed Limit", 2661); //seems to be in km/h
		master.SingleItems.Add("NextSpeedLimit", 2661);
		master.SingleTransformation.Add("NextSpeedLimit", 3.6f); //seems to be in m/s, transform to km/h
		master.SingleItems.Add("Next Speed Limit Distance (NSLD)", 2662);
		master.SingleItems.Add("Next Speed Limit Distance", 2662);
		master.SingleItems.Add("NextSpeedLimitDistance", 2662);
		master.SingleItems.Add("Acceleration", 2657);
		master.Start();
	}
	private static void TCPConnection_ErrorReceived(object sender, Zusi_Datenausgabe.ZusiTcpException ex)
	{
		System.Console.WriteLine(ex.ToString());
	}
}
