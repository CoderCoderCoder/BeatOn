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

  onFileChange(event) {
    if (event.target.files.length > 0) {
      const file = event.target.files[0];
      this.form.get('fileUpload').setValue(file);
    }
  }

  onSubmit() {
    const formData = new FormData();
    formData.append('file', this.form.get('fileUpload').value);
    this.beatOnApi.uploadFile(formData).subscribe(
      (res) => {
        console.log("File uploaded")
      },
      (err) => {
        console.log("Failed to upload: " + err);
      }
    );
  }

  files: any = [];

  uploadFile(event) {
    for (let index = 0; index < event.length; index++) {
      const file = event[index];
      console.log(file)
      this.files.push(file.name)
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
  deleteAttachment(index) {
    this.files.splice(index, 1)
  }
}
