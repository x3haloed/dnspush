namespace DnsPush.Server.ServiceProxy.Models
{
    public class PutResultModel
    {
        public bool Success { get; set; }
        public string[] Errors { get; set; }
    }
}