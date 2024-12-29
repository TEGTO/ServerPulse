import { TestBed } from '@angular/core/testing';

import { MatDialog } from '@angular/material/dialog';
import { ServerSlotDeleteConfirmComponent } from '../..';
import { ServerSlotDialogManagerService } from './server-slot-dialog-manager.service';

describe('ServerSlotDialogManagerService', () => {
  let service: ServerSlotDialogManagerService;
  let mockMatDialog: jasmine.SpyObj<MatDialog>

  beforeEach(() => {
    mockMatDialog = jasmine.createSpyObj<MatDialog>('MatDialog', ['open']);

    TestBed.configureTestingModule({
      providers: [
        { provide: MatDialog, useValue: mockMatDialog }
      ],
    });
    service = TestBed.inject(ServerSlotDialogManagerService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should open delete confirm dialog', () => {
    service.openDeleteSlotConfirmMenu();

    expect(mockMatDialog.open).toHaveBeenCalledWith(ServerSlotDeleteConfirmComponent, {
      height: '200px',
      width: '450px',
    });
  });
});