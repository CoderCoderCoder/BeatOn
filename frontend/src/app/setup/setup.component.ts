import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { BeatOnApiService } from '../services/beat-on-api.service';
import { HostMessageService } from '../services/host-message.service';
import { HostSetupEvent, SetupEventType } from '../models/HostSetupEvent';

@Component({
  selector: 'app-setup',
  templateUrl: './setup.component.html',
  styleUrls: ['./setup.component.scss']
})
export class SetupComponent implements OnInit {

  constructor(private beatOnApi: BeatOnApiService, private msgSvc: HostMessageService, private router : Router ) { }

  ngOnInit() {
    this.msgSvc.setupMessage.subscribe((msg : HostSetupEvent) =>
    {
      switch (msg.SetupEvent)
      {
        case SetupEventType.Step1Complete:
          this.router.navigateByUrl("/setupstep2");
          break;
        case SetupEventType.Step2Complete:
          this.router.navigateByUrl('/setupstep3');
          break;
        case SetupEventType.Step3Complete:
          this.router.navigateByUrl('/');
          break;
      }
    })
  }

}
