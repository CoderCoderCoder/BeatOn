import { MessageType, MessageBase } from './MessageBase';

/*Updates a specific feed configuration.  FeedConfig is the configuration data for the feed*/
export class ClientUpdateSyncFeedConfig extends MessageBase {
    Type: MessageType = MessageType.UpdateSyncFeedConfig;

    ID: string;

    FeedConfig: any;
}
