using FavFitApi.Enums;

namespace FavFitApi.Models;

public class UpdateActivityDto
{
    public long Id {get; set;}

    public string? NewTitle {get; set;}

    public ActivityType? NewType {get; set;}

    public DateTime? NewDate {get; set;}

    public TimeSpan? NewElapsedTime {get; set;}

    public double? NewDistance {get; set;}

    public double? NewAverageSpeed {get; set;}

    public int? NewAverageCadence {get; set;}

    public TimeSpan? NewAveragePace {get; set;}

    public int? NewAverageHeartRate {get; set;}

    public double? NewElevationGain {get; set;}

    public int? NewCalories {get; set;}
}