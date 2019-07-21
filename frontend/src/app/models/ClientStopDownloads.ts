import { MessageBase, MessageType } from './MessageBase';

export class ClientStopDownloads extends MessageBase {
    readonly Type: MessageType = MessageType.StopDownloads;
    DownloadsToStop: string[];
}
