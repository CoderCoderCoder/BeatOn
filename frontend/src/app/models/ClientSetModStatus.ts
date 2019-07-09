import { MessageBase, MessageType } from './MessageBase';
import { ModStatusType } from './ModDefinition';

export class ClientSetModStatus extends MessageBase
{

    readonly Type : MessageType = MessageType.SetModStatus;
    ModID : string;
    Status : ModStatusType;
    
}

