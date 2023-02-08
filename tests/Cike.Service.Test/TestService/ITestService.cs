using Cike.AutoWebApi.ModelBinding;
using Cike.AutoWebApi.Setting;
using Cike.Service.Test.TestService.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Service.Test.TestService
{
    public interface ITestService : IAutoApiService
    {

        public Task<string> GetListAsync(string keyword);

        public Task<List<string>> CreateAsync(TestCreateUpdateInput input);
        public Task<List<string>> UpdateAsync(Guid id, TestCreateUpdateInput input);

        public Task<string> ImportAsync(IAutoApiStreamContent file);
    }
}
