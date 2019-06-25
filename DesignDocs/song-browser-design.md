# BeatOn Song Browser Design

Created by: Yuuki

Last updated: 6/24/2019

# Requirements

BeatOn will have several forms of downloading songs. 
This document will cover the several methods of download.

1. The top bar will have sections for song search, new maps,
top downloads, and all songs.
2. Songs will be in a list item
3. Songs will have options to download and/or add to certain playlist
4. Song items will have ratings and downloads
5. Song search bar will be at the top of the search page
6. Song items will have an icon for if they are already on the quest or not. 
7. Tab for songs will have a dropdown for selecting top, new, and all songs

# High Level Implementation 

## Song Lists

There will be three song lists to find songs in the browser. Each list
will pull data from the beatsaver API and be loaded into a list
item for display. There will be a download to quest button and
add to playlist button for each item. Each item will have the rating
for the song as well as the number of plays and downloads. 

## Song Search

The top bar will have a search tab. Clicking this tab loads a page
with a search bar at the top and will allows for the user to search
songs that are on beatsaver. Songs given in this result will have
download and add to playlist buttons. 