namespace DnsPush.Server.Models
{
    public class PatchResultModel
    {
        public bool Success { get; set; }
        public string[] Errors { get; set; }
    }
}