
export abstract class MessageBase
{
    abstract Type : MessageType;
}

export enum MessageType
{
    SetupEvent,
    Toast,
    DownloadStatus,
    ConfigChange,
    DeleteSong,
    MoveSongToPlaylist,
    DeletePlaylist,
    AddOrUpdatePlaylist,
    OpStatus,
    GetOps,
    SortPlaylist,
    AutoCreatePlaylists
}