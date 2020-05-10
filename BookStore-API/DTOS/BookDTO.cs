using System.Collections.Generic;

namespace BookStore_API.DTOS
{
    public class BookDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int? year { get; set; }
        public string Isbn { get; set; }
        public string Summary { get; set; }
        public string Image { get; set; }
        public double? Price { get; set; }
        public int? AutorId { get; set; }
        public virtual AuthorDTO Author { get; set; }
    }
}