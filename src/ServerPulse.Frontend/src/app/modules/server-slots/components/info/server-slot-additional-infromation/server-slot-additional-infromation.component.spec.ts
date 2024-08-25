import { CdkVirtualScrollViewport, ScrollingModule } from '@angular/cdk/scrolling';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { ComponentFixture, fakeAsync, TestBed, tick } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { of } from 'rxjs';
import { CustomEventResponse } from '../../../../shared';
import { ServerSlotDialogManager, ServerStatisticsService } from '../../../index';
import { ServerSlotAdditionalInfromationComponent } from './server-slot-additional-infromation.component';

describe('ServerSlotAdditionalInfromationComponent', () => {
  let component: ServerSlotAdditionalInfromationComponent;
  let fixture: ComponentFixture<ServerSlotAdditionalInfromationComponent>;
  let mockStatisticsService: jasmine.SpyObj<ServerStatisticsService>;
  let mockDialogManager: jasmine.SpyObj<ServerSlotDialogManager>;

  const mockCustomEventResponse: CustomEventResponse[] = [
    { id: '1', key: 'key', name: 'Test Event 1', description: 'Test Description 1', serializedMessage: 'Test Serialized Message 1', creationDateUTC: new Date() },
    { id: '2', key: 'key', name: 'Test Event 2', description: 'Test Description 2', serializedMessage: 'Test Serialized Message 2', creationDateUTC: new Date() }
  ];

  const mockServerLoadStatisticsResponse = {
    key: 'test-slot-key',
    statistics: {
      isInitial: false,
      amountOfEvents: 10,
      collectedDateUTC: new Date(),
      lastEvent: null,
      loadMethodStatistics: {
        getAmount: 100,
        postAmount: 200,
        putAmount: 50,
        patchAmount: 30,
        deleteAmount: 20,
        collectedDateUTC: new Date(),
        isInitial: false,
      }
    }
  };

  const mockCustomEventStatisticsResponse = {
    key: 'test-slot-key',
    statistics: {
      lastEvent: mockCustomEventResponse[0],
      collectedDateUTC: new Date(),
      isInitial: false,
    }
  };

  beforeEach(async () => {
    mockStatisticsService = jasmine.createSpyObj('ServerStatisticsService', [
      'getLastServerLoadStatistics',
      'getSomeCustomEventsFromDate',
      'getLastCustomStatistics'
    ]);
    mockDialogManager = jasmine.createSpyObj('ServerSlotDialogManager', ['openCustomEventDetails']);

    await TestBed.configureTestingModule({
      declarations: [ServerSlotAdditionalInfromationComponent],
      providers: [
        { provide: ServerStatisticsService, useValue: mockStatisticsService },
        { provide: ServerSlotDialogManager, useValue: mockDialogManager }
      ],
      imports: [ScrollingModule],
      schemas: [CUSTOM_ELEMENTS_SCHEMA]
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ServerSlotAdditionalInfromationComponent);
    component = fixture.componentInstance;
    component.slotKey = 'test-slot-key';

    mockStatisticsService.getLastServerLoadStatistics.and.returnValue(of(mockServerLoadStatisticsResponse));
    mockStatisticsService.getSomeCustomEventsFromDate.and.returnValue(of(mockCustomEventResponse));
    mockStatisticsService.getLastCustomStatistics.and.returnValue(of(mockCustomEventStatisticsResponse));

    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should update fetchDateSubject$ when triggerDataFetch is called', () => {
    const initialDate = new Date();
    const laterDate = new Date(initialDate.getTime() + 1000);
    const earlierDate = new Date(initialDate.getTime() - 1000);
    const mockResponse = {
      id: "id",
      key: "key",
      creationDateUTC: new Date(),
      name: "name",
      description: "description",
      serializedMessage: "serializedMessage",
    };

    component['dataSourceSubject$'].next([mockResponse]);
    component['fetchDateSubject$'].next(initialDate);

    component["triggerDataFetch"]();

    let difference = Math.abs(component['fetchDateSubject$'].value.getTime() - laterDate.getTime());
    expect(difference).toBeLessThan(5000);

    component['dataSourceSubject$'].next([mockResponse]);
    component['fetchDateSubject$'].next(initialDate);

    component["triggerDataFetch"]();

    difference = Math.abs(component['fetchDateSubject$'].value.getTime() - initialDate.getTime());
    expect(difference).toBeLessThan(5000);

    component['dataSourceSubject$'].next([mockResponse]);
    component['fetchDateSubject$'].next(initialDate);

    component["triggerDataFetch"]();

    difference = Math.abs(component['fetchDateSubject$'].value.getTime() - earlierDate.getTime());
    expect(difference).toBeLessThan(5000);
  });

  it('should initialize data chart fetching on ngOnInit', () => {
    component.ngOnInit();

    expect(mockStatisticsService.getLastServerLoadStatistics).toHaveBeenCalledWith('test-slot-key');
    component.chartData$.subscribe(data => {
      expect(data.get('GET')).toBe(100);
      expect(data.get('POST')).toBe(200);
    });
  });

  it('should initialize fetching table items subscription on ngOnInit', fakeAsync(() => {
    component.ngOnInit();
    tick();
    fixture.detectChanges();

    expect(mockStatisticsService.getSomeCustomEventsFromDate).toHaveBeenCalledWith('test-slot-key', 12, jasmine.any(Date), false);
    component.dataSource$.subscribe(data => {
      expect(data.length).toBe(2);
    });
  }));

  it('should monitor scroll and trigger data fetch', fakeAsync(() => {
    const scroller = fixture.debugElement.query(By.directive(CdkVirtualScrollViewport)).injector.get(CdkVirtualScrollViewport);
    spyOn(scroller, 'measureScrollOffset').and.returnValue(10);
    spyOn(scroller.elementScrolled(), 'pipe').and.callThrough();

    component.ngAfterViewInit();
    tick(200);

    expect(scroller.elementScrolled().pipe).toHaveBeenCalled();
    expect(mockStatisticsService.getSomeCustomEventsFromDate).toHaveBeenCalled();
  }));

  it('should open detail menu on button click', () => {
    const serializedMessage = 'Test Serialized Message 1';
    component.openDetailMenu(serializedMessage);

    expect(mockDialogManager.openCustomEventDetails).toHaveBeenCalledWith(serializedMessage);
  });

  it('should clean up on destroy', () => {
    spyOn(component['destroy$'], 'next');
    spyOn(component['destroy$'], 'complete');

    component.ngOnDestroy();

    expect(component['destroy$'].next).toHaveBeenCalled();
    expect(component['destroy$'].complete).toHaveBeenCalled();
  });

  it('should track items by ID', () => {
    const result = component.trackById(0, mockCustomEventResponse[0]);
    expect(result).toBe('1');
  });
});