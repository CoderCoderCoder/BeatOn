# BeatOn Mod Management Design

Created by: Yuuki

Last updated: 6/24/2019

# Requirements

1. User can manage mods
2. User can install/uninstall mods
3. User can change saber color values

## High Level Implementation

### Mod Management

The mod management page will have a list of available mods to install
on the quest. This list will keep up with what is currently installed
on the quest. The user can install and uninstall mods from this list.
Clicking on a mod that requires additional configuration will open
a side popup component for configuring the mod (such as changing colors)