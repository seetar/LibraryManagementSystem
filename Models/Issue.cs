using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryManagementSystem.Models
{
    public class Issue
    {
        public Guid IssueId { get; set; }

        [Required]
        public Guid MemberId { get; set; }
        public Member Member { get; set; }

        [Required]
        public Guid BookId { get; set; }
        public Book Book { get; set; }
    }
}
