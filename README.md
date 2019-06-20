# Beat On!

**Quest Beat Saber song and mod loader and manager, running on the Quest itself.**


### General idea

From a workflow standpoint, I hope to be able to accomplish something like this:

* Using SideQuest (or manually, if user is a barbarian), sideload the Beat On app to the quest from a computer.

* Opening Beat On for the first time:

     1. You'll see a prompt something to the effect of "It looks like Beat Saber is installed, but it hasn't been unlocked for mods yet.  Do you want to get your beat on?"

     2. Saying yes, you'll get some kind of screen telling you what to expect up front, and a "go do it" button

     3. You'll get a progress/status bar, and eventually you'll be prompted to uninstall Beat Saber

     4. After uninstalling, progress/status will continue until you get another prompt to install the modded Beat Saber

     5. After installing, it'll detect that it's ready for mods
 
* After it's prepped for mods, Beat On will open to some sort of dashboard containing the things it can do:

   * Custom songs

   * Manage custom playlists

   * Custom sabers

   * Other settings? (Custom colors, text overrides, etc.)

   * ...anything else found to be moddable one way or the other, maybe a "other mods" for things that can be resource patched in things like custom sabers, custom songs, colors, text, etc.

   * Hopefully leaving it up to somebody with good UI sense how this should be organized

   * Be able to download songs from bsaber or somewhere and add them to playlists in Beat Saber

   * Be able download other mods from wherever and add them to Beat Saber

   * Have various recovery options in case things go wrong

     * If mod install is interrupted

     * Repair if assets become corrupted somehow

     * Detect and re-mod if Beat Saber has upgraded, and Beat On can handle the new version without an update (or maybe some way of handling version definitions that can be downloaded on the fly when only parameters need changing and not code?)

   * TBD...

### Credits and Contributions

* Sc2ad (https://github.com/sc2ad)

* jakibaki (https://github.com/jakibaki/beatsaber-hook)

* Lots of people on the Beat Saber Modding Group discord (discord.gg/beatsabermods)

### Libraries Used

* jakibaki's beatsaber-hook (https://github.com/jakibaki/beatsaber-hook)

* dexlib2 (https://github.com/JesusFreke/smali/)

* The assets modification library I've been working on for assets file modifications (https://github.com/emulamer/QuestomAssets)

* Xamarin and Embeddinator-4000 (https://github.com/mono/Embeddinator-4000) for bridging .NET to Java

* ... and various other nuget packages
 
### Building BeatOnLib

1. Make sure you have these things:

 * Visual Studio
 
 * Xamarin stuff installed for Visual Studio
 
 * JDK (I used OpenJDK 8)
 
 * Android SDK (for 7.1 which I think is API 25, or maybe 9.0 which is 29.  I'm not sure which it's actually using.)
 
 * Android NDK (I used android-ndk-r15c-windows-x86)

2. `git clone https://github.com/emulamer/BeatOn`

3. Do a `git submodule update --init --recursive`

