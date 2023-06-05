
using Volo.Abp;

namespace Abp.AutoWebApi.Test.Services
{
    public class UserService : IRemoteService
    {

        public async Task<String> GetAsync()
        {
            return "Hello,Word!";
        }
    }
}
