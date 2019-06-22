import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { RemoteUploadComponent } from './remote-upload.component';

describe('RemoteUploadComponent', () => {
  let component: RemoteUploadComponent;
  let fixture: ComponentFixture<RemoteUploadComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ RemoteUploadComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(RemoteUploadComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
