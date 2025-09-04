namespace GorevNet.Models
{
    public class BulkCreateEmployeeModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Department { get; set; }
        public string Position { get; set; }
    }

    public class BulkOperationResult
    {
        public string Email { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}