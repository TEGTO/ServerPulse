import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { CreateServerSlotRequest, ServerSlotResponse, UpdateServerSlotRequest, URLDefiner } from '../../../index';
import { ServerSlotApiService } from './server-slot-api.service';

describe('ServerSlotApiService', () => {
  let httpTestingController: HttpTestingController;
  let service: ServerSlotApiService;
  let mockUrlDefiner: jasmine.SpyObj<URLDefiner>;

  beforeEach(() => {
    mockUrlDefiner = jasmine.createSpyObj('URLDefiner', ['combineWithServerSlotApiUrl']);
    mockUrlDefiner.combineWithServerSlotApiUrl.and.callFake((subpath: string) => `/api/server-slot${subpath}`);

    TestBed.configureTestingModule({
      providers: [
        ServerSlotApiService,
        { provide: URLDefiner, useValue: mockUrlDefiner }
      ],
      imports: [HttpClientTestingModule]
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
    const expectedUrl = `/api/server-slot`;
    const response: ServerSlotResponse[] = [
      { id: '1', userEmail: 'user1@example.com', name: 'Server 1', slotKey: 'key1' },
      { id: '2', userEmail: 'user2@example.com', name: 'Server 2', slotKey: 'key2' }
    ];

    service.getUserServerSlots().subscribe(res => {
      expect(res).toEqual(response);
    });

    const req = httpTestingController.expectOne(expectedUrl);
    expect(req.request.method).toBe('GET');
    expect(mockUrlDefiner.combineWithServerSlotApiUrl).toHaveBeenCalledWith('');
    req.flush(response);
  });

  it('should get server slot by id', () => {
    const id = '1';
    const expectedUrl = `/api/server-slot/${id}`;
    const response: ServerSlotResponse = { id: '1', userEmail: 'user1@example.com', name: 'Server 1', slotKey: 'key1' };

    service.getServerSlotById(id).subscribe(res => {
      expect(res).toEqual(response);
    });

    const req = httpTestingController.expectOne(expectedUrl);
    expect(req.request.method).toBe('GET');
    expect(mockUrlDefiner.combineWithServerSlotApiUrl).toHaveBeenCalledWith(`/${id}`);
    req.flush(response);
  });

  it('should get user server slots with string', () => {
    const str = 'test';
    const expectedUrl = `/api/server-slot/contains/${str}`;
    const response: ServerSlotResponse[] = [
      { id: '1', userEmail: 'user1@example.com', name: 'Server 1', slotKey: 'key1' }
    ];

    service.getUserServerSlotsWithString(str).subscribe(res => {
      expect(res).toEqual(response);
    });

    const req = httpTestingController.expectOne(expectedUrl);
    expect(req.request.method).toBe('GET');
    expect(mockUrlDefiner.combineWithServerSlotApiUrl).toHaveBeenCalledWith(`/contains/${str}`);
    req.flush(response);
  });

  it('should create a server slot', () => {
    const request: CreateServerSlotRequest = { name: 'New Server' };
    const expectedUrl = `/api/server-slot`;
    const response: ServerSlotResponse = { id: '1', userEmail: 'user1@example.com', name: 'New Server', slotKey: 'key1' };

    service.createServerSlot(request).subscribe(res => {
      expect(res).toEqual(response);
    });

    const req = httpTestingController.expectOne(expectedUrl);
    expect(req.request.method).toBe('POST');
    expect(mockUrlDefiner.combineWithServerSlotApiUrl).toHaveBeenCalledWith('');
    req.flush(response);
  });

  it('should update a server slot', () => {
    const request: UpdateServerSlotRequest = { id: '1', name: 'Updated Server' };
    const expectedUrl = `/api/server-slot`;

    service.updateServerSlot(request).subscribe();

    const req = httpTestingController.expectOne(expectedUrl);
    expect(req.request.method).toBe('PUT');
    expect(mockUrlDefiner.combineWithServerSlotApiUrl).toHaveBeenCalledWith('');
  });

  it('should delete a server slot', () => {
    const id = '1';
    const expectedUrl = `/api/server-slot/${id}`;

    service.deleteServerSlot(id).subscribe();

    const req = httpTestingController.expectOne(expectedUrl);
    expect(req.request.method).toBe('DELETE');
    expect(mockUrlDefiner.combineWithServerSlotApiUrl).toHaveBeenCalledWith(`/${id}`);
  });

  it('should handle error response', () => {
    const errorResponse = { status: 404, statusText: 'Not Found' };
    const expectedUrl = `/api/server-slot`;

    service.getUserServerSlots().subscribe(
      () => fail('Expected an error, but got a success response'),
      (error) => {
        expect(error).toBeTruthy();
      }
    );

    const req = httpTestingController.expectOne(expectedUrl);
    req.flush('Error', errorResponse);
  });
});