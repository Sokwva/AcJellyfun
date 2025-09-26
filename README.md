# AcJellyfun
## 介绍
这是一个用于对从AcFun下载的视频进行刮削的Jellyfin插件，主要通过请求对应的接口来获取视频信息（播放量、收藏点赞投蕉、简介等数据）、Up主信息（用户名、头像、主页等）、Staff信息，并写入到对应的数据文件中。
## 慢速开始
1. 准备AcFun的稿件信息解析服务：  
    例子：使用[Sokwva/AcFun-DougaInfo](https://github.com/Sokwva/AcFun-DougaInfo)获取相对于的acid的视频信息。

    从对应的github仓库拉取源代码，在源代码目录下执行go build以编译服务程序；复制粘贴conf.example.toml文件，重命名为conf.toml；此服务程序依赖于Redis作为缓存，所以还需要安装一个Redis实例，然后在conf.toml中指定对应的Redis地址、端口。使用命令行启动服务。

2. 拉取源代码以编译产物：  
    使用git拉取本仓库，然后使用dotnet build构建插件，将产物从./Jellyfin.Plugin.AcJellyfun/bin/Release复制到对应的Jellyfin插件目录下（一般在“C:\Users\[用户名]\AppData\Local\jellyfin\plugins”）。

3. 启动Jellyfin并配置插件：  
    启动Jellyfin后，需要在插件的配置页面中配置稿件信息解析服务的地址，配置完之后点击保存即可。

4. 通过AcFunACID刮削：   
    在Jellyfin相应的媒体页面中，点击三个点的菜单（在已看、收藏按钮之后），选择“刮削元数据”，并将视频对应的Acid输入到“AcFun Id”一栏，点击搜索即可。
## 目录结构
```text
├─.vscode # vscode的项目配置文件
├─Jellyfin.Plugin.AcJellyfun # 源代码
│  ├─Configuration # 配置页面和配置定义
│  ├─Model # 接口定义
│  └─Providers # 内容供应模块
│      └─ExternalID # 外部内容供应模块的标识ID
└─refer # 参考文件
```

## 展示