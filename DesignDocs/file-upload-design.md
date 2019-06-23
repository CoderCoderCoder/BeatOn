# BeatOn File Upload Design Document
Created by: Yuuki

Last Edited: 6/23/2019

## Requirements
File upload should meet the following requirements:

1. User can drag zip files to file upload page
2. User can select a zip file from their file system to upload
3. User can upload multiple files at once
4. User can upload song folders
5. User can upload beat saber playlists
5. User should be able to see songs that are in the sync queue
6. User should be able to remove songs from the sync queue

## High Level Implementation

### Drag-N-Drop

Dragging a zip file, folder, or beat saber playlist should be supported. 

Dragging a zip file will immediately cause a download of the file and notify the user of that download. The download manager should handle the status of this download. Once the download completes, there should be a list of these items pending sync to the Quest. The list should allow for the user to remove items to be synced.

Dragging a folder will cause the folder to be zipped and then sent to be downloaded. Remaining functionality will work similar to dragging a zip file.

Dragging a playlist will load a popup modal with the list of songs contained in the playlist. The user should be able to select which songs they want to download. A select all and deselect all option should be available as well. When dragging a playlist, the user should have the option to choose to add that playlist as a new playlist on BeatSaber. Otherwise, they can add the songs to a playlist already created.

### Sync Queue

The sync queue will hold all songs waiting to be synced to the quest. From this list, the user will have the option to not sync some or all of the songs.

## Low Level Implementation

### File Upload

#### Normal File Upload

The upload page will contain an area to drag files, but it should also allow normal file upload. Clicking on an upload icon will cause a file system dialog to pop up and let the user select one or multiple files to upload. Folder upload will not be supported in this dialog.

#### Drag-N-Drop Upload

The user will be able to drag zip files, folders, and beat saber playlists to the Drag-N-Drop interface. The base functionality will use a directive containing listener events to when the user drags files into the drop area.

Files will be converted to form data and sent to the `"/host/beatsaber/upload"` route.

Using the JSZip package, folders dragged into the area will be converted to a zip file, converted to form data and sent to the api.

Beat Saber playlists will be parsed out. Requests will be sent to the beatsaver API and information on the songs in the playlist will be presented. Once the user selects the songs they want from the playlist, these songs will be sent to the beatsaver API, downloaded, and sent to the upload route. 