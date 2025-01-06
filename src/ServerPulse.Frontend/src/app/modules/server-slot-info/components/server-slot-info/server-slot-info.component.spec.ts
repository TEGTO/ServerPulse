/* eslint-disable @typescript-eslint/no-explicit-any */
import { ChangeDetectorRef, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { ComponentFixture, fakeAsync, TestBed, tick } from '@angular/core/testing';
import { MatDialogModule } from '@angular/material/dialog';
import { MatMenuModule } from '@angular/material/menu';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { By } from '@angular/platform-browser';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { ActivatedRoute } from '@angular/router';
import { Store } from '@ngrx/store';
import { of } from 'rxjs';
import { startCustomStatisticsReceiving, startLifecycleStatisticsReceiving, startLoadStatisticsReceiving } from '../../../analyzer';
import { getServerSlotById, ServerSlot } from '../../../server-slot-shared';
import { ServerSlotInfoComponent } from './server-slot-info.component';

describe('ServerSlotInfoComponent', () => {
  let component: ServerSlotInfoComponent;
  let fixture: ComponentFixture<ServerSlotInfoComponent>;
  let storeSpy: jasmine.SpyObj<Store>;
  let activatedRouteStub: Partial<ActivatedRoute>;

  const mockServerSlot = {
    id: 'slot1',
    slotKey: 'key123',
    userEmail: 'user@example.com',
    name: 'Test Slot',
  };

  beforeEach(() => {
    storeSpy = jasmine.createSpyObj<Store>(['dispatch', 'select']);
    storeSpy.select.and.returnValue(of(mockServerSlot));

    activatedRouteStub = {
      paramMap: of({
        get: (key: string) => (key === 'id' ? 'slot1' : null),
      } as any),
    };

    TestBed.configureTestingModule({
      declarations: [ServerSlotInfoComponent],
      imports: [MatMenuModule, MatDialogModule, MatProgressSpinnerModule, NoopAnimationsModule],
      providers: [
        { provide: Store, useValue: storeSpy },
        { provide: ActivatedRoute, useValue: activatedRouteStub },
      ],
      schemas: [CUSTOM_ELEMENTS_SCHEMA],
    }).compileComponents();

    fixture = TestBed.createComponent(ServerSlotInfoComponent);
    component = fixture.componentInstance;
  });

  it('should create the component', () => {
    expect(component).toBeTruthy();
  });

  describe('ngOnInit', () => {
    it('should initialize slotId$ and dispatch actions for the slot', fakeAsync(() => {
      spyOn(component['slotId$'], 'next').and.callThrough();

      component.ngOnInit();

      tick();

      component.serverSlot$.subscribe();

      expect(component['slotId$'].next).toHaveBeenCalledWith('slot1');
      expect(storeSpy.dispatch).toHaveBeenCalledWith(getServerSlotById({ id: 'slot1' }));
      expect(storeSpy.select).toHaveBeenCalled();
    }));

    it('should dispatch statistics receiving actions for the slot', fakeAsync(() => {
      component.ngOnInit();
      component.serverSlot$?.subscribe(() => {
        expect(storeSpy.dispatch).toHaveBeenCalledWith(startLoadStatisticsReceiving({ key: 'key123' }));
        expect(storeSpy.dispatch).toHaveBeenCalledWith(startCustomStatisticsReceiving({ key: 'key123', getInitial: false }));
        expect(storeSpy.dispatch).toHaveBeenCalledWith(startLifecycleStatisticsReceiving({ key: 'key123' }));
      });
    }));
  });

  describe('ngOnDestroy', () => {
    it('should complete the destroy$ subject', () => {
      const destroySpy = spyOn(component['destroy$'], 'complete');
      component.ngOnDestroy();
      expect(destroySpy).toHaveBeenCalled();
    });
  });

  describe('UI Interactions', () => {
    beforeEach(() => {
      component.ngOnInit();
      fixture.detectChanges();
    });

    it('should call showKey when "Key" menu item is clicked', () => {
      spyOn(component, 'showKey');

      const menuTrigger = fixture.debugElement.query(By.css('button#menu-button'));
      menuTrigger.triggerEventHandler('click', null);

      console.log(fixture.debugElement.nativeElement);
      const keyMenuItem = fixture.debugElement.query(By.css('button#key-button'));
      keyMenuItem.triggerEventHandler('click', null);

      expect(component.showKey).toHaveBeenCalledWith(mockServerSlot);
    });

    it('should call makeInputEditable when "Rename" menu item is clicked', () => {
      spyOn(component, 'makeInputEditable');

      const menuTrigger = fixture.debugElement.query(By.css('button#menu-button'));
      menuTrigger.triggerEventHandler('click', null);

      const renameMenuItem = fixture.debugElement.query(By.css('button#rename-button'));
      renameMenuItem.triggerEventHandler('click', null);

      expect(component.makeInputEditable).toHaveBeenCalled();
    });

    it('should call openConfirmDeletion when "Delete" menu item is clicked', () => {
      spyOn(component, 'openConfirmDeletion');

      const menuTrigger = fixture.debugElement.query(By.css('button#menu-button'));
      menuTrigger.triggerEventHandler('click', null);

      const deleteMenuItem = fixture.debugElement.query(By.css('button#delete-button'));
      deleteMenuItem.triggerEventHandler('click', null);

      expect(component.openConfirmDeletion).toHaveBeenCalledWith(mockServerSlot);
    });

    it('should render app-server-slot-name-changer with correct inputs', () => {
      const nameChanger = fixture.debugElement.query(By.css('app-server-slot-name-changer'));
      expect(nameChanger).toBeTruthy();
      expect(nameChanger.properties['serverSlot']).toEqual(mockServerSlot);
      expect(nameChanger.properties['inputIsEditable$']).toEqual(component.inputIsEditable$);
    });

    it('should render app-server-slot-info-download with correct input', () => {
      const infoDownload = fixture.debugElement.query(By.css('app-server-slot-info-download'));
      expect(infoDownload).toBeTruthy();
      expect(infoDownload.properties['slotKey']).toBe(mockServerSlot.slotKey);
    });

    it('should show progress spinner when server slot is loading', () => {
      component.serverSlot$ = of(null as unknown as ServerSlot);
      const cdr = fixture.debugElement.injector.get<ChangeDetectorRef>(ChangeDetectorRef);
      cdr.detectChanges();

      const spinner = fixture.debugElement.query(By.css('mat-progress-spinner'));
      expect(spinner).toBeTruthy();
    });
  });
});
