# UsedName

🌎 [**English**] [简体中文](https://github.com/LittleNightmare/UsedName/blob/Memory/README-CN.md)

A plugin developed for the case of FFXIV without a note name. Currently it is possible to record used names and add nicknames. The used names can be updated automatically (upon opening the corresponding list). Supported: Party List, Friend List, Company Member, and Subscribed Players

## How to use

Use '/pname' or '/pname update' to update data from FriendList

Use '/pname main' to open Main window

Use '/pname sub' to open Subscription window

Use '/pname search firstname lastname' to search 'firstname lastname's used name. I **recommend** using the right-click menu to search

Use '/pname nick firstname lastname nickname' set 'firstname lastname's nickname to 'nickname', only support player from FriendList (Format require:first last nickname; first last nick name)

Use '/pname config' show plugin's setting

## Special Use Case:

### Storing Names of Passersby

You can use the `UsedName` plugin to record the names of acquaintances you have encountered, as well as players you wish to block and the reasons for doing so.

#### Brief Instructions:

1. Open the plugin settings and check `Enable Auto update` and the sub-option `Enable Subscription`
2. Right-click on a player and select `Subscription` to add that player to your subscription list
3. Open the PlayerSearch and search for the player's name
4. Once you have found the player, you have successfully subscribed to them. The relevant information will automatically update whenever you open **any window** except for the blacklist

#### Detailed Instructions:

1. Open the plugin settings by typing `/pname config` and check `Enable Auto update` and the sub-option "Enable Subscription" You can also set the display name of the `Subscription` button.
2. When you encounter a player, right-click on that player and select `Subscription` or your modified `Subscribe String`. This will display the player's name in the subscription window that opens with the command "/pname sub."
3. Open the PlayerSearch and search for the player's name.
4. Once you find the player, their name will disappear from the subscription window, indicating that you have successfully subscribed to them. From this point on, relevant information will automatically update whenever you open **any window** that listed in TODO except for the blacklist.

#### Usage Scenarios:

* You encounter a good teammate or other player in the game, but for various reasons, you don't want to add them as a friend. You can use the subscription function to record their name so that you can contact them later.
* You encounter a problematic player in the game. You can use the subscription function to record their name and reason for blocking them. You can easily find them later by searching for their name in the plugin's stored player names.

# Know Issue
Not exist. If you find any problem, please make an Issue here

# TODO
- [x] Add/edit player's infomation through GUI
- [x] Support everywhere with ContentID. ~~In fact, I think current support parts is enough.~~ Currently, you can record information by subscribing to other players, and the following is a list that can support automatic update to get all records of the list. If you want addition list, please write it in issue
  - [x] FriendList
  - [x] PartyList
  - [ ] PlayerSearch
  - [ ] Members Online and on Home World
  - [x] Company Member
  - [ ] Application of Company
  - [ ] Novice Network(Mentor & New Adventurer/Returner)
- [ ] Support Blacklist
- [ ] Distinguish `Unable to Retrieve` with noral cross world friend
- [ ] Show nick name in FriendList

# Thanks

Thanks to each branch of FFLOGSViewer, and zhouhuichen741 pulled me back from far way
