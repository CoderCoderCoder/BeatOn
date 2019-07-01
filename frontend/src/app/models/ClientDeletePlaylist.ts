import { MessageBase, MessageType } from './MessageBase';

export class ClientDeletePlaylist extends MessageBase
{
    readonly Type : MessageType = MessageType.DeletePlaylist;
    PlaylistID : string;
}
