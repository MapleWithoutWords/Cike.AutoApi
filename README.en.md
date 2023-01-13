# AutoWebApi

#### Description
🔥Automatic api to make your code more concise 🔥. If your controller layer simply relays code from the business layer, like the following, then the automated api is a great fit for your project. The automated api dynamically generates controllers directly based on your business-layer methods, combined with restful specifications.

> The controller just forwards and doesn't do anything, creating a lot of redundant code

```c#
public class UserController:ControllerBase
{
    private readonly IUserAppService _userAppService;
    public UserController(IUserAppService userAppService)
    {
        _userAppService=userAppService;
    }

    [HttpGet]
    public async Task<PageResult<List<xxxDto>>> GetListAsync(xxxDto input)
    {
        return await _userAppService.GetListAsync(input);
    }
    [HttpPost]
    public async Task<xxxDto> CreateAsync(xxxDto input)
    {
        return await _userAppService.CreateAsync(input);
    }
    [HttpPost]
    public async Task<xxxDto> UpdateAsync(Guid id,xxxDto input)
    {
        return await _userAppService.UpdateAsync(id,input);
    }
}
```


#### Software Architecture
* This project relies on.net6

#### Installation

```shell
dotnet add package NET.AutoApi
```

#### Instructions

1. Add the following two pieces of code in ```Program.cs```
```c#

builder.Services.AddAutoApiService(opt =>
{
    //Add dynamic api configuration to the assembly where NETServiceTest resides
    opt.CreateConventional(typeof(NETServiceTest).Assembly);
});

// Put it in app.MapControllers(); In front
app.UseAutoApiService();
```

2. Business layer code, just need to inherit ```IAutoApiService``` interface

```c#
    public class TestService : IAutoApiService
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
        /// Upload file. If you upload files,Please use IAutoApiStreamContent[] .
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public async Task<string> ImportAsync(IAutoApiStreamContent file)
        {
            using var fileStream = file.GetStream();


            return file.FileName;
        }
    }
```


3. Final effect

![Final effect](./doc/%E8%BF%90%E8%A1%8C%E6%95%88%E6%9E%9C%E5%9B%BEen.png)



