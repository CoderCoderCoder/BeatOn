import { MessageBase, MessageType } from './MessageBase';

export class ClientSetBeastSaberUsername extends MessageBase {
    constructor() {
        super();
    }
    readonly Type: MessageType = MessageType.SetBeastSaberUsername;
    BeastSaberUsername: string;
}
