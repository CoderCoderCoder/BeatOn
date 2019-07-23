import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SyncSaberComponent } from './sync-saber.component';

describe('SyncSaberComponent', () => {
    let component: SyncSaberComponent;
    let fixture: ComponentFixture<SyncSaberComponent>;

    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [SyncSaberComponent],
        }).compileComponents();
    }));

    beforeEach(() => {
        fixture = TestBed.createComponent(SyncSaberComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });
});
