import { Component, OnInit } from '@angular/core';
import { ProgressSpinnerDialogComponent } from '../progress-spinner-dialog/progress-spinner-dialog.component';
import { MatDialog, MatDialogRef } from '@angular/material';
import { BeatOnApiService } from '../services/beat-on-api.service';
import { HostShowToast, ToastType } from '../models/HostShowToast';
import { NetInfo } from '../models/NetInfo';
import { Router } from '@angular/router';
import { ModSetupStatus } from '../models/ModSetupStatus';
import { ConfirmDialogComponent } from '../confirm-dialog/confirm-dialog.component';

@Component({
    selector: 'app-tools',
    templateUrl: './tools.component.html',
    styleUrls: ['./tools.component.scss'],
})
export class ToolsComponent implements OnInit {
    constructor(private beatOnApi: BeatOnApiService, private dialog: MatDialog, private router: Router) {}
    netInfo: NetInfo;
    clickUninstallBeatSaber() {
        const dialogRef = this.dialog.open(ProgressSpinnerDialogComponent, {
            width: '450px',
            height: '350px',
            disableClose: true,
            data: { mainText: 'Please wait...' },
        });
        this.beatOnApi.uninstallBeatSaber().subscribe(
            (data: any) => {
                dialogRef.close();
                this.router.navigateByUrl('/');
            },
            err => {
                dialogRef.close();
            }
        );
    }

    clickResetAssets() {
        const dialogRef = this.dialog.open(ProgressSpinnerDialogComponent, {
            width: '450px',
            height: '350px',
            disableClose: true,
            data: { mainText: 'Please wait...' },
        });
        this.beatOnApi.resetAssets().subscribe(
            (data: any) => {
                dialogRef.close();
                window.dispatchEvent(
                    new MessageEvent('host-message', {
                        data: <HostShowToast>{
                            ToastType: ToastType.Info,
                            Timeout: 3000,
                            Title: 'Assets reset.',
                            Message: '',
                        },
                    })
                );
            },
            err => {
                dialogRef.close();
                window.dispatchEvent(
                    new MessageEvent('host-message', {
                        data: <HostShowToast>{
                            ToastType: ToastType.Error,
                            Timeout: 8000,
                            Title: 'Error resetting assets!',
                            Message: err,
                        },
                    })
                );
            }
        );
    }
    clickQuickFix() {
        this.beatOnApi.postLogs('unknown').subscribe((data: any) => {}, err => {});
        const dialogRef = this.dialog.open(ProgressSpinnerDialogComponent, {
            width: '450px',
            height: '350px',
            disableClose: true,
            data: { mainText: 'Resetting assets...' },
        });
        this.beatOnApi.resetAssets().subscribe(
            (data: any) => {
                dialogRef.close();
                const dialogRef2 = this.dialog.open(ProgressSpinnerDialogComponent, {
                    width: '450px',
                    height: '350px',
                    disableClose: true,
                    data: { mainText: 'Loading Songs Folder.  Please wait...' },
                });
                this.beatOnApi.reloadSongsFromFolders().subscribe(
                    (data: any) => {
                        dialogRef2.close();
                        this.beatOnApi.restoreCommittedConfig().subscribe(ret => {});
                    },
                    err => {
                        dialogRef2.close();
                    }
                );
            },
            err => {
                dialogRef.close();
            }
        );
    }
    clickQuit() {
        this.beatOnApi.quitBeatOn().subscribe(ret => {});
    }
    clickLoadLastConfig() {
        this.beatOnApi.restoreCommittedConfig().subscribe(ret => {});
    }
    clickUploadLogs() {
        const dialogRef = this.dialog.open(ProgressSpinnerDialogComponent, {
            width: '450px',
            height: '350px',
            disableClose: true,
            data: { mainText: 'Please wait...' },
        });
        this.beatOnApi.postLogs('unknown').subscribe(
            (data: any) => {
                dialogRef.close();
            },
            err => {
                dialogRef.close();
            }
        );
    }

    clickReloadSongsFolder() {
        const dialogRef = this.dialog.open(ProgressSpinnerDialogComponent, {
            width: '450px',
            height: '350px',
            disableClose: true,
            data: { mainText: 'Loading Songs Folder.  Please wait...' },
        });
        this.beatOnApi.reloadSongsFromFolders().subscribe(
            (data: any) => {
                dialogRef.close();
            },
            err => {
                dialogRef.close();
                window.dispatchEvent(
                    new MessageEvent('host-message', {
                        data: <HostShowToast>{
                            ToastType: ToastType.Error,
                            Timeout: 8000,
                            Title: 'Error reloading songs folder!',
                            Message: err,
                        },
                    })
                );
            }
        );
    }
    getBackupStatus() {
        if (!this.modStatus) return 'unknown';

        if (this.modStatus.HasGoodBackup) return 'Good';
        else if (this.modStatus.HasHalfAssBackup) return 'Not ideal';
        else return "NO BACKUP!  Mods won't uninstall!";
    }
    getVersion() {
        if (!this.modStatus || !this.modStatus.BeatOnVersion) return 'unknown';

        return this.modStatus.BeatOnVersion;
    }
    modStatus: ModSetupStatus;
    ngOnInit() {
        this.beatOnApi.getNetInfo().subscribe((ni: NetInfo) => {
            this.netInfo = ni;
        });
        this.beatOnApi.getModStatus().subscribe((ms: ModSetupStatus) => {
            this.modStatus = ms;
        });
    }
}
