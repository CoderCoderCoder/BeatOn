export abstract class MessageBase {
    constructor() {
        this.MessageID = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
            var r = (Math.random() * 16) | 0,
                v = c == 'x' ? r : (r & 0x3) | 0x8;
            return v.toString(16);
        });
    }
    abstract Type: MessageType;
    MessageID: string;
    ResponseToMessageID: string;
}

export enum MessageType {
    SetupEvent = <any>'SetupEvent',
    Toast = <any>'Toast',
    DownloadStatus = <any>'DownloadStatus',
    ConfigChange = <any>'ConfigChange',
    DeleteSong = <any>'DeleteSong',
    MoveSongToPlaylist = <any>'MoveSongToPlaylist',
    DeletePlaylist = <any>'DeletePlaylist',
    AddOrUpdatePlaylist = <any>'AddOrUpdatePlaylist',
    OpStatus = <any>'OpStatus',
    GetOps = <any>'GetOps',
    SortPlaylist = <any>'SortPlaylist',
    AutoCreatePlaylists = <any>'AutoCreatePlaylists',
    SetModStatus = <any>'SetModStatus',
    ActionResponse = <any>'ActionResponse',
    MoveSongInPlaylist = <any>'MoveSongInPlaylist',
    MovePlaylist = <any>'MovePlaylist',
    DeleteMod = <any>'DeleteMod',
}
