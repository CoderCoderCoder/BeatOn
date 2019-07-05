export class AppButtonEvent {
    button: AppButtonType;
    x: number;
    y: number;
}

export enum AppButtonType {
    Up = <any>'Up',
    Down = <any>'Down',
    Left = <any>'Left',
    Right = <any>'Right',
    Center = <any>'Center',
}
