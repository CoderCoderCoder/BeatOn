import { FeedReader } from './FeedReader';

export interface SyncSaberConfig {
    BeastSaberUsername: string;
    CheckExistingSongsUpdated: boolean;
    FeedReaders: FeedReader[];
}
