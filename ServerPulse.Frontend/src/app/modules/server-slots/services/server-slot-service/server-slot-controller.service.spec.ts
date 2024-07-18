import { TestBed } from '@angular/core/testing';
import { Store } from '@ngrx/store';
import { of } from 'rxjs';
import { createServerSlot, deleteServerSlot, getServerSlots, getServerSlotsWithString, updateServerSlot } from '../..';
import { CreateServerSlotRequest, ServerSlot, UpdateServerSlotRequest } from '../../../shared';
import { ServerSlotControllerService } from './server-slot-controller.service';

describe('ServerSlotControllerService', () => {
  let service: ServerSlotControllerService;
  let store: jasmine.SpyObj<Store>;

  const mockServerSlots: ServerSlot[] = [
    { id: '1', userEmail: 'user1@example.com', name: 'Slot 1', slotKey: 'key1' },
    { id: '2', userEmail: 'user2@example.com', name: 'Slot 2', slotKey: 'key2' }
  ];

  beforeEach(() => {
    const storeSpy = jasmine.createSpyObj('Store', ['dispatch', 'select']);

    TestBed.configureTestingModule({
      providers: [
        ServerSlotControllerService,
        { provide: Store, useValue: storeSpy }
      ]
    });

    service = TestBed.inject(ServerSlotControllerService);
    store = TestBed.inject(Store) as jasmine.SpyObj<Store>;
    store.select.and.returnValue(of(mockServerSlots));
  });

  it('should dispatch getServerSlots action and return server slots', (done) => {
    service.getServerSlots().subscribe(slots => {
      expect(store.dispatch).toHaveBeenCalledWith(getServerSlots());
      expect(slots).toEqual(mockServerSlots);
      done();
    });
  });

  it('should dispatch getServerSlotsWithString action and return server slots', (done) => {
    const searchString = 'test';
    service.getServerSlotsWithString(searchString).subscribe(slots => {
      expect(store.dispatch).toHaveBeenCalledWith(getServerSlotsWithString({ str: searchString }));
      expect(slots).toEqual(mockServerSlots);
      done();
    });
  });

  it('should dispatch createServerSlot action', () => {
    const createRequest: CreateServerSlotRequest = { name: 'New Slot' };
    service.createServerSlot(createRequest);
    expect(store.dispatch).toHaveBeenCalledWith(createServerSlot({ createRequest }));
  });

  it('should dispatch updateServerSlot action', () => {
    const updateRequest: UpdateServerSlotRequest = { id: '1', name: 'Updated Slot' };
    service.updateServerSlot(updateRequest);
    expect(store.dispatch).toHaveBeenCalledWith(updateServerSlot({ updateRequest }));
  });

  it('should dispatch deleteServerSlot action', () => {
    const id = '1';
    service.deleteServerSlot(id);
    expect(store.dispatch).toHaveBeenCalledWith(deleteServerSlot({ id }));
  });
});
