import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CommonModule } from '@angular/common';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { By } from '@angular/platform-browser';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { of } from 'rxjs';
import { ServerSlotService } from '../..';
import { environment } from '../../../../../environment/environment';
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

    await TestBed.configureTestingModule({
      imports: [
        MatButtonModule,
        NoopAnimationsModule,
        CommonModule
      ],
      declarations: [SlotBoardComponent],
      providers: [
        { provide: ServerSlotService, useValue: mockServerSlotService }
      ],
      schemas: [CUSTOM_ELEMENTS_SCHEMA]
    }).compileComponents();

    fixture = TestBed.createComponent(SlotBoardComponent);
    component = fixture.componentInstance;
    mockServerSlotService.getServerSlots.and.returnValue(of(mockServerSlots));
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize with server slots', () => {
    expect(component.serverSlots).toEqual(mockServerSlots);
  });

  it('should render the correct slot amount and max amount', () => {
    const compiled = fixture.nativeElement as HTMLElement;
    const span = compiled.querySelector('.slot-wrapper__header span');
    expect(span?.textContent).toContain(`Servers (${mockServerSlots.length}/${environment.maxAmountOfSlotsPerUser})`);
  });

  it('should call addServerSlot when add button is clicked', () => {
    spyOn(component, 'addServerSlot').and.callThrough();
    const button = fixture.debugElement.query(By.css('button')).nativeElement;
    button.click();
    expect(component.addServerSlot).toHaveBeenCalled();
  });

  it('should not add server slot if max amount is reached', () => {
    spyOn(component, 'addServerSlot').and.callThrough();
    component.serverSlots = Array(environment.maxAmountOfSlotsPerUser).fill({} as ServerSlot);
    fixture.detectChanges();
    const button = fixture.debugElement.query(By.css('button')).nativeElement;
    button.click();
    expect(component.addServerSlot).toHaveBeenCalled();
    expect(mockServerSlotService.createServerSlot).not.toHaveBeenCalled();
  });

  it('should filter server slots on input change', (done) => {
    const input = fixture.debugElement.query(By.css('#server-str')).nativeElement;
    const event = new Event('input');
    input.value = 'test';
    input.dispatchEvent(event);

    mockServerSlotService.getServerSlotsWithString.and.returnValue(of([mockServerSlots[1]]));
    component.onInputChange(event);
    setTimeout(function () {
      expect(mockServerSlotService.getServerSlotsWithString).toHaveBeenCalledWith('test');
      expect(component.serverSlots).toEqual([mockServerSlots[1]]);
      done();
    }, 600);
  });
});
