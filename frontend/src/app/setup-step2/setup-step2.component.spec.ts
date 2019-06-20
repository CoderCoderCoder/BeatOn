import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SetupStep2Component } from './setup-step2.component';

describe('SetupStep2Component', () => {
  let component: SetupStep2Component;
  let fixture: ComponentFixture<SetupStep2Component>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ SetupStep2Component ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SetupStep2Component);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
