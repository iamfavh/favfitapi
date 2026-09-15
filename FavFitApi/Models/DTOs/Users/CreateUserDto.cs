using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FavFitApi.Models;

public class CreateUserDto
{    
    public string? FirstName {get; set;}
    
    public string? MiddleName {get; set;}
    
    public string? LastName {get; set;}

    [Required]
    public string Email {get; set;} = string.Empty;

    [Required]    
    public string Password {get; set;} = string.Empty;
}