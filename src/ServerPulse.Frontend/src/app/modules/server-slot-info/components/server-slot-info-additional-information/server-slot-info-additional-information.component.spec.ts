/* eslint-disable @typescript-eslint/no-explicit-any */
import { CdkVirtualScrollViewport, ScrollingModule } from '@angular/cdk/scrolling';
import { ChangeDetectorRef, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { ComponentFixture, fakeAsync, TestBed, tick } from '@angular/core/testing';
import { BrowserModule, By } from '@angular/platform-browser';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { MemoizedSelector, Store } from '@ngrx/store';
import { of, Subject } from 'rxjs';
import { addNewCustomEvent, getSomeCustomEvents, selectCustomEvents, selectCustomReadFromDate, selectSelectedDate, setCustomReadFromDate, showCustomDetailsEvent, SlotInfoState } from '../..';
import { CustomEvent, getDefaultServerLoadStatistics, selectLastLoadStatisticsByKey, ServerLoadStatistics, ServerLoadStatisticsState } from '../../../analyzer';
import { ServerSlotInfoAdditionalInformationComponent } from './server-slot-info-additional-information.component';

describe('ServerSlotInfoAdditionalInformationComponent', () => {
  let component: ServerSlotInfoAdditionalInformationComponent;
  let fixture: ComponentFixture<ServerSlotInfoAdditionalInformationComponent>;
  let mockStore: jasmine.SpyObj<Store>;
  const mockSlotKey = 'test-slot-key';

  beforeEach(() => {
    mockStore = jasmine.createSpyObj<Store>('Store', ['dispatch', 'select']);

    TestBed.configureTestingModule({
      declarations: [ServerSlotInfoAdditionalInformationComponent],
      imports: [ScrollingModule, BrowserModule, NoopAnimationsModule],
      providers: [
        { provide: Store, useValue: mockStore },
      ],
      schemas: [CUSTOM_ELEMENTS_SCHEMA]
    }).compileComponents();

    fixture = TestBed.createComponent(ServerSlotInfoAdditionalInformationComponent);
    component = fixture.componentInstance;
    component.slotKey = mockSlotKey;

    mockStore.select.and.returnValue(of([]));
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('ngOnInit', () => {
    let mockEvent: CustomEvent;
    let mockStatistics: ServerLoadStatistics;
    let mockDate: Date;

    beforeEach(() => {
      mockEvent = {
        id: 'event-1',
        key: mockSlotKey,
        creationDateUTC: new Date(),
        name: 'Event Name',
        description: 'Description',
        serializedMessage: '{}',
      };

      mockStatistics = { ...getDefaultServerLoadStatistics(), loadMethodStatistics: { ...getDefaultServerLoadStatistics(), getAmount: 1, postAmount: 2, putAmount: 3, patchAmount: 4, deleteAmount: 5 } };

      mockDate = new Date();

      mockStore.select.and.callFake((selector: MemoizedSelector<object, Date, (s1: SlotInfoState) => Date> | MemoizedSelector<object, CustomEvent[], (s1: SlotInfoState) => CustomEvent[]> | MemoizedSelector<object, ServerLoadStatistics | null, (s1: ServerLoadStatisticsState) => ServerLoadStatistics | null>) => {
        if (selector === selectLastLoadStatisticsByKey(mockSlotKey)) {
          return of(mockStatistics);
        }
        if (selector === selectSelectedDate) {
          return of(mockDate);
        }
        if (selector === selectCustomReadFromDate) {
          return of(mockDate);
        }
        if (selector === selectCustomEvents) {
          return of([mockEvent]);
        }
        if (typeof selector === 'function') {
          return of(mockEvent);
        }
        return of(null);
      });
    });

    it('should set up chartData$', () => {
      mockStore.select.and.returnValue(of(mockStatistics));
      component.ngOnInit();
      component.chartData$.subscribe((chartData) => {
        expect(chartData.size).toBe(5);
        expect(chartData.get('GET')).toBe(1);
        expect(chartData.get('POST')).toBe(2);
      });
    });

    it('should dispatch addNewCustomEvent if last custom event exists and date is today', fakeAsync(() => {
      component.ngOnInit();

      tick()

      expect(mockStore.dispatch).toHaveBeenCalledWith(jasmine.objectContaining({
        type: addNewCustomEvent.type
      }));
    }));

    it('should set up customEvents$ and dispatch getSomeCustomEvents on date change', () => {
      component.ngOnInit();
      component.customEvents$.subscribe((events) => {
        expect(events).toBeTruthy();
        console.log(events);
        expect(events.length).toEqual(1);
        expect(events[0].id).toBe('event-1');
      });

      expect(mockStore.dispatch).toHaveBeenCalledWith(
        getSomeCustomEvents({
          req: {
            key: mockSlotKey,
            numberOfMessages: component["tablePageAmount"],
            startDate: mockDate,
            readNew: false,
          },
        })
      );
    });
  });

  describe('ngAfterViewInit', () => {
    it('should monitor scrolling for fetching data', fakeAsync(() => {
      const mockScrollOffsets = [10, 5];
      const mockEvents: CustomEvent[] = [
        { id: '1', key: mockSlotKey, creationDateUTC: new Date(), name: 'Event', description: 'Desc', serializedMessage: '{}' },
      ];

      const scrollerSubject = new Subject<number>();
      const offsetQueue = [...mockScrollOffsets];

      const measureScrollOffsetSpy = jasmine.createSpy('measureScrollOffset').and.callFake(() => {
        return offsetQueue.length ? offsetQueue.shift() : 0;
      });

      component.scroller = {
        elementScrolled: () => scrollerSubject.asObservable(),
        measureScrollOffset: measureScrollOffsetSpy,
      } as unknown as CdkVirtualScrollViewport;

      mockStore.select.and.returnValue(of(mockEvents));
      spyOn<any>(component, 'monitorScrollForFetching').and.callThrough();

      component.ngAfterViewInit();

      scrollerSubject.next(mockScrollOffsets[0]);
      scrollerSubject.next(mockScrollOffsets[1]);

      tick(5000);

      expect(component["monitorScrollForFetching"]).toHaveBeenCalled();

      expect(mockStore.dispatch).toHaveBeenCalledWith(jasmine.objectContaining(
        {
          type: setCustomReadFromDate.type
        }
      ));
    }));
  });

  describe('openDetailMenu', () => {
    it('should dispatch showCustomDetailsEvent', () => {
      const mockEvent: CustomEvent = {
        id: 'event-1',
        key: mockSlotKey,
        creationDateUTC: new Date(),
        name: 'Event Name',
        description: 'Description',
        serializedMessage: '{}',
      };

      component.openDetailMenu(mockEvent);

      expect(mockStore.dispatch).toHaveBeenCalledWith(showCustomDetailsEvent({ event: mockEvent }));
    });
  });

  describe('trackById', () => {
    it('should return the id of the event', () => {
      const mockEvent: CustomEvent = {
        id: 'event-1',
        key: mockSlotKey,
        creationDateUTC: new Date(),
        name: 'Event Name',
        description: 'Description',
        serializedMessage: '{}',
      };

      expect(component.trackById(0, mockEvent)).toBe(mockEvent.id);
    });
  });

  describe('template', () => {
    it('should show progress spinner when events are loading', () => {
      component.customEvents$ = of(null as unknown as CustomEvent[]);
      const cdr = fixture.debugElement.injector.get<ChangeDetectorRef>(ChangeDetectorRef);
      cdr.detectChanges();

      const spinner = fixture.debugElement.query(By.css('mat-progress-spinner'));
      expect(spinner).toBeTruthy();
    });
  });
});
