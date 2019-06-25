import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { DownloadIndicatorComponent } from './download-indicator.component';

describe('DownloadIndicatorComponent', () => {
  let component: DownloadIndicatorComponent;
  let fixture: ComponentFixture<DownloadIndicatorComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ DownloadIndicatorComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(DownloadIndicatorComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
