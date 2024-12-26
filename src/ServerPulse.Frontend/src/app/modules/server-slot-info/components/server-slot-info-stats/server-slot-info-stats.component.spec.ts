/* eslint-disable @typescript-eslint/no-explicit-any */
import { CdkVirtualScrollViewport, ScrollingModule } from '@angular/cdk/scrolling';
import { ChangeDetectorRef, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { ComponentFixture, fakeAsync, TestBed, tick } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { MemoizedSelector, Store } from '@ngrx/store';
import { BehaviorSubject, of, Subject } from 'rxjs';
import { addNewLoadEvent, getSomeLoadEvents, selectSelectedDate, setReadFromDate, SlotInfoState } from '../..';
import { getDefaultServerLifecycleStatistics, LoadEvent } from '../../../analyzer';
import { LocalizedDatePipe, TimeSpan } from '../../../shared';
import { ServerSlotInfoStatsComponent } from './server-slot-info-stats.component';

fdescribe('ServerSlotInfoStatsComponent', () => {
  let component: ServerSlotInfoStatsComponent;
  let fixture: ComponentFixture<ServerSlotInfoStatsComponent>;
  let mockStore: jasmine.SpyObj<Store>;
  let destroy$: Subject<void>;
  let storeSelectSubject$: BehaviorSubject<any>;

  beforeEach(() => {
    mockStore = jasmine.createSpyObj<Store>('Store', ['dispatch', 'select']);
    destroy$ = new Subject<void>();
    storeSelectSubject$ = new BehaviorSubject<any>(null);

    mockStore.select.and.returnValue(
      storeSelectSubject$.asObservable()
    )

    TestBed.configureTestingModule({
      declarations: [ServerSlotInfoStatsComponent],
      imports: [ScrollingModule, LocalizedDatePipe],
      providers: [
        { provide: Store, useValue: mockStore },
      ],
      schemas: [CUSTOM_ELEMENTS_SCHEMA]
    }).compileComponents();

    fixture = TestBed.createComponent(ServerSlotInfoStatsComponent);
    component = fixture.componentInstance;

    component.slotKey = 'test-slot-key';

    fixture.detectChanges();
  });

  afterEach(() => {
    destroy$.next();
    destroy$.complete();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should set server status if there is no statistics to "No Data"', () => {
    component.ngOnInit();
    fixture.detectChanges();

    expect(component.serverStatus).toBe('No Data');
  });

  it('should set server status based on lifecycle statistics', fakeAsync(() => {
    storeSelectSubject$.next({ ...getDefaultServerLifecycleStatistics(), dataExists: true, isAlive: true });

    component.lifecycleStatistics$.subscribe();

    component.ngOnInit();
    fixture.detectChanges();

    expect(component.serverStatus).toBe('Online');

    storeSelectSubject$.next({ ...getDefaultServerLifecycleStatistics(), dataExists: true, isAlive: false });

    fixture.detectChanges();

    expect(component.serverStatus).toBe('Offline');
  }));

  it('should fetch load events on initialization', () => {
    component.ngOnInit();

    expect(mockStore.dispatch).toHaveBeenCalledWith(jasmine.objectContaining({
      type: getSomeLoadEvents.type
    }));
  });

  it('should dispatch addNewLoadEvent if the last load event exists and date is today', () => {
    const mockEvent: LoadEvent = {
      id: 'event-1',
      key: 'test-slot-key',
      endpoint: '/api/data',
      method: 'GET',
      statusCode: 200,
      duration: new TimeSpan(0, 0, 0, 50),
      timestampUTC: new Date(),
      creationDateUTC: new Date(),
    };

    mockStore.select.and.callFake((selector: MemoizedSelector<object, Date, (s1: SlotInfoState) => Date>) => {
      if (selector === selectSelectedDate) {
        return of(new Date());
      }
      return of(mockEvent);
    });

    storeSelectSubject$.next(mockEvent);

    component.ngOnInit();

    expect(mockStore.dispatch).toHaveBeenCalledWith(addNewLoadEvent({ event: mockEvent }));
  });

  it('should monitor scroll for fetching data', fakeAsync(() => {
    const scrollOffset$ = new BehaviorSubject<number>(10);
    component.scroller = {
      elementScrolled: () => scrollOffset$.asObservable(),
      measureScrollOffset: () => 10,
    } as unknown as CdkVirtualScrollViewport;

    component.loadEvents$ = of([
      {
        id: '1',
        key: 'test-slot-key',
        creationDateUTC: new Date(),
      } as LoadEvent,
    ]);

    spyOn<any>(component, 'monitorScrollForFetching').and.callThrough();

    component.ngAfterViewInit();
    scrollOffset$.next(5);

    tick();

    expect(component['monitorScrollForFetching']).toHaveBeenCalled();
    expect(mockStore.dispatch).toHaveBeenCalledWith(jasmine.objectContaining({
      type: setReadFromDate.type
    }));
  }));

  fit('should render lifecycle statistics', () => {
    storeSelectSubject$.next({ ...getDefaultServerLifecycleStatistics(), dataExists: true, isAlive: true });

    const cdr = fixture.debugElement.injector.get<ChangeDetectorRef>(ChangeDetectorRef);
    cdr.detectChanges();

    const statusElement = fixture.debugElement.query(By.css('.left__status'));
    expect(statusElement.nativeElement.textContent).toContain('Online');

    const loadAmountElement = fixture.debugElement.query(By.css('.left__last-load-amount'));
    expect(loadAmountElement.nativeElement.textContent).toContain('20');
  });

});
