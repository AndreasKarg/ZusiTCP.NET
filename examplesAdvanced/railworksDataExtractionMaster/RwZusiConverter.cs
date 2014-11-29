using System.Collections.Generic;
using System.Threading;
using System.IO;
using Zusi_Datenausgabe;

namespace Railworks_GetData
{
    public class RwZusiConverter
    {
        private bool readRailsimData(out Dictionary<string, string> stringVals,
                                     out Dictionary<string, int> intVals,
                                     out Dictionary<string, float> floatVals,
                                     out Dictionary<string, bool> boolVals)
        {
            Dictionary<string, string> stringValsX = new Dictionary<string, string>();
            Dictionary<string, int> intValsX = new Dictionary<string, int>();
            Dictionary<string, float> floatValsX = new Dictionary<string, float>();
            Dictionary<string, bool> boolValsX = new Dictionary<string, bool>();
            stringVals = stringValsX;
            intVals = intValsX;
            floatVals = floatValsX;
            boolVals = boolValsX;

            if (File.Exists(RailworksPath + "\\plugins\\GetData.txt"))
            {
                //The file does exist so open it for reading but with read & write access so Railworks can still write to it while we have it open.
                var fs = new FileStream(RailworksPath + "\\plugins\\GetData.txt", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                var sr = new StreamReader(fs);

                System.Globalization.NumberStyles floatStyle = System.Globalization.NumberStyles.Float |
                                    System.Globalization.NumberStyles.AllowThousands;
                System.IFormatProvider floatCulture = System.Globalization.CultureInfo.InvariantCulture;

                //Read each line in turn until end of file is reached
                while (!sr.EndOfStream)
                {
                    string tmp = sr.ReadLine();//Store the read line into tmp variable

                    //Each line read from the getdata.txt file will be in the form of control name followed by a colon followed by its setting as in Throttle:80
                    //The split function searches for the colon : and seperates the 2 strings and stores them in the splitter array.
                    string[] splitter = tmp.Split(':');
                    //Check we have 2 pieces of data IE a control and value
                    if (splitter.Length == 2)
                    {
                        string name = splitter[0];
                        string stringVal = splitter[1];
                        int intVal = 0;
                        float floatVal = 0.0f;
                        bool boolVal = false;
                        if (int.TryParse(stringVal, floatStyle, floatCulture, out intVal))
                        {
                            intVals.Add(name, intVal);
                        }
                        else if (float.TryParse(stringVal, floatStyle, floatCulture, out floatVal))
                        {
                            floatVals.Add(name, floatVal);
                        }
                        else if (bool.TryParse(stringVal, out boolVal))
                        {
                            boolVals.Add(name, boolVal);
                        }
                        else
                        {
                            stringVals.Add(name, stringVal);
                        }
                    }
                }
                return true;
            }
            else
                return false;
        }

        private void CopyData()
        {
            ZusiTcpTypeMaster zusiMaster = null;
            try
            {
                zusiMaster = new ZusiTcpTypeMaster("Railworks", null);
                zusiMaster.ErrorReceived += OnErrorRecieved;
                zusiMaster.Connect("localhost", 1435); //ToDo: Make more flexible.
                //Wait until he's connected.
                while (zusiMaster.ConnectionState == ConnectionState.Connecting)
                    Thread.Sleep(100);
                Thread.Sleep(100);
                if (zusiMaster.ConnectionState != ConnectionState.Connected)
                    throw new System.Exception();

                Dictionary<string, string> stringValsOld = new Dictionary<string, string>();
                Dictionary<string, int> intValsOld = new Dictionary<string, int>();
                Dictionary<string, float> floatValsOld = new Dictionary<string, float>();
                Dictionary<string, bool> boolValsOld = new Dictionary<string, bool>();

                while (Started)
                {
                    Dictionary<string, string> stringValsN = new Dictionary<string, string>();
                    Dictionary<string, int> intValsN = new Dictionary<string, int>();
                    Dictionary<string, float> floatValsN = new Dictionary<string, float>();
                    Dictionary<string, bool> boolValsN = new Dictionary<string, bool>();
                    readRailsimData(out stringValsN, out intValsN, out floatValsN, out boolValsN);

                    //Calculate Differences
                    Dictionary<string, string> stringVals = GetChanged<string>(stringValsOld, stringValsN);
                    Dictionary<string, int> intVals = GetChanged<int>(intValsOld, intValsN);
                    Dictionary<string, float> floatVals = GetChanged<float>(floatValsOld, floatValsN);
                    Dictionary<string, bool> boolVals = GetChanged<bool>(boolValsOld, boolValsN);

                    //Send Strings
                    foreach(KeyValuePair<string, string> itm1 in stringVals)
                    {
                        int id;
                        if (StringItems != null && StringItems.TryGetValue(itm1.Key, out id))
                        {
                            zusiMaster.SendString(itm1.Value, id);
                        }
                    }

                    //Send Floats
                    foreach(KeyValuePair<string, float> itm1 in floatVals)
                    {
                        int id;
                        if (SingleItems != null && SingleItems.TryGetValue(itm1.Key, out id))
                        {
                            zusiMaster.SendSingle(itm1.Value, id);
                        }
                    }

                    //Send Ints
                    foreach(KeyValuePair<string, int> itm1 in intVals)
                    {
                        int id;
                        if (IntItems != null && IntItems.TryGetValue(itm1.Key, out id))
                        {
                            zusiMaster.SendInt(itm1.Value, id);
                        }
                        if (IntAsSingleItems != null && IntAsSingleItems.TryGetValue(itm1.Key, out id))
                        {
                            zusiMaster.SendIntAsSingle(itm1.Value, id);
                        }
                        if (SingleItems != null && SingleItems.TryGetValue(itm1.Key, out id))
                        {
                            zusiMaster.SendSingle(itm1.Value, id);
                        }
                        if (BoolItems != null && BoolItems.TryGetValue(itm1.Key, out id))
                        {
                            zusiMaster.SendBoolAsSingle(itm1.Value != 0, id);
                        }
                        if (BoolItems != null && BoolItems.TryGetValue("!" + itm1.Key, out id))
                        {
                            zusiMaster.SendBoolAsSingle(itm1.Value == 0, id);
                        }
                    }

                    //Send Bools
                    foreach(KeyValuePair<string, bool> itm1 in boolVals)
                    {
                        int id;
                        if (StringItems != null && StringItems.TryGetValue(itm1.Key, out id))
                        {
                            zusiMaster.SendBoolAsSingle(itm1.Value, id);
                        }
                        if (StringItems != null && StringItems.TryGetValue("!" + itm1.Key, out id))
                        {
                            zusiMaster.SendBoolAsSingle(!itm1.Value, id);
                        }
                    }

                    stringValsOld = stringValsN;
                    intValsOld = intValsN;
                    floatValsOld = floatValsN;
                    boolValsOld = boolValsN;

                    Thread.Sleep(100);
                }
            }
            catch(System.Exception ex)
            {
                OnErrorRecieved(this, new ZusiTcpException("Error in the RwZusiConverter module.", ex));
            }
            finally
            {
                if (zusiMaster != null)
                    zusiMaster.Disconnect();
            }
        }

        private Dictionary<string, T> GetChanged<T>(Dictionary<string, T> oldVals, Dictionary<string, T> newVals)
        {
            Dictionary<string, T> retVal = new Dictionary<string, T>();
            foreach(KeyValuePair<string, T> key in newVals)
            {
                if (!key.Value.Equals(oldVals[key.Key]))
                    retVal.Add(key.Key, key.Value);
            }
            return retVal;
        }

        protected void OnErrorRecieved(object sender, ZusiTcpException ex)
        {
            if (ErrorReceived != null)
                ErrorReceived(sender, ex);
        }

        public event ErrorEvent ErrorReceived;
        public string RailworksPath {get; set;}
        public Dictionary<string, int> StringItems {get; set;}
        public Dictionary<string, int> SingleItems {get; set;}
        public Dictionary<string, int> IntItems {get; set;}
        public Dictionary<string, int> IntAsSingleItems {get; set;}
        public Dictionary<string, int> BoolItems {get; set;}

        public void Start()
        {
            if (Started)
                throw new System.InvalidOperationException();
            Started = true;
            CopyData();
        }
        public void StartAsync()
        {
            if (Started)
                throw new System.InvalidOperationException();
            Started = true;
            (new Thread(CopyData)).Start();
        }
        public bool Started {get; private set;}
        public void Stop()
        {
            Started = false;
        }
    }
}
