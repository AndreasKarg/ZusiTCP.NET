class ServertTest1
{

	public static void Main()
	{
		System.Console.WriteLine("Railworks <-> Zusi Master");
		var master = new Railworks_GetData.RwZusiConverter();
		master.ErrorReceived += TCPConnection_ErrorReceived;
		 //if no Railworks is installed run the application in it's sandbox.
		if (master.RailworksFile == null) //comment this line to run it always it's sandbox.
				master.RailworksFile = "dat\\plugins\\GetData.txt";
		System.Console.WriteLine("Path: " + master.RailworksFile);
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

		master.SingleItems.Add("Zugkraft gesamt", 2565); //traction gauge
		master.SingleItems.Add("Druck Hauptluftleitung", 2562); //main pipe pressure
		master.SingleItems.Add("Druck Hauptluftbehalter", 2564); //reservoir pressure
		master.SingleItems.Add("Druck Bremszylinder", 2563); //brake cylinder pressure
		master.SingleItems.Add("Strom", 2567); //Current
		master.SingleItems.Add("Spannung", 2568); //Voltage
		master.BoolItems.Add("LM Turen", 2607); //Doors locked
		master.BoolItems.Add("LM Sifa", 2596); //Sifa light

		//masterDateTimeFormat = masterDateTimeFormat;
		master.DateTimeItems.Add("Clock", 2610); //clock

		master.Start();
	}
	private static void TCPConnection_ErrorReceived(object sender, Zusi_Datenausgabe.ZusiTcpException ex)
	{
		System.Console.WriteLine(ex.ToString());
	}
}
