import { MessageBase, MessageType } from './MessageBase';
import { FeedReader } from './FeedReader';

export class ClientUpdateFeedReader extends MessageBase {
    constructor() {
        super();
    }
    readonly Type: MessageType = MessageType.UpdateFeedReader;

    ID: string;
    FeedConfig: FeedReader;
}
