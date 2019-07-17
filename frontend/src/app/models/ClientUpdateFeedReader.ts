import { MessageBase, MessageType } from './MessageBase';

export class ClientUpdateFeedReader extends MessageBase {
    constructor() {
        super();
    }
    readonly Type: MessageType = MessageType.UpdateFeedReader;
    MaxSongs: number;
    ID: string;
    IsEnabled: boolean;
}
