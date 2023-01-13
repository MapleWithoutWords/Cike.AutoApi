# AutoWebApi

#### 介绍
🔥自动api，让你的代码更加简洁🔥。如果你的控制器层只是像以下这种，转发了业务层的代码，那么自动api将非常适合您的项目。自动api会直接根据你业务层的方法，结合restful规范动态生成控制器。

> 控制器只做转发，没有做任何事情,形成了大量的冗余代码

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

#### 软件架构
* 本项目依赖于.net6


#### 安装教程

```shell
dotnet add package NET.AutoApi
```

#### 使用说明
* 在 ```Program.cs``` 中添加以下两段代码
```c#

builder.Services.AddAutoApiService(opt =>
{
    //NETServiceTest所在程序集添加进动态api配置
    opt.CreateConventional(typeof(NETServiceTest).Assembly);
});

//放在 app.MapControllers(); 前面
app.UseAutoApiService();
```

2. 业务层代码，只需要继承 ```IAutoApiService``` 接口就行

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
        /// 文件流使用，上传的时候正常传到报文体内就行了
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

3. 最终效果

![运行效果图](./doc/%E8%BF%90%E8%A1%8C%E6%95%88%E6%9E%9C%E5%9B%BE.png)

#### 使用本项目的框架

1. [Yi.Framework](https://gitee.com/ccnetcore/Yi)

