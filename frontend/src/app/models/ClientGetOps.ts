import { MessageBase, MessageType } from './MessageBase';

export class ClientGetOps extends MessageBase
{
    readonly Type : MessageType = MessageType.GetOps;
    ClearFailedOps : boolean;
}