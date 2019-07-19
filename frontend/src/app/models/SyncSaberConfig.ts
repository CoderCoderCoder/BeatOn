import { FeedReader } from './FeedReader';

export interface SyncSaberConfig {
    BeastSaberUsername: string;
    CheckExistingSongsUpdated: boolean;
    IsSyncInProgress: boolean;
    FeedReaders: FeedReader[];
}
