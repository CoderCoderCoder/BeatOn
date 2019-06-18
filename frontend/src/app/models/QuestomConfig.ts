import { BeatSaberPlaylist } from './BeatSaberPlaylist';
import { BeatSaberColor } from './BeatSaberColor';

export interface QuestomConfig
{
    Playlists : BeatSaberPlaylist[];

    //Saber : SaberModel;

    LeftColor : BeatSaberColor;

    RightColor : BeatSaberColor;
}