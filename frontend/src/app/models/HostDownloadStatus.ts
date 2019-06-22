export interface HostDownloadStatus
{
    Downloads : HostDownload[];
}

export interface HostDownload
{
    ID : string;
    Url : string;
    Status : HostDownloadStatusType;
    PercentageComplete : number;  
}

export enum HostDownloadStatusType
{
    NotStarted,
    Downloading,
    Downloaded,
    Installing,
    Installed,
    Failed
}