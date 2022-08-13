# UsedName

一个针对FFXIV无备注名情况开发的插件。目前可以记录曾用名，添加昵称。可以通过网络包，自动更新曾用名（在打开对应列表时）支持范围包括：小队列表，好友名单，玩家搜索

A plugin developed for the case of FFXIV without a note name. Currently it is possible to record used names and add nicknames. The used names can be updated automatically through the network package. (When opening the list) Supported: Party List, Friend List, Player Search

## How to use

使用'/pname update'或'/pname'更新好友列表

使用'/pname search xxxx'搜索xxxx的曾用名。建议使用右键菜单搜索

使用'/pname nick xxxx aaaa'设置xxxx的昵称为aaaa，仅支持好友

使用'/pname config'显示插件设置

Use '/pname' or '/pname update' to update data from FriendList

Use '/pname search firstname lastname' to search 'firstname lastname's used name. I **recommend** using the right-click menu to search

Use '/pname nick firstname lastname nickname' set 'firstname lastname's nickname to 'nickname', only support player from FriendList (Format require:first last nickname; first last nick name)

Use '/pname config' show plugin's setting

# TODO
- [x] GUI能游戏内添加/记录昵称
- [ ] 给大部分能有ContentID的地方，支持功能，不局限于好友。（新人频道的指导者和新人）
- [ ] 黑名单支持 Support Blacklist
- [ ] 区分`找不到该角色`和普通跨服好友 Distinguish `Unable to Retrieve` with noral cross world friend

# Thanks
感谢FFLogsViewer各个分支的参考，还有zhouhuichen741从远路把我拽回来。
