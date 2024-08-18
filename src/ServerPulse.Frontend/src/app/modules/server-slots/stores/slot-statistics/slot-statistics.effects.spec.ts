import { HttpClientTestingModule } from "@angular/common/http/testing";
import { TestBed } from "@angular/core/testing";
import { provideMockActions } from "@ngrx/effects/testing";
import { Observable, of, throwError } from "rxjs";
import { environment } from "../../../../../environment/environment";
import { StatisticsApiService } from "../../../shared";
import { RealTimeStatisticsCollector, subscribeToLoadStatistics, subscribeToLoadStatisticsFailure, subscribeToLoadStatisticsSuccess, subscribeToSlotStatistics, subscribeToSlotStatisticsFailure, subscribeToSlotStatisticsSuccess } from "../../index";
import { ServerSlotStatisticsEffects } from "./slot-statistics.effects";

describe('ServerSlotStatisticsEffects', () => {
    let actions$: Observable<any>;
    let effects: ServerSlotStatisticsEffects;
    let mockStatisticsCollector: jasmine.SpyObj<RealTimeStatisticsCollector>;
    let mockStatisticsApiService: jasmine.SpyObj<StatisticsApiService>;

    beforeEach(() => {
        mockStatisticsCollector = jasmine.createSpyObj('RealTimeStatisticsCollector', [
            'startConnection',
            'startListen',
            'receiveStatistics'
        ]);

        mockStatisticsApiService = jasmine.createSpyObj('StatisticsApiService', ['someMethod']);

        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule],
            providers: [
                ServerSlotStatisticsEffects,
                provideMockActions(() => actions$),
                { provide: RealTimeStatisticsCollector, useValue: mockStatisticsCollector },
                { provide: StatisticsApiService, useValue: mockStatisticsApiService },
            ]
        });

        effects = TestBed.inject(ServerSlotStatisticsEffects);
    });

    describe('subscribeToSlotStatistics$', () => {
        it('should dispatch subscribeToSlotStatisticsSuccess on successful statistics subscription', (done) => {
            const mockData = { key: 'test-key', data: 'some-data' };
            const action = subscribeToSlotStatistics({ slotKey: 'test-slot-key' });
            const outcome = subscribeToSlotStatisticsSuccess({ lastStatistics: mockData });

            actions$ = of(action);
            mockStatisticsCollector.startConnection.and.returnValue(of(undefined));
            mockStatisticsCollector.receiveStatistics.and.returnValue(of(mockData));

            effects.subscribeToSlotStatistics$.subscribe(result => {
                expect(result).toEqual(outcome);
                expect(mockStatisticsCollector.startConnection).toHaveBeenCalledWith(environment.statisticsHub);
                expect(mockStatisticsCollector.startListen).toHaveBeenCalledWith(environment.statisticsHub, 'test-slot-key');
                expect(mockStatisticsCollector.receiveStatistics).toHaveBeenCalledWith(environment.statisticsHub);
                done();
            });
        });

        it('should dispatch subscribeToSlotStatisticsFailure on error', (done) => {
            const action = subscribeToSlotStatistics({ slotKey: 'test-slot-key' });
            const error = new Error('Connection failed');
            const outcome = subscribeToSlotStatisticsFailure({ error });

            actions$ = of(action);
            mockStatisticsCollector.startConnection.and.returnValue(throwError(error));

            effects.subscribeToSlotStatistics$.subscribe(result => {
                expect(result).toEqual(outcome);
                expect(mockStatisticsCollector.startConnection).toHaveBeenCalledWith(environment.statisticsHub);
                expect(mockStatisticsCollector.startListen).not.toHaveBeenCalled();
                expect(mockStatisticsCollector.receiveStatistics).not.toHaveBeenCalled();
                done();
            });
        });
    });

    describe('subscribeToLoadStatistics$', () => {
        it('should dispatch subscribeToLoadStatisticsSuccess on successful subscription', (done) => {
            const mockData = { key: 'test-key', data: 'some-load-data' };
            const action = subscribeToLoadStatistics({ slotKey: 'test-slot-key' });
            const outcome = subscribeToLoadStatisticsSuccess({ lastLoadStatistics: mockData });

            actions$ = of(action);
            mockStatisticsCollector.startConnection.and.returnValue(of(undefined));
            mockStatisticsCollector.receiveStatistics.and.returnValue(of(mockData));

            effects.subscribeToLoadStatistics$.subscribe(result => {
                expect(result).toEqual(outcome);
                expect(mockStatisticsCollector.startConnection).toHaveBeenCalledWith(environment.loadStatisticsHub);
                expect(mockStatisticsCollector.startListen).toHaveBeenCalledWith(environment.loadStatisticsHub, 'test-slot-key');
                expect(mockStatisticsCollector.receiveStatistics).toHaveBeenCalledWith(environment.loadStatisticsHub);
                done();
            });
        });

        it('should dispatch subscribeToLoadStatisticsFailure on error', (done) => {
            const action = subscribeToLoadStatistics({ slotKey: 'test-slot-key' });
            const error = new Error('Connection failed');
            const outcome = subscribeToLoadStatisticsFailure({ error });

            actions$ = of(action);
            mockStatisticsCollector.startConnection.and.returnValue(throwError(error));

            effects.subscribeToLoadStatistics$.subscribe(result => {
                expect(result).toEqual(outcome);
                expect(mockStatisticsCollector.startConnection).toHaveBeenCalledWith(environment.loadStatisticsHub);
                expect(mockStatisticsCollector.startListen).not.toHaveBeenCalled();
                expect(mockStatisticsCollector.receiveStatistics).not.toHaveBeenCalled();
                done();
            });
        });
    });
});