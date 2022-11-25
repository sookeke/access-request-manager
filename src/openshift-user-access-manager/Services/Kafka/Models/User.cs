namespace UserAccessManager.Services.Kafka.Models
{
    public class Result
    {
        public string? Name { get; set; }
        public string? First_Name { get; set; }
        public string? Last_Name { get; set; }
        public string? Email { get; set; }
    }
    public class User
    {
        public List<Result> result { get; set; } = new();
    }


}
