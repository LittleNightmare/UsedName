# UsedName

一个针对FFXIV无备注名情况开发的插件。目前可以记录曾用名，添加昵称。可以自动更新曾用名（在打开对应列表时）支持范围包括：小队列表，好友名单，部队成员，以及订阅过的玩家

A plugin developed for the case of FFXIV without a note name. Currently it is possible to record used names and add nicknames. The used names can be updated automatically (upon opening the corresponding list). Supported: Party List, Friend List, Company Member, and Subscribed Players

## How to use

使用'/pname update'或'/pname'更新好友列表

使用'/pname main'打开主窗口

使用'/pname sub'打开订阅窗口

使用'/pname search xxxx'搜索xxxx的曾用名。**建议**使用右键菜单搜索

使用'/pname nick xxxx aaaa'设置xxxx的昵称为aaaa，仅支持好友

使用'/pname config'显示插件设置

Use '/pname' or '/pname update' to update data from FriendList

Use '/pname main' to open Main window

Use '/pname sub' to open Subscription window

Use '/pname search firstname lastname' to search 'firstname lastname's used name. I **recommend** using the right-click menu to search

Use '/pname nick firstname lastname nickname' set 'firstname lastname's nickname to 'nickname', only support player from FriendList (Format require:first last nickname; first last nick name)

Use '/pname config' show plugin's setting

# Know Issue
目前没有，如果你有发现，请给我提一个Issue 

Not exist. If you find any problem, please make an Issue here

# TODO
- [x] GUI能游戏内添加/记录昵称
- [x] 给能有ContentID的地方，支持功能。~~事实上，我觉得目前的应该够了。~~ 目前可以通过订阅其他玩家来记录信息，下面是可以支持自动获取全部的列表。 如果你想要额外的，可以给我提个issue Support everywhere with ContentID. ~~In fact, I think current support parts is enough.~~ Currently, you can record information by subscribing to other players, and the following is a list that can support automatic update to get all records of the list. If you want addition list, please write it in issue
  - [x] 好友列表 FriendList
  - [x] 小队列表 PartyList
  - [ ] 玩家搜索 PlayerSearch
  - [ ] 原始服务器在线成员 Members Online and on Home World
  - [x] 部队成员 Company Member
  - [ ] 入队申请 Application of Company
  - [ ] 新人频道（指导者 & 新人/回归者）Novice Network(Mentor & New Adventurer/Returner)
- [ ] 黑名单支持 Support Blacklist
- [ ] 区分`找不到该角色`和普通跨服好友 Distinguish `Unable to Retrieve` with noral cross world friend
- [ ] 好友列表显示昵称 Show nick name in FriendList

# Thanks
感谢FFLogsViewer各个分支的参考，还有zhouhuichen741从远路把我拽回来。
