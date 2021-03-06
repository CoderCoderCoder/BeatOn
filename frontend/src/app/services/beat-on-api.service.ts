import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AppSettings } from '../appSettings';
import { Observable } from 'rxjs';
import { NetInfo } from '../models/NetInfo';
import { QuestomConfig } from '../models/QuestomConfig';
import { PlaylistSortMode } from '../models/PlaylistSortMode';

@Injectable({
    providedIn: 'root',
})
export class BeatOnApiService {
    hostname: string = AppSettings.API_ENDPOINT;
    constructor(private http: HttpClient) {}

    getModStatus() {
        return this.http.get(this.hostname + '/host/mod/status');
    }

    installModStep1() {
        return this.http.post(this.hostname + '/host/mod/install/step1', '');
    }

    installModStep2() {
        return this.http.post(this.hostname + '/host/mod/install/step2', '');
    }

    installModStep3() {
        return this.http.post(this.hostname + '/host/mod/install/step3', '');
    }

    resetAssets() {
        return this.http.post(this.hostname + '/host/mod/resetassets', '');
    }

    uninstallBeatSaber() {
        return this.http.post(this.hostname + '/host/mod/uninstallbeatsaber', '');
    }

    getNetInfo(): Observable<NetInfo> {
        return <Observable<NetInfo>>this.http.get(this.hostname + '/host/mod/netinfo');
    }

    getConfig(): Observable<QuestomConfig> {
        return <Observable<QuestomConfig>>this.http.get(this.hostname + '/host/beatsaber/config');
    }

    uploadFile(fileData: any) {
        return this.http.post(this.hostname + '/host/beatsaber/upload', fileData);
    }

    commitConfig() {
        return this.http.post(this.hostname + '/host/beatsaber/commitconfig', '');
    }

    reloadSongsFromFolders() {
        return this.http.post(this.hostname + '/host/beatsaber/reloadsongfolders', '');
    }

    putConfig(config: QuestomConfig) {
        return this.http.put(this.hostname + '/host/beatsaber/config', config);
    }

    autoCreatePlaylists(sortMode: PlaylistSortMode, maxNumPerNamePlaylist: number) {
        return this.http.post(
            this.hostname +
                '/host/beatsaber/playlist/autocreate?sortorder=' +
                PlaylistSortMode[sortMode] +
                'maxnumpernameplaylist=' +
                maxNumPerNamePlaylist,
            {}
        );
    }

    restoreCommittedConfig() {
        return this.http.post(this.hostname + '/host/beatsaber/config/restore?configtype=committed', {});
    }

    revertConfig() {
        return this.http.delete(this.hostname + '/host/beatsaber/config');
    }

    postLogs(tag) {
        return this.http.post(this.hostname + '/host/mod/postlogs?tag=' + encodeURIComponent(tag), {});
    }

    getImageBlob(filename) {
        return this.http.get(this.hostname + '/host/mod/image?filename=' + encodeURIComponent(filename), { responseType: 'blob' });
    }

    getImages() {
        return this.http.get(this.hostname + '/host/mod/images');
    }
    startBeatSaber() {
        return this.http.post(this.hostname + '/host/mod/package?action=start&package=com.beatgames.beatsaber', {});
    }
    quitBeatOn() {
        return this.http.post(this.hostname + '/host/mod/exit', {});
    }
}
