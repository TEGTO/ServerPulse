import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { ComponentFixture, fakeAsync, TestBed, tick } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { Store } from '@ngrx/store';
import { of } from 'rxjs';
import { environment } from '../../../../../environment/environment';
import { createServerSlot, CreateSlotRequest, getUserServerSlots, ServerSlot } from '../../../server-slot-shared';
import { ServerSlotBoardComponent } from './server-slot-board.component';

describe('ServerSlotBoardComponent', () => {
  let component: ServerSlotBoardComponent;
  let fixture: ComponentFixture<ServerSlotBoardComponent>;

  let storeSpy: jasmine.SpyObj<Store>;

  const mockServerSlots: ServerSlot[] = [
    { id: 'slot1', userEmail: 'user1@example.com', name: 'Test Slot 1', slotKey: 'slot-key-1' },
    { id: 'slot2', userEmail: 'user2@example.com', name: 'Test Slot 2', slotKey: 'slot-key-2' },
  ];

  beforeEach(async () => {
    storeSpy = jasmine.createSpyObj<Store>(['dispatch', 'select']);
    storeSpy.select.and.returnValue(of(mockServerSlots));

    await TestBed.configureTestingModule({
      declarations: [ServerSlotBoardComponent],
      imports: [ReactiveFormsModule],
      providers: [
        { provide: Store, useValue: storeSpy },
      ],
      schemas: [CUSTOM_ELEMENTS_SCHEMA],
    }).compileComponents();

    fixture = TestBed.createComponent(ServerSlotBoardComponent);
    component = fixture.componentInstance;
  });

  it('should create the component', () => {
    expect(component).toBeTruthy();
  });

  describe('ngOnInit', () => {
    beforeEach(() => {
      component.ngOnInit();
    });

    it('should initialize serverSlots$', (done) => {
      component.serverSlots$.subscribe((slots) => {
        expect(slots).toEqual(mockServerSlots);
        done();
      });
    });

    it('should initialize inputControl and dispatch getUserServerSlots', (done) => {
      component.serverSlots$.subscribe(() => {
        expect(storeSpy.dispatch).toHaveBeenCalledWith(getUserServerSlots({ str: inputValue }));
        done();
      });

      const inputValue = 'Test Search';
      component.inputControl.setValue(inputValue);
      fixture.detectChanges();
    });

    it('should update slotAmountSubject$ based on serverSlots$', fakeAsync(() => {
      component.serverSlots$.subscribe((slots) => {
        expect(slots).toEqual(mockServerSlots);
      });

      tick(301);

      component.slotAmount$.subscribe((slotAmount) => {
        expect(slotAmount).toEqual(mockServerSlots.length);
      });
    }));
  });

  describe('addServerSlot', () => {
    beforeEach(() => {
      component.ngOnInit();
    });

    it('should dispatch createServerSlot if slot amount is below max', () => {
      spyOnProperty(component, 'maxAmountOfSlots', 'get').and.returnValue(10);
      component["slotAmountSubject$"].next(5);

      const expectedRequest: CreateSlotRequest = { name: 'New Slot' };
      component.addServerSlot();

      expect(storeSpy.dispatch).toHaveBeenCalledWith(createServerSlot({ req: expectedRequest }));
      expect(component.inputControl.value).toBe('');
    });

    it('should not dispatch createServerSlot if slot amount reaches max', () => {
      spyOnProperty(component, 'maxAmountOfSlots', 'get').and.returnValue(5);
      component["slotAmountSubject$"].next(5);

      component.addServerSlot();

      expect(storeSpy.dispatch).not.toHaveBeenCalled();
    });
  });

  describe('Template', () => {
    beforeEach(() => {
      component.ngOnInit();
      fixture.detectChanges();
    });

    it('should display the correct slot count and max slots in the header', fakeAsync(() => {
      component.serverSlots$.subscribe((slots) => {
        expect(slots).toEqual(mockServerSlots);
      });

      tick(301);
      fixture.detectChanges();

      const headerText = fixture.nativeElement.querySelector('.slot-wrapper__header span').textContent;
      expect(headerText).toContain(`${mockServerSlots.length}/${environment.maxAmountOfSlotsPerUser}`);
    }));

    it('should trigger addServerSlot on add button click', () => {
      spyOn(component, 'addServerSlot');
      const addButton = fixture.nativeElement.querySelector('button');
      addButton.click();

      expect(component.addServerSlot).toHaveBeenCalled();
    });

    it('should trigger valueChanges on input change', () => {
      const input = fixture.nativeElement.querySelector('input');
      input.value = 'Test';
      input.dispatchEvent(new Event('input'));

      fixture.detectChanges();
      expect(component.inputControl.value).toBe('Test');
    });
  });
});
