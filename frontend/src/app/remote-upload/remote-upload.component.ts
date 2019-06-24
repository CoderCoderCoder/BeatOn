import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from  '@angular/forms';
import { BeatOnApiService } from '../services/beat-on-api.service';

@Component({
  selector: 'app-remote-upload',
  templateUrl: './remote-upload.component.html',
  styleUrls: ['./remote-upload.component.scss']
})
export class RemoteUploadComponent implements OnInit {

  constructor(private formBuilder: FormBuilder, private beatOnApi : BeatOnApiService) { }
  form: FormGroup;

  ngOnInit() {
    this.form = this.formBuilder.group({
      fileUpload: ['']
    });
  }

  uploadFile(event) {
    for (let index = 0; index < event.length; index++) {
      const file = event[index];
      const formData = new FormData();
      formData.append('file', file);
      this.beatOnApi.uploadFile(formData).subscribe(
        (res) => {
          console.log("File uploaded")
        },
        (err) => {
          console.log("Failed to upload: " + err);
        });
    }
  }
}
