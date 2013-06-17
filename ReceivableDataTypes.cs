namespace Zusi_Datenausgabe
{
  /// <summary>
  /// Mirrors the brake settings modelled by Zusi.
  /// </summary>
  public struct BrakeConfiguration
  {
    /// <summary>
    /// Indicates whether a magnetic brake is enabled (not active) on the train.
    /// </summary>
    public bool HasMgBrake { get; set; }

    /// <summary>
    /// Contains the brake pitch used on the train.
    /// </summary>
    public BrakePitch Pitch { get; set; }
  }

  /// <summary>
  /// Mirrors the brake pitches modeled by Zusi. For magnetic brake equipment <see cref="BrakeConfiguration"/>.
  /// </summary>
  public enum BrakePitch
  {
    /// <summary>
    /// The train is set to brake pitch (Bremsstellung) G
    /// </summary>
    G,

    /// <summary>
    /// The train is set to brake pitch (Bremsstellung) P
    /// </summary>
    P,

    /// <summary>
    /// The train is set to brake pitch (Bremsstellung) R
    /// </summary>
    R,
  }

  /// <summary>
  /// Mirrors the various PZB types modelled by Zusi.
  /// </summary>
  public enum PZBSystem
  {
    /// <summary>
    /// The train does not have any PZB system.
    /// </summary>
    None = 0,

    /// <summary>
    /// The train is equipped with Indusi H54.
    /// </summary>
    IndusiH54 = 1,

    /// <summary>
    /// The train is equipped with Indusi I54.
    /// </summary>
    IndusiI54 = 2,

    /// <summary>
    /// The train is equipped with Indusi I60.
    /// </summary>
    IndusiI60 = 3,

    /// <summary>
    /// The train is equipped with Indusi I60R.
    /// </summary>
    IndusiI60R = 4,

    /// <summary>
    /// The train is equipped with PZB 90 V1.5.
    /// </summary>
    PZB90V15 = 5,

    /// <summary>
    /// The train is equipped with PZB 90 V1.6.
    /// </summary>
    PZB90V16 = 6,

    /// <summary>
    /// The train is equipped with PZ80.
    /// </summary>
    PZ80 = 7,

    /// <summary>
    /// The train is equipped with PZ80R.
    /// </summary>
    PZ80R = 8,

    /// <summary>
    /// The train is equipped with LZB80/I80.
    /// </summary>
    LZB80I80 = 9,

    /// <summary>
    /// The train is equipped with SBB Signum.
    /// </summary>
    SBBSignum = 10,
  }

  /// <summary>
  /// Mirrors the various door states modeled by Zusi.
  /// </summary>
  public enum DoorState
  {
    /// <summary>
    /// Doors are released and can be opened when the train stops.
    /// </summary>
    Released = 0,

    /// <summary>
    /// Doors are open.
    /// </summary>
    Open = 1,

    /// <summary>
    /// The conductor/PA system is announcing the imminent departure of the train.
    /// </summary>
    Announcement = 2,

    /// <summary>
    /// For door control systems of type "Selbstabfertigung" (= Handled by the train driver),
    /// the doors can now be closed.
    /// </summary>
    ReadyToClose = 3,

    /// <summary>
    /// The doors are in the process of closing.
    /// </summary>
    Closing = 4,

    /// <summary>
    /// The doors are closed but the train is not yet allowed to depart.
    /// </summary>
    Closed = 5,

    /// <summary>
    /// The train is allowed to depart.
    /// </summary>
    Depart = 6,

    /// <summary>
    /// The doors are closed and locked.
    /// </summary>
    Locked = 7,
  }
}