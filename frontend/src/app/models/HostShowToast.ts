export interface HostShowToast
{
    ToastType : ToastType;

    Timeout : number;

    Title : string;

    Message : string;
}

export enum ToastType
{
    Error = "Error",
    Warning = "Warning",
    Info = "Info",
    Success = "Success"
}