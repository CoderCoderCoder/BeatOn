import { MessageBase, MessageType } from './MessageBase';
import { PlaylistSortMode } from './PlaylistSortMode';

export class ClientSortPlaylist extends MessageBase
{
    constructor() {
        super();
    }
    readonly Type : MessageType = MessageType.SortPlaylist;
    PlaylistID : string;
    SortMode : PlaylistSortMode;
    Reverse : boolean;
}
