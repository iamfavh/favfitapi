using System.ComponentModel.DataAnnotations;

namespace FavFitApi.Models;

public class AuthRequestDto
{   
    [Required]
    public string AccessToken {get; set;} = string.Empty;

    [Required]
    public string RefreshToken {get; set;} = null!;
}