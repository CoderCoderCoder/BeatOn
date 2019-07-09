import { MessageBase, MessageType } from './MessageBase';

export class ClientMovePlaylist extends MessageBase
{
    constructor() {
        super();
    }
    readonly Type : MessageType = MessageType.MovePlaylist;
    PlaylistID : string;
    Index : number;
}