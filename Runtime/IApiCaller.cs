using System.Threading.Tasks;
namespace live.videosdk
{
    public interface IApiCaller
    {
        Task<string> CallApiGet(string url, string token);
        Task<string> CallApiPost(string url, string token);
    }
}