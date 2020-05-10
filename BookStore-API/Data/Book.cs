namespace BookStore_API.Data
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int? year { get; set; }
        public string Isbn { get; set; }
        public string Summary { get; set; }
        public  string Image { get; set; }
        public double? Price { get; set; }
        public int? AutorId { get; set; }
        public virtual Author Author { get; set; }
    }
}