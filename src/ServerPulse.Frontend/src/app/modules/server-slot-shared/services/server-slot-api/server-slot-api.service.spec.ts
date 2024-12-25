import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { CreateServerSlotRequest, mapServerSlotResponseToServerSlot, ServerSlot, ServerSlotApiService, ServerSlotResponse, UpdateServerSlotRequest } from '../..';
import { URLDefiner } from '../../../shared';

describe('ServerSlotApiService', () => {
  let service: ServerSlotApiService;
  let httpTestingController: HttpTestingController;
  let mockUrlDefiner: jasmine.SpyObj<URLDefiner>;

  beforeEach(() => {
    mockUrlDefiner = jasmine.createSpyObj<URLDefiner>('URLDefiner', ['combineWithServerSlotApiUrl']);
    mockUrlDefiner.combineWithServerSlotApiUrl.and.callFake((subpath: string) => `/api${subpath}`);

    TestBed.configureTestingModule({
      providers: [
        ServerSlotApiService,
        { provide: URLDefiner, useValue: mockUrlDefiner },
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });

    service = TestBed.inject(ServerSlotApiService);
    httpTestingController = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTestingController.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should get user server slots', () => {
    const expectedReq = `/api/serverslot?contains=`;
    const response: ServerSlotResponse[] = [
      { id: '1', userEmail: 'user@example.com', name: 'Slot 1', slotKey: 'key1' },
    ];
    const mappedResponse: ServerSlot[] = response.map(mapServerSlotResponseToServerSlot);

    service.getUserServerSlots().subscribe((res) => {
      expect(res).toEqual(mappedResponse);
    });

    const req = httpTestingController.expectOne(expectedReq);
    expect(req.request.method).toBe('GET');
    expect(mockUrlDefiner.combineWithServerSlotApiUrl).toHaveBeenCalledWith('/serverslot');
    req.flush(response);
  });

  it('should get server slot by ID', () => {
    const expectedReq = `/api/serverslot/1`;
    const response: ServerSlotResponse = { id: '1', userEmail: 'user@example.com', name: 'Slot 1', slotKey: 'key1' };
    const mappedResponse = mapServerSlotResponseToServerSlot(response);

    service.getServerSlotById('1').subscribe((res) => {
      expect(res).toEqual(mappedResponse);
    });

    const req = httpTestingController.expectOne(expectedReq);
    expect(req.request.method).toBe('GET');
    expect(mockUrlDefiner.combineWithServerSlotApiUrl).toHaveBeenCalledWith('/serverslot/1');
    req.flush(response);
  });

  it('should create a server slot', () => {
    const expectedReq = `/api/serverslot`;
    const request: CreateServerSlotRequest = { name: 'New Slot' };
    const response: ServerSlotResponse = { id: '1', userEmail: 'user@example.com', name: 'New Slot', slotKey: 'key1' };
    const mappedResponse = mapServerSlotResponseToServerSlot(response);

    service.createServerSlot(request).subscribe((res) => {
      expect(res).toEqual(mappedResponse);
    });

    const req = httpTestingController.expectOne(expectedReq);
    expect(req.request.method).toBe('POST');
    expect(mockUrlDefiner.combineWithServerSlotApiUrl).toHaveBeenCalledWith('/serverslot');
    req.flush(response);
  });

  it('should update a server slot', () => {
    const expectedReq = `/api/serverslot`;
    const request: UpdateServerSlotRequest = { id: '1', name: 'Updated Slot' };

    service.updateServerSlot(request).subscribe({
      next: () => {
        expect(true).toBeTruthy();
      },
    });

    const req = httpTestingController.expectOne(expectedReq);
    expect(req.request.method).toBe('PUT');
    expect(mockUrlDefiner.combineWithServerSlotApiUrl).toHaveBeenCalledWith('/serverslot');
    req.flush({});
  });

  it('should delete a server slot', () => {
    const expectedReq = `/api/serverslot/1`;

    service.deleteServerSlot('1').subscribe({
      next: () => {
        expect(true).toBeTruthy();
      },
    });

    const req = httpTestingController.expectOne(expectedReq);
    expect(req.request.method).toBe('DELETE');
    expect(mockUrlDefiner.combineWithServerSlotApiUrl).toHaveBeenCalledWith('/serverslot/1');
    req.flush({});
  });

  it('should handle errors gracefully', () => {
    const expectedReq = `/api/serverslot?contains=`;

    service.getUserServerSlots().subscribe({
      next: () => fail('Expected an error, not a success'),
      error: (error) => {
        expect(error.message).toBeTruthy();
      },
    });

    const req = httpTestingController.expectOne(expectedReq);
    req.flush('Error', { status: 400, statusText: 'Bad Request' });
  });
});
