import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatMenuModule } from '@angular/material/menu';
import { By } from '@angular/platform-browser';
import { Store } from '@ngrx/store';
import { of } from 'rxjs';
import { ServerLifecycleStatistics, startLifecycleStatisticsReceiving } from '../../../analyzer';
import { deleteServerSlot, ServerSlot, ServerStatus, showSlotInfo, showSlotKey } from '../../../server-slot-shared';
import { ServerSlotComponent } from './server-slot.component';

describe('ServerSlotComponent', () => {
  let component: ServerSlotComponent;
  let fixture: ComponentFixture<ServerSlotComponent>;

  let storeSpy: jasmine.SpyObj<Store>;

  const mockServerSlot: ServerSlot = {
    id: 'slot1',
    userEmail: 'user@example.com',
    name: 'Test Slot',
    slotKey: 'slot-key-1',
  };

  const mockLifecycleStatistics: ServerLifecycleStatistics = {
    id: 'stat1',
    collectedDateUTC: new Date(),
    isAlive: true,
    dataExists: true,
    serverLastStartDateTimeUTC: new Date(),
    serverUptime: null,
    lastServerUptime: null,
    lastPulseDateTimeUTC: null,
  };

  beforeEach(async () => {
    storeSpy = jasmine.createSpyObj<Store>(['dispatch', 'select']);

    storeSpy.select.and.returnValue(of(mockLifecycleStatistics))

    await TestBed.configureTestingModule({
      declarations: [ServerSlotComponent],
      imports: [MatMenuModule],
      providers: [
        { provide: Store, useValue: storeSpy },
      ],
      schemas: [CUSTOM_ELEMENTS_SCHEMA]
    }).compileComponents();

    fixture = TestBed.createComponent(ServerSlotComponent);
    component = fixture.componentInstance;
  });

  it('should create the component', () => {
    expect(component).toBeTruthy();
  });

  describe('ngOnInit', () => {
    it('should dispatch startLifecycleStatisticsReceiving action', () => {
      component.serverSlot = mockServerSlot;

      component.ngOnInit();

      expect(storeSpy.dispatch).toHaveBeenCalledWith(startLifecycleStatisticsReceiving({ key: 'slot-key-1' }));
    });

    it('should update server status based on lifecycle statistics', () => {
      component.serverSlot = mockServerSlot;

      component.ngOnInit();

      component.serverStatus$.subscribe((status) => {
        expect(status).toBe(ServerStatus.Online);
      });
    });
  });

  describe('ngOnDestroy', () => {
    it('should complete destroy$ subject', () => {
      const completeSpy = spyOn(component['destroy$'], 'complete');
      component.ngOnDestroy();
      expect(completeSpy).toHaveBeenCalled();
    });
  });

  describe('UI Interactions', () => {
    beforeEach(() => {
      component.serverSlot = mockServerSlot;
      fixture.detectChanges();
    });

    it('should dispatch showSlotInfo action when showSlotInfo is called', () => {
      component.showSlotInfo();
      expect(storeSpy.dispatch).toHaveBeenCalledWith(showSlotInfo({ id: 'slot1' }));
    });

    it('should dispatch showSlotKey action when showKey is called', () => {
      component.showKey();
      expect(storeSpy.dispatch).toHaveBeenCalledWith(showSlotKey({ slot: mockServerSlot }));
    });

    it('should dispatch deleteServerSlot action when deleteSlot is called', () => {
      component.deleteSlot();
      expect(storeSpy.dispatch).toHaveBeenCalledWith(deleteServerSlot({ id: 'slot1' }));
    });

    it('should set inputIsEditable$ to true when makeInputEditable is called', () => {
      component.makeInputEditable();
      component.inputIsEditable$.subscribe((isEditable) => {
        expect(isEditable).toBeTrue();
      });
    });
  });

  describe('HTML Template', () => {
    beforeEach(() => {
      component.serverSlot = mockServerSlot;
      fixture.detectChanges();
    });

    it('should have the correct status indicator class', () => {
      const statusIndicator = fixture.nativeElement.querySelector('.server-status__indicator');
      expect(statusIndicator.classList).toContain('green');
    });

    it('should call showSlotInfo when info button is clicked', () => {
      spyOn(component, 'showSlotInfo');

      const infoButton = fixture.debugElement.query(By.css('button#show-slot-info-button')).nativeElement;
      infoButton.click();

      expect(component.showSlotInfo).toHaveBeenCalled();
    });
  });
});
