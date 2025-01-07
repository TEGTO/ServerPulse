/* eslint-disable @typescript-eslint/no-explicit-any */
import { fakeAsync, TestBed, tick } from '@angular/core/testing';
import { provideMockActions } from '@ngrx/effects/testing';
import { Store } from "@ngrx/store";
import { Observable, of, Subject, throwError } from "rxjs";
import { AnalyzerApiService, BaseStatisticsResponse, downloadSlotStatistics, downLoadSlotStatisticsFailure, getDefaultCustomStatisticsResponse, getDefaultLoadAmountStatistics, getDefaultLoadStatisticsResponse, getDefaultServerLifecycleStatisticsResponse, getDefaultSlotStatistics, getLoadAmountStatisticsInRange, getLoadAmountStatisticsInRangeFailure, getLoadAmountStatisticsInRangeSuccess, LoadAmountStatistics, mapServerCustomStatisticsResponseToServerCustomStatistics, mapServerLifecycleStatisticsResponseToServerLifecycleStatistics, mapServerLoadStatisticsResponseToServerLoadStatistics, MessageAmountInRangeRequest, receiveCustomStatisticsFailure, receiveCustomStatisticsSuccess, receiveLifecycleStatisticsFailure, receiveLifecycleStatisticsSuccess, receiveLoadStatisticsFailure, receiveLoadStatisticsSuccess, SignalStatisticsService, startCustomStatisticsReceiving, startLifecycleStatisticsReceiving, startLoadStatisticsReceiving, stopCustomStatisticsReceiving, stopLifecycleStatisticsReceiving, stopLoadKeyListening } from "..";
import { JsonDownloader, SnackbarManager, TimeSpan } from "../../shared";
import { AnalyzerEffects } from "./analyzer.effects";

describe('AnalyzerEffects', () => {
    let actions$: Observable<any>;
    let effects: AnalyzerEffects;

    let storeSpy: jasmine.SpyObj<Store>;
    let apiServiceSpy: jasmine.SpyObj<AnalyzerApiService>;
    let snackbarManagerSpy: jasmine.SpyObj<SnackbarManager>;
    let signalStatisticsSpy: jasmine.SpyObj<SignalStatisticsService>;
    let jsonDownloaderSpy: jasmine.SpyObj<JsonDownloader>;

    const token = 'token';

    beforeEach(() => {
        storeSpy = jasmine.createSpyObj<Store>(['select', 'dispatch']);
        apiServiceSpy = jasmine.createSpyObj<AnalyzerApiService>(['getLoadAmountStatisticsInRange', 'getSlotStatistics']);
        snackbarManagerSpy = jasmine.createSpyObj<SnackbarManager>(['openErrorSnackbar']);
        signalStatisticsSpy = jasmine.createSpyObj<SignalStatisticsService>(['startConnection', 'startListen', 'stopListen', 'receiveStatistics']);
        jsonDownloaderSpy = jasmine.createSpyObj<JsonDownloader>(['downloadInJson']);

        const authData = { accessTokenData: { accessToken: token } };
        storeSpy.select.and.returnValue(of(authData));

        signalStatisticsSpy.startConnection.and.returnValue(of(undefined));

        TestBed.configureTestingModule({
            providers: [
                AnalyzerEffects,
                provideMockActions(() => actions$),
                { provide: Store, useValue: storeSpy },
                { provide: AnalyzerApiService, useValue: apiServiceSpy },
                { provide: SnackbarManager, useValue: snackbarManagerSpy },
                { provide: SignalStatisticsService, useValue: signalStatisticsSpy },
                { provide: JsonDownloader, useValue: jsonDownloaderSpy }
            ],
        });

        effects = TestBed.inject(AnalyzerEffects);
    });

    it('should be created', () => {
        expect(effects).toBeTruthy();
    });

    //#region Lifecycle Statistics

    describe('startLifecycleStatisticsReceiving$', () => {
        it('should start connection, listen, and add listener', fakeAsync(() => {
            const action = startLifecycleStatisticsReceiving({ key: 'test-key' });

            const response = getDefaultServerLifecycleStatisticsResponse();
            const expecteStatistics = mapServerLifecycleStatisticsResponseToServerLifecycleStatistics(response);

            signalStatisticsSpy.receiveStatistics.and.returnValue(
                of({
                    key: 'test-key',
                    response: response,
                }) as unknown as Subject<{ key: string; response: BaseStatisticsResponse }>
            );

            actions$ = of(action);

            effects.startLifecycleStatisticsReceiving$.subscribe((result) => {
                expect(result).toEqual(
                    receiveLifecycleStatisticsSuccess({
                        key: 'test-key',
                        statistics: expecteStatistics,
                    })
                );
            });

            tick();

            expect(signalStatisticsSpy.startConnection).toHaveBeenCalled();
            expect(signalStatisticsSpy.startListen).toHaveBeenCalled();
        }));

        it('should skip duplicate listeners', fakeAsync(() => {
            const action = startLifecycleStatisticsReceiving({ key: 'test-key' });
            effects['activeListeners'].lifecycle.add('test-key');

            actions$ = of(action);

            effects.startLifecycleStatisticsReceiving$.subscribe();

            tick();

            expect(signalStatisticsSpy.startConnection).not.toHaveBeenCalled();
            expect(signalStatisticsSpy.startListen).not.toHaveBeenCalled();
        }));

        it('should handle connection errors gracefully', () => {
            const action = startLifecycleStatisticsReceiving({ key: 'test-key' });
            const error = new Error('Connection failed');

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
        it('should display an error snackbar', fakeAsync(() => {
            const action = receiveLifecycleStatisticsFailure({ error: 'Test error' });

            actions$ = of(action);

            effects.receiveLifecycleStatisticsFailure$.subscribe();

            tick();

            expect(snackbarManagerSpy.openErrorSnackbar).toHaveBeenCalledWith([
                'Failed to receive statistics in lifecycle statistics hub: Test error',
            ]);
        }));
    });

    describe('stopLifecycleStatisticsReceiving$', () => {
        it('should stop listening and remove the listener', fakeAsync(() => {
            const action = stopLifecycleStatisticsReceiving({ key: 'test-key' });

            actions$ = of(action);

            effects.stopLifecycleStatisticsReceiving$.subscribe();

            tick();

            expect(signalStatisticsSpy.stopListen).toHaveBeenCalled();
            expect(effects['activeListeners'].lifecycle.has('test-key')).toBeFalse();
        }));
    });

    //#endregion

    //#region Load Statistics

    describe('startLoadStatisticsReceiving$', () => {
        it('should start connection, listen, and add listener', fakeAsync(() => {
            const action = startLoadStatisticsReceiving({ key: 'test-key' });

            const response = getDefaultLoadStatisticsResponse();
            const expecteStatistics = mapServerLoadStatisticsResponseToServerLoadStatistics(response);

            signalStatisticsSpy.receiveStatistics.and.returnValue(
                of({
                    key: 'test-key',
                    response: response,
                }) as unknown as Subject<{ key: string; response: BaseStatisticsResponse }>
            );

            actions$ = of(action);

            effects.startLoadStatisticsReceiving$.subscribe((result) => {
                expect(result).toEqual(
                    receiveLoadStatisticsSuccess({
                        key: 'test-key',
                        statistics: expecteStatistics,
                    })
                );
            });

            tick(300);

            expect(signalStatisticsSpy.startConnection).toHaveBeenCalled();
            expect(signalStatisticsSpy.startListen).toHaveBeenCalled();
        }));

        it('should skip duplicate listeners', fakeAsync(() => {
            const action = startLoadStatisticsReceiving({ key: 'test-key' });
            effects['activeListeners'].load.add('test-key');

            actions$ = of(action);

            effects.startLoadStatisticsReceiving$.subscribe();

            expect(signalStatisticsSpy.startConnection).not.toHaveBeenCalled();
            expect(signalStatisticsSpy.startListen).not.toHaveBeenCalled();
        }));

        it('should handle connection errors gracefully', () => {
            const action = startLoadStatisticsReceiving({ key: 'test-key' });
            const error = new Error('Connection failed');

            signalStatisticsSpy.startConnection.and.returnValue(throwError(() => error));

            actions$ = of(action);

            effects.startLoadStatisticsReceiving$.subscribe((result) => {
                expect(result).toEqual(
                    receiveLoadStatisticsFailure({ error: error.message })
                );
            });
        });
    });

    describe('receiveLoadStatisticsFailure$', () => {
        it('should display an error snackbar', fakeAsync(() => {
            const action = receiveLoadStatisticsFailure({ error: 'Test error' });

            actions$ = of(action);

            effects.receiveLoadStatisticsFailure$.subscribe();

            tick();

            expect(snackbarManagerSpy.openErrorSnackbar).toHaveBeenCalledWith([
                'Failed to receive statistics in load statistics hub: Test error',
            ]);
        }));
    });

    describe('stopLoadStatisticsReceiving$', () => {
        it('should stop listening and remove the listener', fakeAsync(() => {
            const action = stopLoadKeyListening({ key: 'test-key' });

            actions$ = of(action);

            effects.stopLoadStatisticsReceiving$.subscribe();

            tick();

            expect(signalStatisticsSpy.stopListen).toHaveBeenCalled();
            expect(effects['activeListeners'].load.has('test-key')).toBeFalse();
        }));
    });

    //#endregion

    //#region Custom Statistics

    describe('startCustomStatisticsReceiving$', () => {
        it('should start connection, listen, and add listener', fakeAsync(() => {
            const action = startCustomStatisticsReceiving({ key: 'test-key' });

            const response = getDefaultCustomStatisticsResponse();
            const expecteStatistics = mapServerCustomStatisticsResponseToServerCustomStatistics(response);

            signalStatisticsSpy.receiveStatistics.and.returnValue(
                of({
                    key: 'test-key',
                    response: response,
                }) as unknown as Subject<{ key: string; response: BaseStatisticsResponse }>
            );

            actions$ = of(action);

            effects.startCustomStatisticsReceiving$.subscribe((result) => {
                expect(result).toEqual(
                    receiveCustomStatisticsSuccess({
                        key: 'test-key',
                        statistics: expecteStatistics,
                    })
                );
            });
            tick();

            expect(signalStatisticsSpy.startConnection).toHaveBeenCalled();
            expect(signalStatisticsSpy.startListen).toHaveBeenCalled();
        }));

        it('should skip duplicate listeners', fakeAsync(() => {
            const action = startCustomStatisticsReceiving({ key: 'test-key' });
            effects['activeListeners'].custom.add('test-key');

            actions$ = of(action);

            effects.startCustomStatisticsReceiving$.subscribe();

            tick();

            expect(signalStatisticsSpy.startConnection).not.toHaveBeenCalled();
            expect(signalStatisticsSpy.startListen).not.toHaveBeenCalled();
        }));

        it('should handle connection errors gracefully', () => {
            const action = startCustomStatisticsReceiving({ key: 'test-key' });
            const error = new Error('Connection failed');

            signalStatisticsSpy.startConnection.and.returnValue(throwError(() => error));

            actions$ = of(action);

            effects.startCustomStatisticsReceiving$.subscribe((result) => {
                expect(result).toEqual(
                    receiveCustomStatisticsFailure({ error: error.message })
                );
            });
        });
    });

    describe('receiveCustomStatisticsFailure$', () => {
        it('should display an error snackbar', fakeAsync(() => {
            const action = receiveCustomStatisticsFailure({ error: 'Test error' });

            actions$ = of(action);

            effects.receiveCustomStatisticsFailure$.subscribe();

            tick();

            expect(snackbarManagerSpy.openErrorSnackbar).toHaveBeenCalledWith([
                'Failed to receive statistics in custom statistics hub: Test error',
            ]);
        }));
    });

    describe('stopCustomStatisticsReceiving$', () => {
        it('should stop listening and remove the listener', fakeAsync(() => {
            const action = stopCustomStatisticsReceiving({ key: 'test-key' });

            actions$ = of(action);

            effects.stopCustomStatisticsReceiving$.subscribe();

            tick();

            expect(signalStatisticsSpy.stopListen).toHaveBeenCalled();
            expect(effects['activeListeners'].custom.has('test-key')).toBeFalse();
        }));
    });

    //#endregion

    //#region Load Amount Statistics

    describe('getLoadAmountStatisticsInRange$', () => {
        it('should dispatch getLoadAmountStatisticsInRangeSuccess on successful API call', () => {
            const req: MessageAmountInRangeRequest = {
                key: 'test-key',
                timeSpan: '00:05:00',
                from: new Date(),
                to: new Date()
            };
            const statistics: LoadAmountStatistics[] = [getDefaultLoadAmountStatistics()];
            const action = getLoadAmountStatisticsInRange({ req });

            apiServiceSpy.getLoadAmountStatisticsInRange.and.returnValue(of(statistics));

            actions$ = of(action);

            effects.getLoadAmountStatisticsInRange$.subscribe((result) => {
                expect(result).toEqual(
                    getLoadAmountStatisticsInRangeSuccess({
                        key: 'test-key',
                        statistics,
                        timespan: TimeSpan.fromString('00:05:00'),
                    })
                );
                expect(apiServiceSpy.getLoadAmountStatisticsInRange).toHaveBeenCalledWith(req);
            });
        });

        it('should dispatch getLoadAmountStatisticsInRangeFailure on API error', () => {
            const req: MessageAmountInRangeRequest = {
                key: 'test-key',
                timeSpan: '00:05:00',
                from: new Date(),
                to: new Date()
            };
            const error = { message: 'API error' };
            const action = getLoadAmountStatisticsInRange({ req });

            apiServiceSpy.getLoadAmountStatisticsInRange.and.returnValue(throwError(() => error));

            actions$ = of(action);

            effects.getLoadAmountStatisticsInRange$.subscribe((result) => {
                expect(result).toEqual(getLoadAmountStatisticsInRangeFailure({ error: error.message }));
                expect(apiServiceSpy.getLoadAmountStatisticsInRange).toHaveBeenCalledWith(req);
            });
        });
    });

    describe('getLoadAmountStatisticsInRangeFailure$', () => {
        it('should open error snackbar with the error message', fakeAsync(() => {
            const error = 'Test error message';
            const action = getLoadAmountStatisticsInRangeFailure({ error });

            actions$ = of(action);

            effects.getLoadAmountStatisticsInRangeFailure$.subscribe();

            tick();

            expect(snackbarManagerSpy.openErrorSnackbar).toHaveBeenCalledWith([
                'Failed to receive load amount statistics in range: Test error message',
            ]);
        }));
    });

    //#endregion

    //#region Slot Statistics

    describe('downloadSlotStatistics$', () => {
        it('should download statistics on successful API call', () => {
            const key = 'test-key';
            const statistics = getDefaultSlotStatistics();
            const action = downloadSlotStatistics({ key });

            apiServiceSpy.getSlotStatistics.and.returnValue(of(statistics));

            actions$ = of(action);

            effects.downloadSlotStatistics$.subscribe(() => {
                expect(apiServiceSpy.getSlotStatistics).toHaveBeenCalledWith(key);
                expect(jsonDownloaderSpy.downloadInJson).toHaveBeenCalledWith(statistics, `slot-data-${key}`);
            });
        });

        it('should dispatch downLoadSlotStatisticsFailure on API error', () => {
            const key = 'test-key';
            const error = { message: 'API error' };
            const action = downloadSlotStatistics({ key });

            apiServiceSpy.getSlotStatistics.and.returnValue(throwError(() => error));

            actions$ = of(action);

            effects.downloadSlotStatistics$.subscribe(() => {
                expect(apiServiceSpy.getSlotStatistics).toHaveBeenCalledWith(key);
            });
        });
    });

    describe('downLoadSlotStatisticsFailure$', () => {
        it('should open error snackbar with the error message', fakeAsync(() => {
            const error = 'Test error message';
            const action = downLoadSlotStatisticsFailure({ error });

            actions$ = of(action);

            effects.downLoadSlotStatisticsFailure$.subscribe();

            tick();

            expect(snackbarManagerSpy.openErrorSnackbar).toHaveBeenCalledWith([
                'Failed to download slot statistics : Test error message',
            ]);
        }));
    });

    //#endregion
});