export interface HostSetupEvent
{
    SetupEvent : SetupEventType;
    Message : string;
}

export enum SetupEventType
{
    Step1Complete = "Step1Complete",
    Step2Complete = "Step2Complete",
    Step3Complete = "Step3Complete",
    Error = "Error",
    StatusMessage = "StatusMessage"
}