import { MessageBase, MessageType } from './MessageBase';
import { BeatSaberColor } from './BeatSaberColor';

export class ClientChangeColor extends MessageBase {
    constructor() {
        super();
    }
    readonly Type: MessageType = MessageType.ChangeColor;
    Color: BeatSaberColor;

    ColorType: ColorType;
}
export enum ColorType {
    LeftColor = <any>'LeftColor',
    RightColor = <any>'RightColor',
}
