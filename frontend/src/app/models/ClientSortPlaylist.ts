import { MessageBase, MessageType } from './MessageBase';
import { PlaylistSortMode } from './PlaylistSortMode';

export class ClientSortPlaylist extends MessageBase
{
    readonly Type : MessageType = MessageType.SortPlaylist;
    PlaylistID : string;
    SortMode : PlaylistSortMode;
    Reverse : boolean;
}
