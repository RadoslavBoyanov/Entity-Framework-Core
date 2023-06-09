﻿
using System.ComponentModel.DataAnnotations;

namespace VaporStore.DataProcessor.ImportDto
{
    public class ImportUsersDto
    {

        [Required]
        [MinLength(3), MaxLength(20)]
        public string Username { get; set; }

        [Required]
        [RegularExpression(@"^([A-Z]{1}[a-z]+)\\s([A-Z]{1}[a-z]+)$")]
        public string FullName { get; set; }


        [Required]
        public string Email { get; set; }

        [Required, Range(3, 103)]
        public int Age { get; set; }

        public ImportUserCardsDto[] Cards { get; set; }
    }
}
