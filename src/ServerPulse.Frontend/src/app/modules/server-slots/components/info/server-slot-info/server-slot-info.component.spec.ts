import { CUSTOM_ELEMENTS_SCHEMA, DebugElement, ElementRef } from '@angular/core';
import { ComponentFixture, fakeAsync, TestBed, tick } from '@angular/core/testing';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatMenuModule } from '@angular/material/menu';
import { By } from '@angular/platform-browser';
import { ActivatedRoute } from '@angular/router';
import { of } from 'rxjs';
import { RedirectorService, SnackbarManager } from '../../../../shared';
import { ServerSlotDialogManager, ServerSlotService, ServerStatisticsService } from '../../../index';
import { ServerSlotInfoComponent } from './server-slot-info.component';

describe('ServerSlotInfoComponent', () => {
  let component: ServerSlotInfoComponent;
  let fixture: ComponentFixture<ServerSlotInfoComponent>;
  let debugElement: DebugElement;
  let mockServerSlotService: jasmine.SpyObj<ServerSlotService>;
  let mockDialogManager: jasmine.SpyObj<ServerSlotDialogManager>;
  let mockRedirector: jasmine.SpyObj<RedirectorService>;
  let mockSnackBarManager: jasmine.SpyObj<SnackbarManager>;
  let mockServerStatisticsService: jasmine.SpyObj<ServerStatisticsService>;
  let mockRoute: jasmine.SpyObj<ActivatedRoute>;

  const mockServerSlot = {
    id: '1',
    name: 'Test Slot',
    slotKey: 'testkey',
    userEmail: 'test@example.com'
  };

  beforeEach(async () => {
    mockServerSlotService = jasmine.createSpyObj('ServerSlotService', ['getServerSlotById', 'updateServerSlot', 'deleteServerSlot']);
    mockDialogManager = jasmine.createSpyObj('ServerSlotDialogManager', ['openDeleteSlotConfirmMenu']);
    mockRedirector = jasmine.createSpyObj('RedirectorService', ['redirectToHome']);
    mockSnackBarManager = jasmine.createSpyObj('SnackbarManager', ['openInfoSnackbar']);
    mockServerStatisticsService = jasmine.createSpyObj('ServerStatisticsService', ['setCurrentLoadStatisticsDate']);
    mockRoute = jasmine.createSpyObj('ActivatedRoute', [], {
      paramMap: of({
        get: (key: string) => (key === 'id' ? mockServerSlot.id : null)
      })
    });

    await TestBed.configureTestingModule({
      declarations: [ServerSlotInfoComponent],
      imports: [
        FormsModule,
        ReactiveFormsModule,
        MatMenuModule
      ],
      providers: [
        { provide: ServerSlotService, useValue: mockServerSlotService },
        { provide: ServerSlotDialogManager, useValue: mockDialogManager },
        { provide: RedirectorService, useValue: mockRedirector },
        { provide: SnackbarManager, useValue: mockSnackBarManager },
        { provide: ServerStatisticsService, useValue: mockServerStatisticsService },
        { provide: ActivatedRoute, useValue: mockRoute }
      ],
      schemas: [CUSTOM_ELEMENTS_SCHEMA]
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ServerSlotInfoComponent);
    component = fixture.componentInstance;
    debugElement = fixture.debugElement;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize inputControl with serverSlot name on init', () => {
    mockServerSlotService.getServerSlotById.and.returnValue(of(mockServerSlot));
    component.ngOnInit();
    fixture.detectChanges();
    expect(component.inputControl.value).toBe(mockServerSlot.name);
  });

  it('should adjust input width when input value changes', fakeAsync(() => {
    mockServerSlotService.getServerSlotById.and.returnValue(of(mockServerSlot));
    component.ngOnInit();
    fixture.detectChanges();

    // Mock textSizer element with a scrollWidth
    component.textSizer = {
      nativeElement: { scrollWidth: 150 }
    } as ElementRef;

    component.inputControl.setValue('Updated Slot Name');
    fixture.detectChanges();
    tick(300);

    expect(component.inputWidth$.getValue()).toBe(151);  // 150 (scrollWidth) + 1
  }));

  it('should reset input value to "New slot" if input is empty on blur', () => {
    mockServerSlotService.getServerSlotById.and.returnValue(of(mockServerSlot));
    component.ngOnInit();
    fixture.detectChanges();

    component.inputControl.setValue('');
    component.onBlur();

    expect(component.inputControl.value).toBe('New slot');
    expect(mockServerSlotService.updateServerSlot).toHaveBeenCalledWith({ id: mockServerSlot.id, name: 'New slot' });
  });

  it('should make input editable and focus when makeInputEditable is called', fakeAsync(() => {
    component.serverSlot$.next(mockServerSlot);
    fixture.detectChanges();
    component.makeInputEditable();
    tick();
    expect(component.inputIsEditable$.getValue()).toBeTrue();
    const inputElement = debugElement.query(By.css('input[name="slotName"]')).nativeElement;
    expect(inputElement).toBe(document.activeElement);
  }));

  it('should open snackbar with key when showKey is called', () => {
    mockServerSlotService.getServerSlotById.and.returnValue(of(mockServerSlot));
    component.ngOnInit();
    fixture.detectChanges();

    component.showKey();
    expect(mockSnackBarManager.openInfoSnackbar).toHaveBeenCalledWith(`🔑: ${mockServerSlot.slotKey}`, 10);
  });

  it('should open confirm deletion dialog and delete server slot on confirm', () => {
    const mockDialogRef = {
      afterClosed: () => of(true)
    } as any;

    mockDialogManager.openDeleteSlotConfirmMenu.and.returnValue(mockDialogRef);
    component.serverSlot$.next(mockServerSlot);
    component.openConfirmDeletion();

    expect(mockDialogManager.openDeleteSlotConfirmMenu).toHaveBeenCalled();
    expect(mockServerSlotService.deleteServerSlot).toHaveBeenCalledWith(mockServerSlot.id);
    expect(mockRedirector.redirectToHome).toHaveBeenCalled();
  });

  it('should not delete server slot if dialog is closed without confirmation', () => {
    const mockDialogRef = {
      afterClosed: () => of(false)
    } as any;

    mockDialogManager.openDeleteSlotConfirmMenu.and.returnValue(mockDialogRef);
    component.serverSlot$.next(mockServerSlot);
    component.openConfirmDeletion();

    expect(mockDialogManager.openDeleteSlotConfirmMenu).toHaveBeenCalled();
    expect(mockServerSlotService.deleteServerSlot).not.toHaveBeenCalled();
    expect(mockRedirector.redirectToHome).not.toHaveBeenCalled();
  });

  it('should clean up subscriptions on destroy', () => {
    spyOn(component['destroy$'], 'next');
    spyOn(component['destroy$'], 'complete');
    component.ngOnDestroy();
    expect(component['destroy$'].next).toHaveBeenCalled();
    expect(component['destroy$'].complete).toHaveBeenCalled();
  });
});