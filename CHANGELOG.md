## Changelog
**3.0.0**

A year of changes, should've kept track. But here are a few I found reading through the commits.
- "Bug fixes and performance improvements" (aka I didn't keep track :D. Still true though)
- Added batch download feature
- Added dialog to view all errors in a operation
- Added max simultaneous download option
- Added playlist filtering
- Added playlist options. (Create playlist folder, ignore existing, reverse & number prefix)
- Added VERY silent update notification
- Added login for age-gated videos
- Added in-app updater for `youtube-dl`, since it can become outdated rather quick
- Added 'Open' & 'Copy' menu items for playlist list
- Faster playlist download
- Resume downloads
- Download specific Twitch VOD portions
- Crop and convert in same operation
- Removed WPF version
- Replaced queue list with [ObjectListView](http://objectlistview.sourceforge.net/cs/index.html). Queue should feel and look better now.

**2.0.0**

- Added a separate executable with for a WPF version of this program. It's more visually pleasing than Windows Forms, but it has the exact same functions as the "old" one. For now I'm just going to develop both along side each other, but I might choose one over the other in the future. 
- Added Folder converting. Convert all files in folder, with extension filtering
- Added a updater. It can download the newest version in-app, and shows this changelog
- Added Twitch VOD downloading. Unfortunately it's not always reliable and, unless it's been fixed recently, it's not something I can fix
- Added options to filter DASH/non-dash/normal formats for download
- Fixed canceling/pausing/resuming/removing operations. Everything works now during my limited testing
- Fixed logging errors that would close the program
- Fixed file access errors
- Fixed a lot of others errors aswell, but I haven't been writing it down since I didn't have a changelog before so that's all I can say

**1.0.0**

- Initial release
