import { TestBed } from '@angular/core/testing';

import { HostMessageService } from './host-message.service';

describe('HostMessageService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: HostMessageService = TestBed.get(HostMessageService);
    expect(service).toBeTruthy();
  });
});
