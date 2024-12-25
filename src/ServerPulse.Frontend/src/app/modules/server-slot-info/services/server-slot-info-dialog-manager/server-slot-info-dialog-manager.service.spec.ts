import { TestBed } from '@angular/core/testing';
import { MatDialog } from '@angular/material/dialog';
import { CustomEventDetailsComponent } from '../..';
import { getDefaultCustomEvent } from '../../../analyzer';
import { ServerSlotInfoDialogManagerService } from './server-slot-info-dialog-manager.service';

describe('ServerSlotInfoDialogManagerService', () => {
  let service: ServerSlotInfoDialogManagerService;
  let mockMatDialog: jasmine.SpyObj<MatDialog>

  beforeEach(() => {
    mockMatDialog = jasmine.createSpyObj<MatDialog>('MatDialog', ['open']);

    TestBed.configureTestingModule({
      providers: [
        { provide: MatDialog, useValue: mockMatDialog }
      ],
    });
    service = TestBed.inject(ServerSlotInfoDialogManagerService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should custom event details dialog', () => {
    const customEvent = getDefaultCustomEvent();

    service.openCustomEventDetails(customEvent.serializedMessage);

    expect(mockMatDialog.open).toHaveBeenCalledWith(CustomEventDetailsComponent, {
      height: '500px',
      width: '550px',
      data: customEvent.serializedMessage
    });
  });
});
