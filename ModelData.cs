namespace HouseProphecy.Components
{
    /// <summary>
    /// The parameters on which the rent price of the premises depends in the forecast
    /// </summary>
    public class ModelData
    {
        public string ZipCode { get; set; } = string.Empty;
        public string BedRooms { get; set; } = string.Empty;
        public string BathRooms { get; set; } = string.Empty;
        public string Square { get; set; } = string.Empty;
        public string CatsOk { get; set; } = string.Empty;
        public string DogsOk { get; set; } = string.Empty;
        public string Furnished { get; set; } = string.Empty;
        public string NoSmoking { get; set; } = string.Empty;
        public string WheelchairAccessible { get; set; } = string.Empty;
        public string HousingType { get; set; } = string.Empty;
        public string Laundry { get; set; } = string.Empty;
        public string LaundrySeparation { get; set; } = string.Empty;
        public string Parking { get; set; } = string.Empty;
    }
}