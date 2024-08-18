import { CUSTOM_ELEMENTS_SCHEMA, DebugElement } from '@angular/core';
import { ComponentFixture, fakeAsync, TestBed, tick } from '@angular/core/testing';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef } from '@angular/material/dialog';
import { MatMenuModule } from '@angular/material/menu';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { By } from '@angular/platform-browser';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { of } from 'rxjs';
import { RedirectorService, ServerStatisticsResponse, SnackbarManager, TimeSpan } from '../../../shared';
import { ServerSlotDialogManager, ServerSlotService, ServerStatisticsService } from '../../index';
import { ServerSlotComponent, ServerStatus } from './server-slot.component';

describe('ServerSlotComponent', () => {
  let component: ServerSlotComponent;
  let fixture: ComponentFixture<ServerSlotComponent>;
  let debugElement: DebugElement;
  let mockServerSlotService: jasmine.SpyObj<ServerSlotService>;
  let mockDialogManager: jasmine.SpyObj<ServerSlotDialogManager>;
  let mockRedirector: jasmine.SpyObj<RedirectorService>;
  let mockSnackBarManager: jasmine.SpyObj<SnackbarManager>;
  let mockServerStatisticsService: jasmine.SpyObj<ServerStatisticsService>;

  const mockServerSlot = {
    id: '1',
    name: 'Test Slot',
    slotKey: 'testkey',
    userEmail: 'test@example.com'
  };

  const mockStatistics: ServerStatisticsResponse = {
    isAlive: true,
    dataExists: true,
    serverLastStartDateTimeUTC: new Date(),
    serverUptime: new TimeSpan(),
    lastServerUptime: new TimeSpan(),
    lastPulseDateTimeUTC: new Date(),
    collectedDateUTC: new Date(),
    isInitial: false
  };

  const mockDialogRef = {
    afterClosed: () => of(true)
  } as MatDialogRef<any>;

  beforeEach(async () => {
    mockServerSlotService = jasmine.createSpyObj('ServerSlotService', ['updateServerSlot', 'deleteServerSlot']);
    mockDialogManager = jasmine.createSpyObj('ServerSlotDialogManager', ['openDeleteSlotConfirmMenu']);
    mockRedirector = jasmine.createSpyObj('RedirectorService', ['redirectTo']);
    mockSnackBarManager = jasmine.createSpyObj('SnackbarManager', ['openInfoSnackbar']);
    mockServerStatisticsService = jasmine.createSpyObj('ServerStatisticsService', ['getLastServerStatistics']);

    mockServerStatisticsService.getLastServerStatistics.and.returnValue(of({ key: mockServerSlot.slotKey, statistics: mockStatistics }));

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
        { provide: ServerSlotService, useValue: mockServerSlotService },
        { provide: ServerSlotDialogManager, useValue: mockDialogManager },
        { provide: RedirectorService, useValue: mockRedirector },
        { provide: SnackbarManager, useValue: mockSnackBarManager },
        { provide: ServerStatisticsService, useValue: mockServerStatisticsService }
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

  it('should initialize inputControl with serverSlot name on init', () => {
    expect(component.inputControl.value).toBe(mockServerSlot.name);
  });

  it('should call redirectToInfo when Info button is clicked', () => {
    const infoButton = debugElement.query(By.css('button[mat-button]'));
    infoButton.triggerEventHandler('click', null);
    expect(mockRedirector.redirectTo).toHaveBeenCalledWith(`serverslot/${mockServerSlot.id}`);
  });

  it('should open snackbar with key when showKey is called', () => {
    component.showKey();
    expect(mockSnackBarManager.openInfoSnackbar).toHaveBeenCalledWith(`🔑: ${mockServerSlot.slotKey}`, 10);
  });

  it('should make input editable and focus when makeInputEditable is called', fakeAsync(() => {
    component.makeInputEditable();
    tick();
    expect(component.inputIsEditable$.getValue()).toBeTrue();
    const inputElement = debugElement.query(By.css('input[name="slotName"]')).nativeElement;
    expect(inputElement).toBe(document.activeElement);
  }));

  it('should update input width when input value changes', fakeAsync(() => {
    component.inputControl.setValue('Updated Slot Name');
    fixture.detectChanges();
    tick(300);
    const sizerWidth = component.textSizer.nativeElement.scrollWidth;
    expect(component.inputWidth$.getValue()).toBe(sizerWidth + 1);
  }));

  it('should reset input value to "New slot" if input is empty on blur', () => {
    component.inputControl.setValue('');
    component.onBlur();
    expect(component.inputControl.value).toBe('New slot');
    expect(mockServerSlotService.updateServerSlot).toHaveBeenCalledWith({ id: mockServerSlot.id, name: 'New slot' });
  });

  it('should call deleteServerSlot when confirm deletion dialog returns true', () => {
    mockDialogManager.openDeleteSlotConfirmMenu.and.returnValue(mockDialogRef);
    component.openConfirmDeletion();
    expect(mockDialogManager.openDeleteSlotConfirmMenu).toHaveBeenCalled();
    expect(mockServerSlotService.deleteServerSlot).toHaveBeenCalledWith(mockServerSlot.id);
  });

  it('should set serverStatus to Online if data exists and isAlive is true', () => {
    const method = spyOn<any>(component, 'handleStatisticsMessage').and.callThrough();
    method.call(component, { key: mockServerSlot.slotKey, statistics: { ...mockStatistics, isAlive: true } });
    expect(component.serverStatus$.getValue()).toBe(ServerStatus.Online);
  });

  it('should set serverStatus to Offline if data exists and isAlive is false', () => {
    component['handleStatisticsMessage']({ key: mockServerSlot.slotKey, statistics: { ...mockStatistics, dataExists: true, isAlive: false } });
    expect(component.serverStatus$.getValue()).toBe(ServerStatus.Offline);
  });

  it('should set serverStatus to NoData if data does not exist', () => {
    component['handleStatisticsMessage']({ key: mockServerSlot.slotKey, statistics: { ...mockStatistics, dataExists: false } });
    expect(component.serverStatus$.getValue()).toBe(ServerStatus.NoData);
  });

  it('should update serverSlot name when validateAndSaveInput is called', async () => {
    const newName = 'Updated Slot Name';
    component.inputControl.setValue(newName);
    component['validateAndSaveInput']();
    await fixture.whenStable();
    expect(mockServerSlotService.updateServerSlot).toHaveBeenCalledWith({ id: mockServerSlot.id, name: newName });
  });
});