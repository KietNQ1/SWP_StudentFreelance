
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using StudentFreelance.Models;
public class BankAccountViewModel
{
    [Required]
    [Display(Name = "Bank Name")]
    [MaxLength(100)]
    public string BankName { get; set; }

    [Required]
    [Display(Name = "Account Number")]
    [MaxLength(30)]
    public string AccountNumber { get; set; }

    [Required]
    [Display(Name = "Account Holder Name")]
    [MaxLength(100)]
    public string AccountHolderName { get; set; }
}
