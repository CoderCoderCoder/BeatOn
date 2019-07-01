import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { BrowserNavComponent } from './browser-nav.component';

describe('BrowserNavComponent', () => {
  let component: BrowserNavComponent;
  let fixture: ComponentFixture<BrowserNavComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ BrowserNavComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(BrowserNavComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
