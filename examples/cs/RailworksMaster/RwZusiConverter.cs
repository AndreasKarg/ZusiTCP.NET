using System.Collections.Generic;
using System.Threading;
using System.IO;
using Zusi_Datenausgabe;

namespace Railworks_GetData
{
    public class RwZusiConverter
    {
        public RwZusiConverter()
        {
            RailworksPath = (string)Microsoft.Win32.Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\railsimulator.com\\railworks\\", "Install_Path", "");
            Started = false;
            StringItems = new Dictionary<string, int>();
            SingleItems = new Dictionary<string, int>();
            SingleTransformation = new Dictionary<string, float>();
            IntItems = new Dictionary<string, int>();
            IntAsSingleItems = new Dictionary<string, int>();
            BoolItems = new Dictionary<string, int>();
        }
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
                            if (!intVals.ContainsKey(name))
                                intVals.Add(name, intVal);
                        }
                        else if (float.TryParse(stringVal, floatStyle, floatCulture, out floatVal))
                        {
                            if (!floatVals.ContainsKey(name))
                                floatVals.Add(name, floatVal);
                        }
                        else if (bool.TryParse(stringVal, out boolVal))
                        {
                            if (!boolVals.ContainsKey(name))
                                boolVals.Add(name, boolVal);
                        }
                        else
                        {
                            if (!stringVals.ContainsKey(name))
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

                System.Console.WriteLine("Connected");

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
                    if (readRailsimData(out stringValsN, out intValsN, out floatValsN, out boolValsN))
                        System.Console.Write(".");
                    else
                        System.Console.Write("-");

                    //Calculate Differences
                    Dictionary<string, string> stringVals = GetChanged<string>(stringValsOld, stringValsN);
                    Dictionary<string, int> intVals = GetChanged<int>(intValsOld, intValsN);
                    Dictionary<string, float> floatVals = GetChanged<float>(floatValsOld, floatValsN);
                    Dictionary<string, bool> boolVals = GetChanged<bool>(boolValsOld, boolValsN);




                    //Send Strings
                    foreach(KeyValuePair<string, string> itm1 in stringVals)
                    {
                    System.Console.WriteLine("?s " + itm1.Key + " -> " + itm1.Value);
                        int id;
                        if (StringItems != null && StringItems.TryGetValue(itm1.Key, out id))
                        {
                            System.Console.WriteLine("s0 " + itm1.Key + " -> " + itm1.Value + " -> " + id);
                            zusiMaster.SendString(itm1.Value, id);
                        }
                    }

                    //Send Floats
                    foreach(KeyValuePair<string, float> itm1 in floatVals)
                    {
                    System.Console.WriteLine("?f " + itm1.Key + " -> " + itm1.Value);
                        int id;
                        if (SingleItems != null && SingleItems.TryGetValue(itm1.Key, out id))
                        {
                            System.Console.WriteLine("f0 " + itm1.Key + " -> " + itm1.Value + " -> " + id);
                            zusiMaster.SendSingle(itm1.Value * GetSingleTransformation(itm1.Key), id);
                        }
                    }

                    //Send Ints
                    foreach(KeyValuePair<string, int> itm1 in intVals)
                    {
                    System.Console.WriteLine("?i " + itm1.Key + " -> " + itm1.Value);
                        int id;
                        if (IntItems != null && IntItems.TryGetValue(itm1.Key, out id))
                        {
                        System.Console.WriteLine("i0 " + itm1.Key + " -> " + itm1.Value + " -> " + id);
                            zusiMaster.SendInt(itm1.Value, id);
                        }
                        if (IntAsSingleItems != null && IntAsSingleItems.TryGetValue(itm1.Key, out id))
                        {
                        System.Console.WriteLine("i1 " + itm1.Key + " -> " + itm1.Value + " -> " + id);
                            zusiMaster.SendIntAsSingle(itm1.Value, id);
                        }
                        if (SingleItems != null && SingleItems.TryGetValue(itm1.Key, out id))
                        {
                        System.Console.WriteLine("i2 " + itm1.Key + " -> " + itm1.Value + " -> " + id);
                            zusiMaster.SendSingle(itm1.Value * GetSingleTransformation(itm1.Key), id);
                        }
                        if (BoolItems != null && BoolItems.TryGetValue(itm1.Key, out id))
                        {
                        System.Console.WriteLine("i3 " + itm1.Key + " -> " + itm1.Value + " -> " + id);
                            zusiMaster.SendBoolAsSingle(itm1.Value != 0, id);
                        }
                        if (BoolItems != null && BoolItems.TryGetValue("!" + itm1.Key, out id))
                        {
                        System.Console.WriteLine("i4 " + itm1.Key + " -> " + itm1.Value + " -> " + id);
                            zusiMaster.SendBoolAsSingle(itm1.Value == 0, id);
                        }
                    }

                    //Send Bools
                    foreach(KeyValuePair<string, bool> itm1 in boolVals)
                    {
                    System.Console.WriteLine("?b " + itm1.Key + " -> " + itm1.Value);
                        int id;
                        if (StringItems != null && StringItems.TryGetValue(itm1.Key, out id))
                        {
                        System.Console.WriteLine("b0 " + itm1.Key + " -> " + itm1.Value + " -> " + id);
                            zusiMaster.SendBoolAsSingle(itm1.Value, id);
                        }
                        if (StringItems != null && StringItems.TryGetValue("!" + itm1.Key, out id))
                        {
                        System.Console.WriteLine("b1 " + itm1.Key + " -> " + itm1.Value + " -> " + id);
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
                T val;
                if (oldVals.TryGetValue(key.Key, out val))
                {
                    if (val.Equals(key.Value))
                        continue;
                }
                retVal.Add(key.Key, key.Value);
            }
            return retVal;
        }

        protected void OnErrorRecieved(object sender, ZusiTcpException ex)
        {
            if (ErrorReceived != null)
                ErrorReceived(sender, ex);
        }

        private float GetSingleTransformation(string id)
        {
            float val;
            if (SingleTransformation != null && SingleTransformation.TryGetValue(id, out val))
                return val;
            return 1.0f;
        }
        public event ErrorEvent ErrorReceived;
        public string RailworksPath {get; set;}
        public Dictionary<string, int> StringItems {get; set;}
        public Dictionary<string, int> SingleItems {get; set;}
        public Dictionary<string, float> SingleTransformation {get; set;}
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
