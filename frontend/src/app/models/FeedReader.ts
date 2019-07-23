export interface FeedReader {
    DisplayName: string;
    FeedType: number;
    ID: string;
    IsEnabled: boolean;
    LastSyncAttempt: string;
    LastSyncSuccess: string;
    PlaylistID: string;
    MaxSongs: number;
    Authors: string[];
}
