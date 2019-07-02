import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { MainModsComponent } from './main-mods.component';

describe('MainModsComponent', () => {
  let component: MainModsComponent;
  let fixture: ComponentFixture<MainModsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ MainModsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(MainModsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
