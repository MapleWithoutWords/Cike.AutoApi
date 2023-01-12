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
dotnet add package NET.AutoApi --version 1.0.1
```

#### 使用说明
1. 在 ```Program.cs``` 中添加以下两段代码
```c#

builder.Services.AddAutoApiService(opt =>
{
    opt.CreateConventional(typeof(NETServiceTest).Assembly);
});

//这段要放在 app.MapControllers(); 前面
app.UseAutoApiService();
```

#### 使用本项目的框架

1. [Yi.Framework](https://gitee.com/ccnetcore/Yi)


