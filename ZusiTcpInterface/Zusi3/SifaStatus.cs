using System;

namespace ZusiTcpInterface.Zusi3
{
  public class SifaStatus : IProtocolChunk
  {
    private readonly string _type;
    private readonly bool _pilotLightOn;
    private readonly SifaHornState _hornState;
    private readonly bool _mainSwitchEnabled;
    private readonly bool _disruptionOverrideSwitchEnabled;
    private readonly bool _airCutoffValveOpen;

    public SifaStatus(string type, bool pilotLightOn, bool mainSwitchEnabled, SifaHornState hornState, bool disruptionOverrideSwitchEnabled, bool airCutoffValveOpen)
    {
      _mainSwitchEnabled = mainSwitchEnabled;
      _type = type;
      _pilotLightOn = pilotLightOn;
      _hornState = hornState;
      _disruptionOverrideSwitchEnabled = disruptionOverrideSwitchEnabled;
      _airCutoffValveOpen = airCutoffValveOpen;
    }

    public String Type
    {
      get { return _type; }
    }

    public bool PilotLightOn
    {
      get { return _pilotLightOn; }
    }

    public SifaHornState HornState
    {
      get { return _hornState; }
    }

    public bool MainSwitchEnabled
    {
      get { return _mainSwitchEnabled; }
    }

    public bool DisruptionOverrideSwitchEnabled
    {
      get { return _disruptionOverrideSwitchEnabled; }
    }

    public bool AirCutoffValveOpen
    {
      get { return _airCutoffValveOpen; }
    }

    public override string ToString()
    {
      return string.Format("Type: {0}, PilotLightOn: {1}, HornState: {2}, MainSwitchEnabled: {3}, DisruptionOverrideSwitchEnabled: {4}, AirCutoffValveOpen: {5}", Type, PilotLightOn, HornState, MainSwitchEnabled, DisruptionOverrideSwitchEnabled, AirCutoffValveOpen);
    }
  }
}