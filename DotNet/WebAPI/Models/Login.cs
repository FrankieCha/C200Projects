﻿using System.ComponentModel.DataAnnotations;

#nullable disable

namespace C200.WebApi.Models;

public class Login
{
   [Required]
   public string UserId { get; set; }
   
   [Required]
   public string Password { get; set; }
}

