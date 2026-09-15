using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FavFitApi.Models;

public class AuthDto
{
    [Required]
    public string Email {get; set;} = string.Empty;

    [Required]
    public string Password {get; set;} = string.Empty;
}