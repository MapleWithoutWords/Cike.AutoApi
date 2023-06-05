using Abp.Service.Test.TestService.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Content;

namespace Abp.Service.Test.TestService
{
    public interface ITestService : IRemoteService
    {

        public Task<string> GetListAsync(string keyword);

        public Task<List<string>> CreateAsync(TestCreateUpdateInput input);
        public Task<List<string>> UpdateAsync(Guid id, TestCreateUpdateInput input);

        public Task<string> ImportAsync(IRemoteStreamContent file);
    }
}
