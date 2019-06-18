import { Component, OnInit } from '@angular/core';
import { BeatOnApiService } from '../services/beat-on-api.service';
import { HostMessageService } from '../services/host-message.service';

@Component({
  selector: 'app-setup',
  templateUrl: './setup.component.html',
  styleUrls: ['./setup.component.scss']
})
export class SetupComponent implements OnInit {

  constructor(private beatOnApi: BeatOnApiService, private msgSvc: HostMessageService) { }

  ngOnInit() {
    this.msgSvc.setupMessage.subscribe((msg) =>
    {
      
    })
  }

}
