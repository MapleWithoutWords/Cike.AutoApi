using Cike.AutoWebApi.Setting;

namespace Cike.AutoWebApi.Test.Services
{
    public class UserService : IAutoApiService
    {

        public async Task<String> GetAsync()
        {
            return "Hello,Word!";
        }
    }
}
