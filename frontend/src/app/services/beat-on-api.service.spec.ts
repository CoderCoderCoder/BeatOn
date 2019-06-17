import { TestBed } from '@angular/core/testing';

import { BeatOnApiService } from './beat-on-api.service';

describe('BeatOnApiService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: BeatOnApiService = TestBed.get(BeatOnApiService);
    expect(service).toBeTruthy();
  });
});
