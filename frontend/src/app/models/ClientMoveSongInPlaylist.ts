import { MessageBase, MessageType } from './MessageBase';

export class ClientMoveSongInPlaylist extends MessageBase
{
    constructor() {
        super();
    }
    readonly Type : MessageType = MessageType.MoveSongInPlaylist;
    SongID : string;
    //Currently not used server side
    PlaylistID: string;
    Index : number;
}