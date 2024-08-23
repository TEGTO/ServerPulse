import { CdkVirtualScrollViewport, ScrollingModule } from '@angular/cdk/scrolling';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { ComponentFixture, fakeAsync, TestBed, tick } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { BehaviorSubject, of } from 'rxjs';
import { TimeSpan } from '../../../shared';
import { ServerStatisticsService } from '../../index';
import { ServerSlotInfoStatsComponent } from './server-slot-info-stats.component';

describe('ServerSlotInfoStatsComponent', () => {
  let component: ServerSlotInfoStatsComponent;
  let fixture: ComponentFixture<ServerSlotInfoStatsComponent>;
  let mockStatisticsService: jasmine.SpyObj<ServerStatisticsService>;
  let dataSourceSubject$: BehaviorSubject<any[]>;
  let fetchDateSubject$: BehaviorSubject<Date>;
  let areTableItemsLoadingSubject$: BehaviorSubject<boolean>;

  beforeEach(async () => {
    mockStatisticsService = jasmine.createSpyObj('ServerStatisticsService', [
      'getLastServerStatistics',
      'getSomeLoadEventsFromDate',
      'getCurrentLoadStatisticsDate',
      'getLastServerLoadStatistics',
    ]);

    dataSourceSubject$ = new BehaviorSubject<any[]>([]);
    fetchDateSubject$ = new BehaviorSubject<Date>(new Date());
    areTableItemsLoadingSubject$ = new BehaviorSubject<boolean>(false);

    await TestBed.configureTestingModule({
      declarations: [ServerSlotInfoStatsComponent],
      imports: [ScrollingModule],
      providers: [
        { provide: ServerStatisticsService, useValue: mockStatisticsService },
      ],
      schemas: [CUSTOM_ELEMENTS_SCHEMA],
    }).compileComponents();
  });

  beforeEach(() => {

    mockStatisticsService.getCurrentLoadStatisticsDate.and.returnValue(of(new Date()));

    mockStatisticsService.getLastServerLoadStatistics.and.returnValue(of(
      {
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
      }
    ));

    mockStatisticsService.getLastServerStatistics.and.returnValue(of(
      {
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
      }
    ));

    mockStatisticsService.getSomeLoadEventsFromDate.and.returnValue(of(
      [
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
      ]
    ));

    fixture = TestBed.createComponent(ServerSlotInfoStatsComponent);
    component = fixture.componentInstance;
    component.slotKey = 'testSlotKey';

    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
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
    expect(tableRows.length).toBe(1);
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
});