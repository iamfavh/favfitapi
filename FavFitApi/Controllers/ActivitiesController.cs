using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FavFitApi.Models;
using FavFitApi.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace FavFitApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ActivitiesController : ControllerBase
{
    private readonly FavFitdbContext _context;
    private readonly ActivityMapper _activityMapper;

    public ActivitiesController(FavFitdbContext context, ActivityMapper activityMapper)
    {
        _context = context;
        _activityMapper = activityMapper;
    }

    [HttpPost]
    [EndpointSummary("Creates an activity")]
    [EndpointDescription("Activity Type Options: Bike = 1, Run = 2, Hike = 3, Walk = 4, Swim = 5, WeightLifting = 6")]
    public async Task<ActionResult<Activity>> CreateActivity(CreateActivityDto request)
    {   
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (userId == null)
            return Unauthorized("Not authorized to perform that request");

        if(!ModelState.IsValid)
            return BadRequest(ModelState);

        var existingUser = await _context.Users.FindAsync(request.UserId);

        if (existingUser == null || userId != existingUser.Id.ToString())
            return Unauthorized("Not authorized to perform that request");

        var newActivity = _activityMapper.CreateActivityToActivity(request);

        await _context.Activities.AddAsync(newActivity);

        await _context.SaveChangesAsync();

        var response = _activityMapper.AcitivtyToActivityDto(newActivity);

        return CreatedAtAction(nameof(GetActivityById), new {id = newActivity.Id}, response); 
    }

    [HttpGet]
    [Route("{id}")]
    [EndpointSummary("Returns an activity")]
    public async Task<ActionResult<ActivityDto>> GetActivityById(long id)
    {   
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
            return Unauthorized("Not authorized to perform that request");

        var existingActivity = await _context.Activities.FindAsync(id);

        if (existingActivity == null || userId != existingActivity.UserId.ToString())
            return NotFound();
        
        var activityDto = _activityMapper.AcitivtyToActivityDto(existingActivity);
        
        return activityDto;
    }

    [HttpPatch]
    [EndpointSummary("Updates an activity")]
    public async Task<ActionResult<UpdateActivityDto>> UpdateActivity(UpdateActivityDto request)
    {   
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existingActivity = await _context.Activities.FindAsync(request.Id);

        if (existingActivity == null || userId != existingActivity.UserId.ToString())
            return NotFound();
        
        if (request.NewTitle != null)
            existingActivity.Title = request.NewTitle;
        
        existingActivity.Type = request.NewType ?? existingActivity.Type;
        existingActivity.Date = request.NewDate is { } newDate ? ActivityMapper.ToUtc(newDate) : existingActivity.Date;
        existingActivity.ElapsedTime = request.NewElapsedTime ?? existingActivity.ElapsedTime;
        existingActivity.Distance = request.NewDistance ?? existingActivity.Distance;
        existingActivity.AverageSpeed = request.NewAverageSpeed ?? existingActivity.AverageSpeed;
        existingActivity.AverageCadence = request.NewAverageCadence ?? existingActivity.AverageCadence;
        existingActivity.AveragePace = request.NewAveragePace ?? existingActivity.AveragePace;
        existingActivity.AverageHeartRate = request.NewAverageHeartRate ?? existingActivity.AverageHeartRate;
        existingActivity.ElevationGain = request.NewElevationGain ?? existingActivity.ElevationGain;
        existingActivity.Calories = request.NewCalories ?? existingActivity.Calories;
        
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete]
    [EndpointSummary("Deletes an activity")]
    public async Task<ActionResult> DeleteActivity(long id)
    {   
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
            return Unauthorized("Not authorized to perform that request");

        var existingActivity = await _context.Activities.FindAsync(id);

        if (existingActivity == null || userId != existingActivity.UserId.ToString())
            return NotFound();
        
        _context.Activities.Remove(existingActivity);

        await _context.SaveChangesAsync();

        return NoContent();
    }
    
}