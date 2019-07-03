import { MessageBase, MessageType } from './MessageBase';

export class ClientGetOps extends MessageBase
{
    constructor() {
        super();
    }
    readonly Type : MessageType = MessageType.GetOps;
    ClearFailedOps : boolean;
}