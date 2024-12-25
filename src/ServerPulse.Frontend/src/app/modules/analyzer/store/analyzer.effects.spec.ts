/* eslint-disable @typescript-eslint/no-explicit-any */
import { TestBed } from '@angular/core/testing';
import { provideMockActions } from '@ngrx/effects/testing';
import { Store } from "@ngrx/store";
import { Observable, of, Subject, throwError } from "rxjs";
import { AnalyzerApiService, BaseStatisticsResponse, getDefaultServerLifecycleStatisticsResponse, mapServerLifecycleStatisticsResponseToServerLifecycleStatistics, receiveLifecycleStatisticsFailure, receiveLifecycleStatisticsSuccess, SignalStatisticsService, startLifecycleStatisticsReceiving, stopLifecycleStatisticsReceiving } from "..";
import { SnackbarManager } from "../../shared";
import { AnalyzerEffects } from "./analyzer.effects";

describe('AnalyzerEffects', () => {
    let actions$: Observable<any>;
    let effects: AnalyzerEffects;

    let storeSpy: jasmine.SpyObj<Store>;
    let apiServiceSpy: jasmine.SpyObj<AnalyzerApiService>;
    let snackbarManagerSpy: jasmine.SpyObj<SnackbarManager>;
    let signalStatisticsSpy: jasmine.SpyObj<SignalStatisticsService>;
    const token = 'token';

    beforeEach(() => {
        storeSpy = jasmine.createSpyObj<Store>(['select', 'dispatch']);
        apiServiceSpy = jasmine.createSpyObj<AnalyzerApiService>(['getLoadAmountStatisticsInRange', 'getSlotStatistics']);
        snackbarManagerSpy = jasmine.createSpyObj<SnackbarManager>(['openErrorSnackbar']);
        signalStatisticsSpy = jasmine.createSpyObj<SignalStatisticsService>(['startConnection', 'startListen', 'stopListen', 'receiveStatistics']);

        const authData = { authToken: { accessToken: token } };
        storeSpy.select.and.returnValue(of(authData));

        TestBed.configureTestingModule({
            providers: [
                AnalyzerEffects,
                provideMockActions(() => actions$),
                { provide: Store, useValue: storeSpy },
                { provide: AnalyzerApiService, useValue: apiServiceSpy },
                { provide: SnackbarManager, useValue: snackbarManagerSpy },
                { provide: SignalStatisticsService, useValue: signalStatisticsSpy },
            ],
        });

        effects = TestBed.inject(AnalyzerEffects);
    });

    it('should be created', () => {
        expect(effects).toBeTruthy();
    });

    //#region Lifecycle Statistics

    describe('startLifecycleStatisticsReceiving$', () => {
        it('should start connection, listen, and add listener', () => {
            const action = startLifecycleStatisticsReceiving({ key: 'test-key' });
            const hubUrl = 'test-hub-url';

            const response = getDefaultServerLifecycleStatisticsResponse();
            const expecteStatistics = mapServerLifecycleStatisticsResponseToServerLifecycleStatistics(response);

            signalStatisticsSpy.startConnection.and.returnValue(of());
            signalStatisticsSpy.receiveStatistics.and.returnValue(
                of({
                    key: 'test-key',
                    response: getDefaultServerLifecycleStatisticsResponse(),
                }) as unknown as Subject<{ key: string; response: BaseStatisticsResponse }>
            );

            actions$ = of(action);

            effects.startLifecycleStatisticsReceiving$.subscribe((result) => {
                expect(signalStatisticsSpy.startConnection).toHaveBeenCalledWith(hubUrl, token);
                expect(signalStatisticsSpy.startListen).toHaveBeenCalledWith(hubUrl, 'test-key', true);
                expect(result).toEqual(
                    receiveLifecycleStatisticsSuccess({
                        key: 'test-key',
                        statistics: expecteStatistics,
                    })
                );
            });
        });

        it('should skip duplicate listeners', () => {
            const action = startLifecycleStatisticsReceiving({ key: 'test-key' });
            effects['activeListeners'].lifecycle.add('test-key');

            actions$ = of(action);

            effects.startLifecycleStatisticsReceiving$.subscribe(() => {
                expect(signalStatisticsSpy.startConnection).not.toHaveBeenCalled();
                expect(signalStatisticsSpy.startListen).not.toHaveBeenCalled();
            });
        });

        it('should handle connection errors gracefully', () => {
            const authData = { authToken: { accessToken: 'token' } };
            const action = startLifecycleStatisticsReceiving({ key: 'test-key' });
            const error = new Error('Connection failed');

            storeSpy.select.and.returnValue(of(authData));
            signalStatisticsSpy.startConnection.and.returnValue(throwError(() => error));

            actions$ = of(action);

            effects.startLifecycleStatisticsReceiving$.subscribe((result) => {
                expect(result).toEqual(
                    receiveLifecycleStatisticsFailure({ error: error.message })
                );
            });
        });
    });

    describe('receiveLifecycleStatisticsFailure$', () => {
        it('should display an error snackbar', () => {
            const action = receiveLifecycleStatisticsFailure({ error: 'Test error' });

            actions$ = of(action);

            effects.receiveLifecycleStatisticsFailure$.subscribe(() => {
                expect(snackbarManagerSpy.openErrorSnackbar).toHaveBeenCalledWith([
                    'Failed to receive statistics in lifecycle statistics hub: Test error',
                ]);
            });
        });
    });

    describe('stopLifecycleStatisticsReceiving$', () => {
        it('should stop listening and remove the listener', () => {
            const action = stopLifecycleStatisticsReceiving({ key: 'test-key' });

            actions$ = of(action);

            effects.stopLifecycleStatisticsReceiving$.subscribe(() => {
                expect(signalStatisticsSpy.stopListen).toHaveBeenCalledWith('test-hub-url', 'test-key');
                expect(effects['activeListeners'].lifecycle.has('test-key')).toBeFalse();
            });
        });
    });

    //#endregion
});