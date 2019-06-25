# BeatOn Playlist Design

Created by: Yuuki

Last updated: 6/24/2019

# Requirements

The BeatOn playlist page should meet the following requirements:

1. User can create new playlists
2. User can change playlist cover
3. User can change playlist names
4. User can add and delete songs based on BeatSaber song list
5. Playlists will be selectable from a list across the top of the page (like Google search)
6. Song items will appear as a grid with the cover image an a x button in the top right
7. Song items can be dragged onto another playlist to be added to that playlist
8. Clicking a song item will show a song details page that will show up a right side panel
9. Song details page will have an option to delete songs and show song ratings

## High Level Implementation

### Playlist Top Bar

The top of the playlist page will hold a scrollable bar containing
the cover image of the playlist. Each playlist can be clicked on to
show the songs in the playlist. The user should be able to click and drag
to scroll through their playlists, as well as click on arrows on
either side of the bar. 

There should be a plus item in the end of the list that will allow 
the user to create a new playlist. This will pop up the create playlist 
dialog.

#### Creating Playlists

Creating playlists can be done from the playlist top bar and other
locations. When creating a playlist, a modal will pop up that
will ask the user to upload a playlist cover image and provide a
name. 

#### Exporting/Saving Playlists

Playlists should be exportable via an export button that will
appear when the user clicks a playlist cover in the top bar.
The Export Playlist button will appear below the playlist top bar
along with the save playlist button. Exporting of playlists will
export the playlist in the `.bsplist` format.

### Song Items

Song items will appear in a grid under their respective playlists.
Each item will have an x in the top right for delete the song from
the playlist and quest. Dragging one of these items to a playlist
cover image will move that song item to that playlist.

#### Song Item Details

When a song item is clicked, its details will be displayed on the
right via a sidebar pop up component. The component should have
the following items:

* Song name
* Song rating from bsaber
* Link to song
* Delete button


