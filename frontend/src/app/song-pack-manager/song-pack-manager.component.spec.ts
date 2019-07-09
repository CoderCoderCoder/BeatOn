import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SongPackManagerComponent } from './song-pack-manager.component';

describe('SongPackManagerComponent', () => {
  let component: SongPackManagerComponent;
  let fixture: ComponentFixture<SongPackManagerComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ SongPackManagerComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SongPackManagerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
