import { MessageBase, MessageType } from "./MessageBase";
import { PlaylistSortMode } from './PlaylistSortMode';

export class ClientAutoCreatePlaylists extends MessageBase
{
    constructor() {
        super();
    }
    readonly Type : MessageType = MessageType.AutoCreatePlaylists;
    SortMode : PlaylistSortMode;
    MaxPerNamePlaylist : number;
}