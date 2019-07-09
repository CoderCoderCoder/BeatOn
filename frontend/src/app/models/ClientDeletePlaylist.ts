import { MessageBase, MessageType } from './MessageBase';

export class ClientDeletePlaylist extends MessageBase
{
    constructor() {
        super();
    }
    readonly Type : MessageType = MessageType.DeletePlaylist;
    PlaylistID : string;
}
