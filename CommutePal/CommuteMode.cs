namespace CommutePal;

public enum CommuteMode
{
    Bike,
    Car,
    PublicTransport,
    Home,
}

public static class CommuteModeExtensions
{
    public static string DisplayName(this CommuteMode mode) => mode switch
    {
        CommuteMode.Bike => "Bike",
        CommuteMode.Car => "Car",
        CommuteMode.PublicTransport => "Public transport",
        CommuteMode.Home => "Home",
        _ => mode.ToString(),
    };
}
