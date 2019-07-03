import { MessageBase, MessageType } from './MessageBase';

export class ClientDeleteSong extends MessageBase
{
    constructor() {
        super();
    }
    readonly Type : MessageType = MessageType.DeleteSong;
    SongID : string;
}