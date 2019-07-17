import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SyncSaberMaxSongsComponent } from './sync-saber-max-songs.component';

describe('SyncSaberMaxSongsComponent', () => {
    let component: SyncSaberMaxSongsComponent;
    let fixture: ComponentFixture<SyncSaberMaxSongsComponent>;

    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [SyncSaberMaxSongsComponent],
        }).compileComponents();
    }));

    beforeEach(() => {
        fixture = TestBed.createComponent(SyncSaberMaxSongsComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });
});
