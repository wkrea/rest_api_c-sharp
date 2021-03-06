﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OpenImis.ModulesV1.InsureeModule.Models
{
    public class Contribution
    {
        [Required]
        //[InsureeNumber(ErrorMessage = "1:Wrong format or missing insurance number ")]
        public string InsuranceNumber { get; set; }
        [Required(ErrorMessage = "7-Wrong or missing payer")]
        public string Payer { get; set; }
        [Required(ErrorMessage = "4:Wrong or missing payment date")]
        //[ValidDate(ErrorMessage = "4:Wrong or missing payment date")]
        public string PaymentDate { get; set; }
        [Required(ErrorMessage = "3:Wrong or missing  product ")]
        public string ProductCode { get; set; }
        [Required]
        public decimal Amount { get; set; }
        [Required(ErrorMessage = "8:Missing receipt no.")]
        public string ReceiptNumber { get; set; }
        [Required(ErrorMessage = "6:Wrong or missing payment type")]
        [StringLength(1, ErrorMessage = "6:Wrong or missing payment type")]
        public string PaymentType { get; set; }
        [Required]
        public ReactionType ReactionType { get; set; }
        [StringLength(1, ErrorMessage = "5:Wrong contribution category")]
        public string ContributionCategory { get; set; }
    }
}
