using FavFitApi.Enums;
using FavFitApi.Models;
using Fit = Dynastream.Fit;

namespace FavFitApi.Services;

public record FitImportResult(Activity? Activity, string? Error);

public class FitImportService
{
    public const long MaxFileSizeBytes = 20 * 1024 * 1024;

    public FitImportResult Import(Stream fitStream, long userId)
    {
        var decoder = new Fit.Decode();

        if (!decoder.IsFIT(fitStream))
            return new FitImportResult(null, "The uploaded file is not a FIT file.");

        fitStream.Position = 0;

        if (!decoder.CheckIntegrity(fitStream))
            return new FitImportResult(null, "The FIT file failed its integrity check. It may be truncated or corrupted.");

        fitStream.Position = 0;

        var listener = new Fit.FitListener();
        decoder.MesgEvent += listener.OnMesg;

        try
        {
            if (!decoder.Read(fitStream))
                return new FitImportResult(null, "The FIT file could not be decoded.");
        }
        catch (Fit.FitException ex)
        {
            return new FitImportResult(null, $"The FIT file could not be decoded: {ex.Message}");
        }

        var session = listener.FitMessages.SessionMesgs.FirstOrDefault();

        if (session == null)
            return new FitImportResult(null, "The FIT file does not contain any session data.");

        var activity = new Activity
        {
            UserId = userId,
            Type = MapSport(session.GetSport(), session.GetSubSport())
        };

        // Values are stored in FIT's native units: meters, meters/second, seconds, kcal.
        // Rounding matches FIT's field resolution and strips float-to-double noise.
        if (session.GetStartTime() is { } startTime)
            activity.Date = ActivityMapper.ToUtc(startTime.GetDateTime());

        if (session.GetTotalElapsedTime() is { } elapsedSeconds)
            activity.ElapsedTime = TimeSpan.FromSeconds(Math.Round((double)elapsedSeconds, 3));

        if (session.GetTotalDistance() is { } distanceMeters)
            activity.Distance = Math.Round((double)distanceMeters, 2);

        if ((session.GetEnhancedAvgSpeed() ?? session.GetAvgSpeed()) is { } speedMetersPerSecond)
            activity.AverageSpeed = Math.Round((double)speedMetersPerSecond, 3);

        if (session.GetAvgHeartRate() is { } heartRate)
            activity.AverageHeartRate = heartRate;

        if (session.GetTotalCalories() is { } calories)
            activity.Calories = calories;

        if (session.GetTotalAscent() is { } ascentMeters)
            activity.ElevationGain = ascentMeters;

        return new FitImportResult(activity, null);
    }

    private static ActivityType MapSport(Fit.Sport? sport, Fit.SubSport? subSport) => sport switch
    {
        Fit.Sport.Running => ActivityType.Run,
        Fit.Sport.Cycling => ActivityType.Bike,

        // Closest existing ActivityType for other FIT sports
        Fit.Sport.EBiking => ActivityType.Bike,
        Fit.Sport.WheelchairPushRun => ActivityType.Run,
        Fit.Sport.Hiking or Fit.Sport.Mountaineering or Fit.Sport.Snowshoeing => ActivityType.Hike,
        Fit.Sport.Walking or Fit.Sport.WheelchairPushWalk => ActivityType.Walk,
        Fit.Sport.Swimming => ActivityType.Swim,
        Fit.Sport.Training => ActivityType.WeightLifting,
        Fit.Sport.FitnessEquipment when subSport is Fit.SubSport.Treadmill or Fit.SubSport.IndoorRunning => ActivityType.Run,
        Fit.Sport.FitnessEquipment when subSport is Fit.SubSport.IndoorCycling or Fit.SubSport.Spin => ActivityType.Bike,
        Fit.Sport.FitnessEquipment when subSport is Fit.SubSport.IndoorWalking => ActivityType.Walk,

        // No reasonable match (team sports, water sports, etc.)
        _ => ActivityType.Walk
    };
}
