using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BookStore_UI.Models
{
    public class Author
    {
        public int Id { get; set; }
        [Required]
        [DisplayName("First name")]
        public string Firstname { get; set; }
        [Required]
        [DisplayName("Lastname")]
        public string Lastname { get; set; }
        [Required]
        [DisplayName("Biogrphy")]
        [StringLength(250)]
        public string Bio { get; set; }

        public virtual IList<Book> Books { get; set; }
    }
}
