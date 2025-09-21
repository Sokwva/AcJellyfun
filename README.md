# Title

## 目录组成

### 常规的目录应当包含：
1. 视频本体文件
2. nfo描述文件

## 标准参考：
1. NFO文件描述 https://kodi.wiki/view/NFO_files/Movies

### 转换
dougaInfo => nfo

|dougaInfo|nfo|
|--|--|
|channel|类型|
|up|actor.name|
|tag|tag|
|commentCount|内容简介|
|bananaCount|内容简介|
|stowCount|内容简介|
|likeCount|内容简介|
|viewCount|内容简介|

###
媒体库为混合电影和剧集的库
文件夹下只有一个视频文件
    判定为单P视频 默认为电影
    视频封面应用于封面和背景图
否则
    判定为多分P视频
    规则：
        1-1-1080p+
        第一季第一集1080p+分辨率的视频