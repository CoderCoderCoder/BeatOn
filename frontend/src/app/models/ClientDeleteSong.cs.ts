import { MessageBase, MessageType } from './MessageBase';

export class ClientDeleteSong extends MessageBase
{
    readonly Type : MessageType = MessageType.DeleteSong;
    SongID : string;
}