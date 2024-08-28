import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { SlotDataApiService, SlotDataResponse } from '../../../shared';
import { ServerSlotDataCollectorService } from './server-slot-data-collector.service';

describe('ServerSlotDataCollectorService', () => {
  let service: ServerSlotDataCollectorService;
  let mockApiService: jasmine.SpyObj<SlotDataApiService>;

  beforeEach(() => {
    mockApiService = jasmine.createSpyObj('SlotDataApiService', ['getSlotData']);

    TestBed.configureTestingModule({
      providers: [
        ServerSlotDataCollectorService,
        { provide: SlotDataApiService, useValue: mockApiService }
      ]
    });

    service = TestBed.inject(ServerSlotDataCollectorService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('getServerSlotData should return expected data', () => {
    const slotKey = 'test-slot-key';
    const expectedResponse: SlotDataResponse = {
      collectedDateUTC: new Date(),
      generalStatistics: null,
      loadStatistics: null,
      customEventStatistics: null,
      lastLoadEvents: [],
      lastCustomEvents: []
    };

    mockApiService.getSlotData.and.returnValue(of(expectedResponse));

    service.getServerSlotData(slotKey).subscribe(data => {
      expect(data).toEqual(expectedResponse);
    });

    expect(mockApiService.getSlotData).toHaveBeenCalledWith(slotKey);
  });
});