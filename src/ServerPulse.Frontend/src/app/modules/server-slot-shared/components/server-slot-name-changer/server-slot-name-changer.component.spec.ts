/* eslint-disable @typescript-eslint/no-explicit-any */
import { ElementRef } from '@angular/core';
import { ComponentFixture, fakeAsync, TestBed, tick } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { By } from '@angular/platform-browser';
import { Store } from '@ngrx/store';
import { BehaviorSubject } from 'rxjs';
import { ServerSlotNameChangerComponent, updateServerSlot } from '../..';

describe('ServerSlotNameChangerComponent', () => {
  let component: ServerSlotNameChangerComponent;
  let fixture: ComponentFixture<ServerSlotNameChangerComponent>;
  let mockStore: jasmine.SpyObj<Store>;

  beforeEach(() => {
    mockStore = jasmine.createSpyObj<Store>(['dispatch']);

    TestBed.configureTestingModule({
      declarations: [ServerSlotNameChangerComponent],
      imports: [ReactiveFormsModule],
      providers: [{ provide: Store, useValue: mockStore }],
    }).compileComponents();

    fixture = TestBed.createComponent(ServerSlotNameChangerComponent);
    component = fixture.componentInstance;
    component.serverSlot = { id: '1', name: 'Test Slot', userEmail: 'user@test.com', slotKey: 'key-1' };
    component.inputIsEditable$ = new BehaviorSubject<boolean>(false);
    fixture.detectChanges();
  });

  it('should create the component', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize the inputControl with serverSlot name', () => {
    expect(component.inputControl.value).toBe('Test Slot');
  });

  it('should update input width after view initialization', () => {
    spyOn<any>(component, 'adjustInputWidth');
    component.ngAfterViewInit();
    expect(component["adjustInputWidth"]).toHaveBeenCalled();
  });

  it('should make input editable on double click', () => {
    spyOn(component, 'makeInputEditable').and.callThrough();
    const inputElement = fixture.debugElement.query(By.css('.name-input'));
    inputElement.triggerEventHandler('dblclick', null);

    expect(component.makeInputEditable).toHaveBeenCalled();
    expect(component.inputIsEditable$.value).toBe(true);
  });

  it('should focus the input element when made editable', (done) => {
    component.makeInputEditable();
    setTimeout(() => {
      const inputElement = component.slotInputElement.nativeElement;
      expect(document.activeElement).toBe(inputElement);
      done();
    });
  });

  it('should validate and save input on blur', () => {
    spyOn<any>(component, 'validateAndSaveInput').and.callThrough();
    const inputElement = fixture.debugElement.query(By.css('.name-input'));
    inputElement.triggerEventHandler('blur', null);

    expect(component["validateAndSaveInput"]).toHaveBeenCalled();
  });

  it('should dispatch updateServerSlot action with correct payload', () => {
    component.inputControl.setValue('Updated Slot Name');
    component["validateAndSaveInput"]();

    expect(mockStore.dispatch).toHaveBeenCalledWith(
      updateServerSlot({
        req: { id: '1', name: 'Updated Slot Name' },
      })
    );
  });

  it('should set default name when input is empty', () => {
    component.inputControl.setValue('');
    component["checkEmptyInput"]();

    expect(component.inputControl.value).toBe('New slot');
  });

  it('should adjust input width dynamically', fakeAsync(() => {
    component.textSizer = {
      nativeElement: { scrollWidth: 150 },
    } as ElementRef;

    component["adjustInputWidth"]();

    tick();

    expect(component.inputWidth$.value).toBe(150);
  }));
});
