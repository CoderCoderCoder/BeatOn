import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { MatDialog } from '@angular/material';
import { InputBoxComponent } from '../input-box/input-box.component';

@Component({
    selector: 'app-browser-nav',
    templateUrl: './browser-nav.component.html',
    styleUrls: ['./browser-nav.component.scss'],
})
export class BrowserNavComponent implements OnInit {
    constructor(private dialog: MatDialog) {}
    browserLinks: any[] = [
        { title: 'Beast Saber', url: 'https://bsaber.com' },
        { title: 'Beat Saver', url: 'https://beatsaver.com' },
        { title: 'Google', url: 'https://google.com' },
        { title: 'Mods', url: 'https://github.com/RedBrumbler/BeatOnCustomSabers' },
    ];

    @Output() linkSelected = new EventEmitter<string>();

    clickLink(link) {
        this.linkSelected.emit(link.url);
    }
    clickUrl() {
        const dialogRef = this.dialog.open(InputBoxComponent, {
            width: '600px',
            height: '450px',
            disableClose: true,
            data: {
                label: 'URL',
                key: 'browserUrl',
            },
        });
        dialogRef.afterClosed().subscribe(res => {
            if (res != null) {
                if (res.indexOf('://') < 1) res = 'http://' + res;
                this.linkSelected.emit(res);
            }
        });
    }
    ngOnInit() {}
}
