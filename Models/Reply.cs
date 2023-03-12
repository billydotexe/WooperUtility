namespace WooperUtility.Models
{
    public class Reply
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string? Message { get; set; }
        public List<string> Parameters { get; set; } = new();
        public string? ImgUrl { get; set; }
        public List<Button> Buttons { get; set; } = new();
    }
}
