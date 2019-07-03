import { MessageBase, MessageType } from './MessageBase';

export class ClientMoveSongToPlaylist extends MessageBase
{
    constructor() {
        super();
    }
    readonly Type : MessageType = MessageType.MoveSongToPlaylist;
    SongID : string;
    ToPlaylistID : string;
}