import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { PlaylistSongsComponent } from './playlist-songs.component';

describe('PlaylistSongsComponent', () => {
  let component: PlaylistSongsComponent;
  let fixture: ComponentFixture<PlaylistSongsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ PlaylistSongsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(PlaylistSongsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
