import { QuestomConfig } from './QuestomConfig';
import { SyncSaberConfig } from './SyncSaberConfig';

export interface BeatOnConfig {
    Config: QuestomConfig;
    IsCommitted: boolean;
    SyncConfig: SyncSaberConfig;
}
