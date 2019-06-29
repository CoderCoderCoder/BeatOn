import { TestBed } from '@angular/core/testing';

import { ToolbarEventsService } from './toolbar-events.service';

describe('ToolbarEventsService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: ToolbarEventsService = TestBed.get(ToolbarEventsService);
    expect(service).toBeTruthy();
  });
});
