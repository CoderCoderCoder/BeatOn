import { BeatSaberPlaylist } from '../models/BeatSaberPlaylist';
import { MessageBase, MessageType } from './MessageBase';

export class ClientAddOrUpdatePlaylist extends MessageBase
{
    constructor() {
        super();
    }
    readonly Type : MessageType = MessageType.AddOrUpdatePlaylist;

    Playlist : BeatSaberPlaylist;
}

