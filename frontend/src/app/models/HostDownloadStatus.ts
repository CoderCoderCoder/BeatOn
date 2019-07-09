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
    NotStarted = <any>"NotStarted",
    Downloading = <any>"Downloading",
    Downloaded = <any>"Downloaded",
    Processing = <any>"Proccessing",
    Processed = <any>"Processed",
    Failed = <any>"Failed"
}