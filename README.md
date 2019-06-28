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

* elliotttate for tons of stuff... getting me started on this whole project even

* Lots of people on the Beat Saber Modding Group discord (discord.gg/beatsabermods), especially other devs who are jumping in

  * Yuuki (https://github.com/brandonhenry)
  
  * karldeux

* Thanks to those who helped test as well, specifically those who have given really good, detailed feedback:

  * Agent42
  
  * diomark
  
### Libraries Used

* jakibaki's beatsaber-hook (https://github.com/jakibaki/beatsaber-hook)

* dexlib2 (https://github.com/JesusFreke/smali/)

* The assets modification library I've been working on for assets file modifications (https://github.com/emulamer/QuestomAssets)

* Xamarin and Embeddinator-4000 (https://github.com/mono/Embeddinator-4000) for bridging .NET to Java

* ... and various other nuget packages
 
### Building BeatOn

1. Make sure you have these things:

 * Visual Studio (I used VS 2019)
 
 * Xamarin stuff installed for Visual Studio
 
 * JDK (I used OpenJDK 8)
 
 * Android SDK (for 7.1 which I think is API 25, or maybe 9.0 which is 29.  I'm not sure which it's actually using.)
 
 * Android NDK (I used android-ndk-r15c-windows-x86)
 
 * For building the front end, Node.js, npm and angular-cli
 
 * The JARs for the java projects are included, but if you want to build the Java helper lib it's an intellij project.
  

2. `git clone https://github.com/emulamer/BeatOn`

3. Do a `git submodule update --init --recursive`

4. Open BeatOn.sln and build it.

### Building the front end

(haven't tested all of these steps yet)

1. go to the frontend directory

2. npm install

3. ng build --prod   (this will dump the build output into the BeatOn project's assets\www folder.

4. The output filenames change with a build, so you'll need to open the BeatOn project, remove the old files and add the new ones, then rebuild BeatOn


### Debugging/developing the front end

1. Run BeatOn on the Quest, either debug through VS or start the app on the quest.  Make sure the quest doesn't go to sleep or the app will pause.

2. With the app running on the quest, edit the appSettings.ts file in the front end and change the "DEV_API_ENDPOINT" variable to "http://<ip of your quest>:50000" and set DEV_WS_ENDPOINT to null (it'll be pulled from the web server automatically)

3. run ng serve and the page should be available at http://localhost:4200

4. I think I don't have things configured right for the built in server, sub-routes don't load, so after a page refresh you'll have to go back to the root URL.


### Testing in an emulator

Some parts of Beat On will only work on a Quest, e.g. the initial setup/mod process.  The post-setup workflows will mostly work on the emulator with the assets modification, web host, etc.
To set up your emulator:

* You'll need the asset files from the /assets/bin/Data/ folder out of the extracted Beat Saber APK from the quest (version 1.1.0 at the moment)

* Pick an android TV device profile, I've tested with the 720p one.  I also bumped the ram to 2GB, internal storage to a GB and the SD card size to 4GB just to avoid running out of space.

* To save yourself headache, make sure your xamarin/emulator stuff is using the same version of ADB that you're running from the command line or they'll keep killing each other.

* After the emulator is up, make an /sdcard/BeatOnData folder, and push the assets from the APK out of /assets/bin/Data/** to /sdcard/BeatOnData/BeatSaberAssets/

This is what my batch file for resetting the emulator looks like (kills any processes, clears data, uninstalls the app, pushes up fresh data):
```
adb shell am force-stop com.emulamer.beaton
adb shell am force-stop com.emulamer.beatonservice
adb -s emulator-5554 shell rm -rf /sdcard/BeatOnData
adb -s emulator-5554 shell mkdir /sdcard/BeatOnData
adb -s emulator-5554 shell mkdir /sdcard/BeatOnData/CustomSongs
adb -s emulator-5554 push <path to extracted APK>\assets\bin\Data /sdcard/BeatOnData/BeatSaberAssets
adb uninstall com.emulamer.beaton
```
* Change the solution configuration in visual studio to "Emulator Debug".  This skips most of the "is the mod installed" checks and loads directly into the main UI.

* If you want to debug the front end from the dev machine:

  * Since the emulators don't allow direct network connections, you need to have ADB forward ports from your local machine, e.g.:
```
adb forward tcp:51000 tcp:50000
adb forward tcp:51001 tcp:50001
```
  * Edit the appSettings.ts file to point towards the forwarded ports, e.g. set the DEV_API_ENDPOINT variable to "http://localhost:51000" and set DEV_WS_ENDPOINT to "ws://localhost:51001"
  