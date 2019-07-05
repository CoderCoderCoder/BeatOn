import { MessageBase, MessageType } from './MessageBase';

export class ClientDeleteMod extends MessageBase {
    constructor() {
        super();
    }
    readonly Type: MessageType = MessageType.DeleteMod;
    ModID: string;
}
