
namespace FavFitApi.Models;

public class ActivityMapper
{

    public Activity CreateActivityToActivity(CreateActivityDto request)
    {
        
        var newActivity = new Activity
        {
            UserId = request.UserId,
            Title = request.Title,
            Type = request.Type,
            Date = ToUtc(request.Date),
            ElapsedTime = request.ElapsedTime,
            Distance = request.Distance,
            AverageSpeed = request.AverageSpeed,
            AverageCadence = request.AverageCadence,
            AveragePace = request.AveragePace,
            AverageHeartRate = request.AverageHeartRate,
            ElevationGain = request.ElevationGain,
            Calories = request.Calories
        };

        return newActivity;
    }

    public ActivityDto AcitivtyToActivityDto(Activity activity)
    {
        
        var activityDto = new ActivityDto
        {   
            Id = activity.Id,
            UserId = activity.UserId,
            Title = activity.Title,
            Type = activity.Type,
            Date = activity.Date,
            ElapsedTime = activity.ElapsedTime,
            Distance = activity.Distance,
            AverageSpeed = activity.AverageSpeed,
            AverageCadence = activity.AverageCadence,
            AveragePace = activity.AveragePace,
            AverageHeartRate = activity.AverageHeartRate,
            ElevationGain = activity.ElevationGain,
            Calories = activity.Calories,
            CreatedAt = activity.CreatedAt
        };

        return activityDto;
    }

    public static DateTime ToUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };

    
}