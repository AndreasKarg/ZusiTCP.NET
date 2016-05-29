using System;

namespace ZusiTcpInterface.Zusi3
{
  [Obsolete]
  public struct SifaStatus : IEquatable<SifaStatus>
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

    #region Equality operator etc

    public bool Equals(SifaStatus other)
    {
      return string.Equals(_type, other._type) && _pilotLightOn == other._pilotLightOn && _hornState == other._hornState && _mainSwitchEnabled == other._mainSwitchEnabled && _disruptionOverrideSwitchEnabled == other._disruptionOverrideSwitchEnabled && _airCutoffValveOpen == other._airCutoffValveOpen;
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      return obj is SifaStatus && Equals((SifaStatus) obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        var hashCode = (_type != null ? _type.GetHashCode() : 0);
        hashCode = (hashCode*397) ^ _pilotLightOn.GetHashCode();
        hashCode = (hashCode*397) ^ (int) _hornState;
        hashCode = (hashCode*397) ^ _mainSwitchEnabled.GetHashCode();
        hashCode = (hashCode*397) ^ _disruptionOverrideSwitchEnabled.GetHashCode();
        hashCode = (hashCode*397) ^ _airCutoffValveOpen.GetHashCode();
        return hashCode;
      }
    }

    public static bool operator ==(SifaStatus left, SifaStatus right)
    {
      return left.Equals(right);
    }

    public static bool operator !=(SifaStatus left, SifaStatus right)
    {
      return !left.Equals(right);
    }

#endregion Equality operator etc
  }
}