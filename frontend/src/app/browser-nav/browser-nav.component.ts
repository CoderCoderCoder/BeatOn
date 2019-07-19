import { Component, OnInit, Output, EventEmitter } from '@angular/core';

@Component({
    selector: 'app-browser-nav',
    templateUrl: './browser-nav.component.html',
    styleUrls: ['./browser-nav.component.scss'],
})
export class BrowserNavComponent implements OnInit {
    constructor() {}
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

    ngOnInit() {}
}
