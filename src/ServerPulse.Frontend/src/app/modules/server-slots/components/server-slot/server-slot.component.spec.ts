import { CUSTOM_ELEMENTS_SCHEMA, DebugElement } from '@angular/core';
import { ComponentFixture, fakeAsync, TestBed, tick } from '@angular/core/testing';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef } from '@angular/material/dialog';
import { MatMenuModule } from '@angular/material/menu';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { of } from 'rxjs';
import { ServerSlotDialogManager, ServerSlotService, ServerStatisticsService } from '../..';
import { RedirectorService, ServerStatisticsResponse, SnackbarManager, TimeSpan } from '../../../shared';
import { ServerSlotComponent, ServerStatus } from './server-slot.component';

describe('ServerSlotComponent', () => {
  let component: ServerSlotComponent;
  let fixture: ComponentFixture<ServerSlotComponent>;
  let debugElement: DebugElement;
  let serverSlotService: jasmine.SpyObj<ServerSlotService>;
  let dialogManager: jasmine.SpyObj<ServerSlotDialogManager>;
  let redirector: jasmine.SpyObj<RedirectorService>;
  let snackBarManager: jasmine.SpyObj<SnackbarManager>;
  let serverStatisticsService: jasmine.SpyObj<ServerStatisticsService>;

  const mockServerSlot = {
    id: '1',
    name: 'Test Slot',
    slotKey: 'testkey',
    userEmail: 'test@example.com'
  };
  const mockStatistics: ServerStatisticsResponse = {
    isAlive: true,
    dataExists: true,
    serverLastStartDateTime: new Date(),
    serverUptime: new TimeSpan(),
    lastServerUptime: new TimeSpan(),
    lastPulseDateTime: new Date()
  };

  const mockDialogRef = {
    afterClosed: () => of(true)
  } as MatDialogRef<any>;

  beforeEach(async () => {
    serverSlotService = jasmine.createSpyObj('ServerSlotService', ['updateServerSlot', 'deleteServerSlot']);
    dialogManager = jasmine.createSpyObj('ServerSlotDialogManager', ['openDeleteSlotConfirmMenu']);
    redirector = jasmine.createSpyObj('RedirectorService', ['redirectTo']);
    snackBarManager = jasmine.createSpyObj('SnackbarManager', ['openInfoSnackbar']);
    serverStatisticsService = jasmine.createSpyObj('ServerStatisticsService', ['startConnection', 'startListenPulse', 'receiveStatistics']);

    serverStatisticsService.startConnection.and.returnValue(of());
    serverStatisticsService.receiveStatistics.and.returnValue(of({ key: mockServerSlot.slotKey, data: JSON.stringify(mockStatistics) }));

    await TestBed.configureTestingModule({
      declarations: [ServerSlotComponent],
      imports: [
        MatMenuModule,
        MatSnackBarModule,
        FormsModule,
        ReactiveFormsModule,
        NoopAnimationsModule
      ],
      providers: [
        { provide: ServerSlotService, useValue: serverSlotService },
        { provide: ServerSlotDialogManager, useValue: dialogManager },
        { provide: RedirectorService, useValue: redirector },
        { provide: SnackbarManager, useValue: snackBarManager },
        { provide: ServerStatisticsService, useValue: serverStatisticsService }
      ],
      schemas: [CUSTOM_ELEMENTS_SCHEMA]
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ServerSlotComponent);
    component = fixture.componentInstance;
    debugElement = fixture.debugElement;
    component.serverSlot = mockServerSlot;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize inputValue with serverSlot name on init', () => {
    expect(component.inputValue).toBe(mockServerSlot.name);
  });

  it('should call redirectToInfo', () => {
    component.redirectToInfo();
    expect(redirector.redirectTo).toHaveBeenCalledWith(`serverslot/${mockServerSlot.id}`);
  });

  it('should open snackbar with key when showKey is called', () => {
    component.showKey();
    expect(snackBarManager.openInfoSnackbar).toHaveBeenCalledWith(`🔑: ${mockServerSlot.slotKey}`, 10);
  });

  it('should make input editable and focus when makeInputEditable is called', fakeAsync(() => {
    component.makeInputEditable();
    tick();
    expect(component.inputIsEditable).toBeTrue();
    expect(component.nameInput.nativeElement).toBe(document.activeElement! as HTMLInputElement);
  }));

  it('should update input width when input value changes', () => {
    component.onInputChange();
    const sizerWidth = component.textSizer.nativeElement.scrollWidth;
    expect(component.inputWidth).toBe(sizerWidth);
  });

  it('should reset input value to "New slot" if input is empty on blur', () => {
    component.inputValue = '';
    component.onBlur();
    expect(component.inputValue).toBe('New slot');
    expect(serverSlotService.updateServerSlot).toHaveBeenCalledWith({ id: mockServerSlot.id, name: 'New slot' });
  });

  it('should call deleteServerSlot when confirm deletion dialog returns true', () => {
    dialogManager.openDeleteSlotConfirmMenu.and.returnValue(mockDialogRef);
    component.openConfirmDeletion();
    expect(dialogManager.openDeleteSlotConfirmMenu).toHaveBeenCalled();
    expect(serverSlotService.deleteServerSlot).toHaveBeenCalledWith(mockServerSlot.id);
  });

  it('should set serverStatus to Online if data exists and isAlive is true', () => {
    (component as any).currentServerSlotStatistics = {
      isAlive: true,
      dataExists: true,
      serverLastStartDateTime: new Date(),
      serverUptime: null,
      lastServerUptime: null,
      lastPulseDateTime: new Date(),
      lastLoadDateTime: new Date(),
      loadEventNumber: 1
    };
    component.toggleServerStatus();
    expect(component.serverStatus).toBe(ServerStatus.Online);
  });

  it('should set serverStatus to Offline if data exists and isAlive is false', () => {
    (component as any).currentServerSlotStatistics = {
      isAlive: false,
      dataExists: true,
      serverLastStartDateTime: new Date(),
      serverUptime: null,
      lastServerUptime: null,
      lastPulseDateTime: new Date(),
      lastLoadDateTime: new Date(),
      loadEventNumber: 1
    };
    component.toggleServerStatus();
    expect(component.serverStatus).toBe(ServerStatus.Offline);
  });

  it('should set serverStatus to NoData if data does not exist', () => {
    (component as any).currentServerSlotStatistics = {
      isAlive: false,
      dataExists: false,
      serverLastStartDateTime: new Date(),
      serverUptime: null,
      lastServerUptime: null,
      lastPulseDateTime: new Date(),
      lastLoadDateTime: new Date(),
      loadEventNumber: 1
    };
    component.toggleServerStatus();
    expect(component.serverStatus).toBe(ServerStatus.NoData);
  });
});