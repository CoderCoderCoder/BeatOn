import {BeatSaberSong} from './BeatSaberSong'

export interface BeatSaberPlaylist
{
    CoverArtFilename : string;

    PlaylistID : string;

    PlaylistName : string;
    
    SongList : BeatSaberSong[];
}