using Abp.Service.Test.TestService.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Content;

namespace Abp.Service.Test.TestService.Impl
{
    public class TestService : ITestService
    {
        public async Task<List<string>> CreateAsync(TestCreateUpdateInput input)
        {
            return new List<string>
            {
                $"{input.Code}|{input.Name}"
            };
        }

        public async Task<string> GetListAsync(string keyword)
        {
            return keyword;
        }

        public async Task<List<string>> UpdateAsync(Guid id, TestCreateUpdateInput input)
        {
            return new List<string>
            {
                $"{id}|{input.Code}|{input.Name}"
            };
        }

        /// <summary>
        /// 文件流使用
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public async Task<string> ImportAsync(IRemoteStreamContent file)
        {
            using var fileStream = file.GetStream();


            return file.FileName;
        }
    }
}
