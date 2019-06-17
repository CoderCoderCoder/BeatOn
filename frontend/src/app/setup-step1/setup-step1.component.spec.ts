import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SetupStep1Component } from './setup-step1.component';

describe('SetupStep1Component', () => {
  let component: SetupStep1Component;
  let fixture: ComponentFixture<SetupStep1Component>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ SetupStep1Component ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SetupStep1Component);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
