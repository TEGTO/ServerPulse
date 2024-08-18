import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { ComponentFixture, fakeAsync, TestBed, tick } from '@angular/core/testing';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { By } from '@angular/platform-browser';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { of } from 'rxjs';
import { ServerSlotService } from '../..';
import { ServerSlot } from '../../../shared';
import { SlotBoardComponent } from './slot-board.component';

describe('SlotBoardComponent', () => {
  let component: SlotBoardComponent;
  let fixture: ComponentFixture<SlotBoardComponent>;
  let mockServerSlotService: jasmine.SpyObj<ServerSlotService>;

  const mockServerSlots: ServerSlot[] = [
    { id: '1', userEmail: 'user1@example.com', name: 'Slot 1', slotKey: 'key1' },
    { id: '2', userEmail: 'user2@example.com', name: 'Slot 2', slotKey: 'key2' }
  ];

  beforeEach(async () => {
    mockServerSlotService = jasmine.createSpyObj('ServerSlotService', ['getServerSlots', 'createServerSlot', 'getServerSlotsWithString']);
    mockServerSlotService.getServerSlots.and.returnValue(of(mockServerSlots));
    mockServerSlotService.getServerSlotsWithString.and.returnValue(of(mockServerSlots));

    await TestBed.configureTestingModule({
      imports: [
        MatButtonModule,
        FormsModule,
        ReactiveFormsModule,
        NoopAnimationsModule
      ],
      declarations: [SlotBoardComponent],
      providers: [
        { provide: ServerSlotService, useValue: mockServerSlotService }
      ],
      schemas: [CUSTOM_ELEMENTS_SCHEMA]
    }).compileComponents();

    fixture = TestBed.createComponent(SlotBoardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize with server slots', fakeAsync(() => {
    component.serverSlots$.subscribe(slots => {
      expect(slots).toEqual(mockServerSlots);
    });
    tick(300);
  }));

  it('should render the correct slot amount and max amount', async () => {
    await fixture.whenStable();

    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const span = compiled.querySelector('.slot-wrapper__header span');

    expect(span?.textContent).toContain(`Servers (${mockServerSlots.length}/${component.maxAmountOfSlots})`);
  });

  it('should call addServerSlot when add button is clicked', () => {
    spyOn(component, 'addServerSlot').and.callThrough();
    const button = fixture.debugElement.query(By.css('button[mat-button]')).nativeElement;
    button.click();
    expect(component.addServerSlot).toHaveBeenCalled();
  });

  it('should not add server slot if max amount is reached', () => {
    component['slotAmountSubject$'].next(component.maxAmountOfSlots);
    fixture.detectChanges();
    const button = fixture.debugElement.query(By.css('button[mat-button]')).nativeElement;
    button.click();
    expect(mockServerSlotService.createServerSlot).not.toHaveBeenCalled();
  });

  it('should filter server slots on input change', fakeAsync(() => {
    mockServerSlotService.getServerSlotsWithString.and.returnValue(of([mockServerSlots[1]]));
    component.serverSlots$.subscribe(slots => {
      expect(slots).toEqual([mockServerSlots[1]]);
    });
    const input = fixture.debugElement.query(By.css('#server-str')).nativeElement;
    input.value = 'Slot 2';
    input.dispatchEvent(new Event('input'));
    tick(300);
    fixture.detectChanges();

  }));

  it('should reset input value to empty after adding a server slot', () => {
    spyOn(component.inputControl, 'setValue');
    component.addServerSlot();
    expect(component.inputControl.setValue).toHaveBeenCalledWith('');
  });

  it('should update slotAmountSubject$ when serverSlots$ emits new slots', async () => {
    const newSlots = [{ id: '3', userEmail: 'user3@example.com', name: 'Slot 3', slotKey: 'key3' }];
    mockServerSlotService.getServerSlots.and.returnValue(of(newSlots));

    component.ngOnInit();

    await fixture.whenStable();
    fixture.detectChanges();

    component.slotAmount$.subscribe(amount => {
      expect(amount).toBe(newSlots.length);
    });
  });
});