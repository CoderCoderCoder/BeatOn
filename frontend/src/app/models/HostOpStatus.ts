export interface HostOpStatus
{
    Ops : HostOp[];
}

export interface HostOp
{
    ID : number;
    OpDescription : string;
    Status : OpStatus;
    Error: string;
}

export enum OpStatus
{
    Queued = <any>"Queued",
    Started = <any>"Started",
    Complete = <any>"Complete",
    Failed = <any>"Failed"
}