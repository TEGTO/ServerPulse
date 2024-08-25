import { CdkVirtualScrollViewport, ScrollingModule } from '@angular/cdk/scrolling';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { ComponentFixture, fakeAsync, TestBed, tick } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { of, Subject } from 'rxjs';
import { LocalizedDatePipe, TimeSpan } from '../../../../shared';
import { ServerStatisticsService } from '../../../index';
import { ServerSlotInfoStatsComponent } from './server-slot-info-stats.component';

describe('ServerSlotInfoStatsComponent', () => {
  let component: ServerSlotInfoStatsComponent;
  let fixture: ComponentFixture<ServerSlotInfoStatsComponent>;
  let mockStatisticsService: jasmine.SpyObj<ServerStatisticsService>;
  let scroller: jasmine.SpyObj<CdkVirtualScrollViewport>;
  let scrollEvents: Subject<Event>;

  beforeEach(async () => {
    mockStatisticsService = jasmine.createSpyObj('ServerStatisticsService', [
      'getLastServerStatistics',
      'getSomeLoadEventsFromDate',
      'getCurrentLoadStatisticsDate',
      'getLastServerLoadStatistics',
    ]);

    await TestBed.configureTestingModule({
      declarations: [ServerSlotInfoStatsComponent, LocalizedDatePipe],
      imports: [ScrollingModule],
      providers: [
        { provide: ServerStatisticsService, useValue: mockStatisticsService },
      ],
      schemas: [CUSTOM_ELEMENTS_SCHEMA],
    }).compileComponents();
  });

  beforeEach(() => {
    mockStatisticsService.getCurrentLoadStatisticsDate.and.returnValue(of(new Date()));
    mockStatisticsService.getLastServerLoadStatistics.and.returnValue(of({
      key: 'test-slot-key',
      statistics: {
        lastEvent: null,
        isInitial: false,
        amountOfEvents: 10,
        collectedDateUTC: new Date(),
        loadMethodStatistics: null
      }
    }));

    mockStatisticsService.getLastServerStatistics.and.returnValue(of({
      key: 'test-slot-key',
      statistics: {
        lastEvent: null,
        isInitial: false,
        amountOfEvents: 10,
        collectedDateUTC: new Date(),
        isAlive: true,
        dataExists: true,
        serverLastStartDateTimeUTC: new Date(),
        serverUptime: new TimeSpan(),
        lastServerUptime: new TimeSpan(),
        lastPulseDateTimeUTC: new Date(),
      }
    }));

    mockStatisticsService.getSomeLoadEventsFromDate.and.returnValue(of([
      {
        id: '1',
        key: "",
        endpoint: '/api/test',
        method: 'GET',
        statusCode: 200,
        duration: new TimeSpan(),
        creationDateUTC: new Date(),
        timestampUTC: new Date()
      },
    ]));
    scrollEvents = new Subject<Event>();
    scroller = jasmine.createSpyObj('CdkVirtualScrollViewport', ['elementScrolled', 'measureScrollOffset']);
    scroller.elementScrolled.and.returnValue(scrollEvents.asObservable());

    fixture = TestBed.createComponent(ServerSlotInfoStatsComponent);
    component = fixture.componentInstance;
    component.slotKey = 'testSlotKey';
    component.scroller = scroller;

    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should update fetchDateSubject$ when triggerDataFetch is called', () => {
    const initialDate = new Date();
    const laterDate = new Date(initialDate.getTime() + 1000);
    const earlierDate = new Date(initialDate.getTime() - 1000);
    const mockServerLoadResponse = {
      id: "id",
      key: "key",
      creationDateUTC: new Date(),
      endpoint: "endpoint",
      method: "method",
      statusCode: 200,
      duration: new TimeSpan(),
      timestampUTC: new Date()
    };

    component['dataSourceSubject$'].next([mockServerLoadResponse]);
    component['fetchDateSubject$'].next(initialDate);

    component["triggerDataFetch"]();

    let difference = Math.abs(component['fetchDateSubject$'].value.getTime() - laterDate.getTime());
    expect(difference).toBeLessThan(5000);

    component['dataSourceSubject$'].next([mockServerLoadResponse]);
    component['fetchDateSubject$'].next(initialDate);

    component["triggerDataFetch"]();

    difference = Math.abs(component['fetchDateSubject$'].value.getTime() - initialDate.getTime());
    expect(difference).toBeLessThan(5000);

    component['dataSourceSubject$'].next([mockServerLoadResponse]);
    component['fetchDateSubject$'].next(initialDate);

    component["triggerDataFetch"]();

    difference = Math.abs(component['fetchDateSubject$'].value.getTime() - earlierDate.getTime());
    expect(difference).toBeLessThan(5000);
  });

  it('should initialize statistics subscription', fakeAsync(() => {
    component.ngOnInit();
    fixture.detectChanges();

    component.serverStatus$.subscribe(status => {
      expect(status).toBe('Online');
    });

    tick();
  }));

  it('should fetch and display table items', async () => {
    component['dataSourceSubject$'].next([]);
    component.ngOnInit();
    component['dataSourceSubject$'].next([
      {
        id: '1',
        key: "",
        endpoint: '/api/test',
        method: 'GET',
        statusCode: 200,
        duration: new TimeSpan(),
        creationDateUTC: new Date(),
        timestampUTC: new Date()
      }
    ]);
    await fixture.whenStable();
    fixture.detectChanges();

    const tableRows = fixture.debugElement.queryAll(By.css('tbody tr'));
    expect(tableRows.length).toBeGreaterThan(0);
    expect(tableRows[0].nativeElement.textContent).toContain('/api/test');
  });

  it('should display the correct server status', fakeAsync(() => {
    component.ngOnInit();
    fixture.detectChanges();
    tick();

    const statusElement = fixture.debugElement.query(By.css('.left__status'));
    expect(statusElement.nativeElement.textContent).toContain('Online');
  }));

  it('should correctly render server start time', fakeAsync(() => {
    component.ngOnInit();
    fixture.detectChanges();
    tick();

    const startDateElement = fixture.debugElement.query(By.css('.left__uptime'));
    expect(startDateElement.nativeElement.textContent).toContain('Last start:');
  }));

  it('should handle scrolling and fetch new data', fakeAsync(() => {
    component.ngOnInit();
    fixture.detectChanges();
    tick();

    const scroller = fixture.debugElement.query(By.directive(CdkVirtualScrollViewport));
    scroller.triggerEventHandler('scroll', null);

    fixture.detectChanges();
    tick();

    expect(mockStatisticsService.getSomeLoadEventsFromDate).toHaveBeenCalled();
  }));

  it('should render the loading spinner when fetching table items', () => {
    component["areTableItemsLoadingSubject$"].next(true);
    fixture.detectChanges();

    let spinner = fixture.debugElement.query(By.css('mat-progress-spinner'));
    expect(spinner).toBeTruthy();

    component["areTableItemsLoadingSubject$"].next(false);
    fixture.detectChanges();

    spinner = fixture.debugElement.query(By.css('mat-progress-spinner'));
    expect(spinner).toBeFalsy();
  });

  it('should clean up subscriptions on destroy', () => {
    spyOn(component['destroy$'], 'next');
    spyOn(component['destroy$'], 'complete');
    component.ngOnDestroy();
    expect(component['destroy$'].next).toHaveBeenCalled();
    expect(component['destroy$'].complete).toHaveBeenCalled();
  });
});