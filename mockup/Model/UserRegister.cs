using System.ComponentModel.DataAnnotations;

namespace mockup.Model
{
    public class UserRegister
    {
        [Required,EmailAddress]
        
        public string Email { get; set; }
        [Required,MaxLength(6)]
        public string Password { get; set; }
        [Required,Compare("Password")]
        public string ConfirmPassword { get; set; } 
    }
}
