namespace DnsPush.Server.ServiceProxy.Models
{
    public class PatchResultModel
    {
        public bool Success { get; set; }
        public string[] Errors { get; set; }
    }
}