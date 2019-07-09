import { BeatSaberPlaylist } from './BeatSaberPlaylist';
import { BeatSaberColor } from './BeatSaberColor';
import { ModDefinition } from './ModDefinition';

export interface QuestomConfig
{
    Playlists : BeatSaberPlaylist[];

    Mods : ModDefinition[];

    //Saber : SaberModel;

    LeftColor : BeatSaberColor;

    RightColor : BeatSaberColor;
}