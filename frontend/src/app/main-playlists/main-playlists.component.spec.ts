import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { MainPlaylistsComponent } from './main-playlists.component';

describe('MainPlaylistsComponent', () => {
  let component: MainPlaylistsComponent;
  let fixture: ComponentFixture<MainPlaylistsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ MainPlaylistsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(MainPlaylistsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
