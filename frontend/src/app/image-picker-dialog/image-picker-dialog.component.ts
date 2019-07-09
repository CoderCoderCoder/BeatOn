import { Component, OnInit, Injectable } from '@angular/core';
import { BeatOnApiService } from '../services/beat-on-api.service';
import { MatDialogRef } from '@angular/material';
import { AppSettings } from '../appSettings';
@Injectable({
    providedIn: 'root',
})
@Component({
    selector: 'app-image-picker-dialog',
    templateUrl: './image-picker-dialog.component.html',
    styleUrls: ['./image-picker-dialog.component.scss'],
})
export class ImagePickerDialogComponent implements OnInit {
    images = [];
    constructor(public dialogRef: MatDialogRef<ImagePickerDialogComponent>, private api: BeatOnApiService) {}

    ngOnInit() {
        this.api.getImages().subscribe((imgs: any) => {
            this.images = imgs;
        });
    }
    getImgUrl(image) {
        return 'url(' + AppSettings.API_ENDPOINT + '/host/mod/image?filename=' + encodeURIComponent(image) + ')';
    }
    pickImage(image) {
        this.api.getImageBlob(image).subscribe((imgblob: Blob) => {
            var reader = new FileReader();
            reader.onload = ev => {
                this.dialogRef.close(<string>reader.result);
            };
            reader.readAsDataURL(imgblob);
        });
    }
    clickCancel() {
        this.dialogRef.close(null);
    }
}
