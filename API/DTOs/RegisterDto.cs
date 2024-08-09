﻿using System.ComponentModel.DataAnnotations;

namespace API;

public class RegisterDto
{
    [Required]
    public required string Username { get; set; }
    [Required]
    [StringLength(8, MinimumLength = 4)]
    public required string Password { get; set; }
}
