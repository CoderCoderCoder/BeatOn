import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { MainBrowserComponent } from './main-browser.component';

describe('MainBrowserComponent', () => {
  let component: MainBrowserComponent;
  let fixture: ComponentFixture<MainBrowserComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ MainBrowserComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(MainBrowserComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
