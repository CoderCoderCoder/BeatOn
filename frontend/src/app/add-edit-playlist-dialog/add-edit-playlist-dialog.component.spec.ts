import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AddEditPlaylistDialogComponent } from './add-edit-playlist-dialog.component';

describe('AddEditPlaylistDialogComponent', () => {
  let component: AddEditPlaylistDialogComponent;
  let fixture: ComponentFixture<AddEditPlaylistDialogComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AddEditPlaylistDialogComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AddEditPlaylistDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
