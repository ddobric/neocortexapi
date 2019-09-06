import { TestBed } from '@angular/core/testing';

import { NeoCortexUtilsService } from './neo-cortex-utils.service';

describe('NeoCortexUtilsService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: NeoCortexUtilsService = TestBed.get(NeoCortexUtilsService);
    expect(service).toBeTruthy();
  });
});
