import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SetupStep3Component } from './setup-step3.component';

describe('SetupStep3Component', () => {
  let component: SetupStep3Component;
  let fixture: ComponentFixture<SetupStep3Component>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ SetupStep3Component ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SetupStep3Component);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
