export interface ModSetupStatus {
    CurrentStatus: ModSetupStatusType;
    IsBeatSaberInstalled: boolean;
    BeatOnVersion: string;
    HasGoodBackup: boolean;
    HasHalfAssBackup: boolean;
}
export enum ModSetupStatusType {
    ModInstallNotStarted = <any>'ModInstallNotStarted',
    ReadyForModApply = <any>'ReadyForModApply',
    ReadyForInstall = <any>'ReadyForInstall',
    ModInstalled = <any>'ModInstalled',
}
