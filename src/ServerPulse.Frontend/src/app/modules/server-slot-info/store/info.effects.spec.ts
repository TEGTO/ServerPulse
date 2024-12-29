import { fakeAsync, TestBed, tick } from "@angular/core/testing";
import { provideMockActions } from "@ngrx/effects/testing";
import { Action } from "@ngrx/store";
import { Observable, of, throwError } from "rxjs";
import { ServerSlotInfoDialogManagerService } from "..";
import { AnalyzerApiService, CustomEvent, LoadAmountStatistics, LoadEvent } from "../../analyzer";
import { SnackbarManager, TimeSpan } from "../../shared";
import { getDailyLoadAmountStatistics, getDailyLoadAmountStatisticsFailure, getDailyLoadAmountStatisticsSuccess, getSomeCustomEvents, getSomeCustomEventsFailure, getSomeCustomEventsSuccess, getSomeLoadEvents, getSomeLoadEventsFailure, getSomeLoadEventsSuccess, showCustomDetailsEvent } from "./info.actions";
import { ServerSlotInfoEffects } from "./info.effects";

describe('ServerSlotInfoEffects', () => {
    let actions$: Observable<Action>;
    let effects: ServerSlotInfoEffects;

    let analyzerApiServiceSpy: jasmine.SpyObj<AnalyzerApiService>;
    let snackbarManagerSpy: jasmine.SpyObj<SnackbarManager>;
    let dialogManagerSpy: jasmine.SpyObj<ServerSlotInfoDialogManagerService>;

    beforeEach(() => {
        analyzerApiServiceSpy = jasmine.createSpyObj<AnalyzerApiService>('AnalyzerApiService', [
            'getSomeLoadEvents',
            'getSomeCustomEvents',
            'getDailyLoadStatistics',
            'getLoadAmountStatisticsInRange',
        ]);
        snackbarManagerSpy = jasmine.createSpyObj<SnackbarManager>('SnackbarManager', ['openErrorSnackbar']);
        dialogManagerSpy = jasmine.createSpyObj<ServerSlotInfoDialogManagerService>('ServerSlotInfoDialogManagerService', ['openCustomEventDetails']);

        TestBed.configureTestingModule({
            providers: [
                ServerSlotInfoEffects,
                provideMockActions(() => actions$),
                { provide: AnalyzerApiService, useValue: analyzerApiServiceSpy },
                { provide: SnackbarManager, useValue: snackbarManagerSpy },
                { provide: ServerSlotInfoDialogManagerService, useValue: dialogManagerSpy },
            ],
        });

        effects = TestBed.inject(ServerSlotInfoEffects);
    });

    describe('getSomeLoadEvents$', () => {
        it('should dispatch getSomeLoadEventsSuccess when API call succeeds', () => {
            const testEvents: LoadEvent[] = [{ id: '1', key: 'key1', creationDateUTC: new Date(), endpoint: '', method: '', statusCode: 200, duration: new TimeSpan(0, 0, 1, 0), timestampUTC: new Date() }];
            const action = getSomeLoadEvents({ req: { key: 'key1', numberOfMessages: 10, startDate: new Date(), readNew: true } });
            const response = getSomeLoadEventsSuccess({ events: testEvents });

            analyzerApiServiceSpy.getSomeLoadEvents.and.returnValue(of(testEvents));
            actions$ = of(action);

            effects.getSomeLoadEvents$.subscribe(result => {
                expect(result).toEqual(response);
                expect(analyzerApiServiceSpy.getSomeLoadEvents).toHaveBeenCalledWith(action.req);
            });
        });

        it('should dispatch getSomeLoadEventsFailure when API call fails', () => {
            const action = getSomeLoadEvents({ req: { key: 'key1', numberOfMessages: 10, startDate: new Date(), readNew: true } });
            const response = getSomeLoadEventsFailure({ error: 'Error occurred' });

            analyzerApiServiceSpy.getSomeLoadEvents.and.returnValue(throwError(() => new Error('Error occurred')));
            actions$ = of(action);

            effects.getSomeLoadEvents$.subscribe(result => {
                expect(result).toEqual(response);
            });
        });
    });

    it('should open dialog menu on getSomeLoadEventsFailure', fakeAsync(() => {
        const action = getSomeLoadEventsFailure({ error: 'Error occurred' });

        actions$ = of(action);

        effects.getSomeLoadEventsFailure$.subscribe();

        tick();

        expect(snackbarManagerSpy.openErrorSnackbar).toHaveBeenCalledWith(['Failed to get load events: Error occurred']);
    }));

    describe('getSomeCustomEvents$', () => {
        it('should dispatch getSomeCustomEventsSuccess when API call succeeds', () => {
            const testEvents: CustomEvent[] = [{ id: '2', key: 'key2', creationDateUTC: new Date(), name: 'Test Event', description: '', serializedMessage: '' }];
            const action = getSomeCustomEvents({ req: { key: 'key2', numberOfMessages: 5, startDate: new Date(), readNew: true } });
            const response = getSomeCustomEventsSuccess({ events: testEvents });

            analyzerApiServiceSpy.getSomeCustomEvents.and.returnValue(of(testEvents));
            actions$ = of(action);

            effects.getSomeCustomEvents$.subscribe(result => {
                expect(result).toEqual(response);
                expect(analyzerApiServiceSpy.getSomeCustomEvents).toHaveBeenCalledWith(action.req);
            });
        });

        it('should dispatch getSomeCustomEventsFailure when API call fails', () => {
            const action = getSomeCustomEvents({ req: { key: 'key2', numberOfMessages: 5, startDate: new Date(), readNew: true } });
            const response = getSomeCustomEventsFailure({ error: 'Error occurred' });

            analyzerApiServiceSpy.getSomeCustomEvents.and.returnValue(throwError(() => new Error('Error occurred')));
            actions$ = of(action);

            effects.getSomeCustomEvents$.subscribe(result => {
                expect(result).toEqual(response);
            });
        });
    });

    it('should open dialog menu on getSomeCustomEventsFailure', fakeAsync(() => {
        const action = getSomeCustomEventsFailure({ error: 'Error occurred' });

        actions$ = of(action);

        effects.getSomeCustomEventsFailure$.subscribe();

        tick();

        expect(snackbarManagerSpy.openErrorSnackbar).toHaveBeenCalledWith(['Failed to get custom events: Error occurred']);
    }));

    describe('getDailyLoadAmountStatistics$', () => {
        it('should dispatch getDailyLoadAmountStatisticsSuccess when API call succeeds', () => {
            const testStatistics: LoadAmountStatistics[] = [{ id: '3', collectedDateUTC: new Date(), amountOfEvents: 10, dateFrom: new Date(), dateTo: new Date() }];
            const action = getDailyLoadAmountStatistics({ key: 'key3' });
            const response = getDailyLoadAmountStatisticsSuccess({ statistics: testStatistics });

            analyzerApiServiceSpy.getDailyLoadStatistics.and.returnValue(of(testStatistics));
            actions$ = of(action);

            effects.getDailyLoadAmountStatistics$.subscribe(result => {
                expect(result).toEqual(response);
            });
        });

        it('should dispatch getDailyLoadAmountStatisticsFailure when API call fails', () => {
            const action = getDailyLoadAmountStatistics({ key: 'key3' });
            const response = getDailyLoadAmountStatisticsFailure({ error: 'Error occurred' });

            analyzerApiServiceSpy.getDailyLoadStatistics.and.returnValue(throwError(() => new Error('Error occurred')));
            actions$ = of(action);

            effects.getDailyLoadAmountStatistics$.subscribe(result => {
                expect(result).toEqual(response);
            });
        });
    });

    it('should open dialog menu on getDailyLoadAmountStatisticsFailure', fakeAsync(() => {
        const action = getDailyLoadAmountStatisticsFailure({ error: 'Error occurred' });

        actions$ = of(action);

        effects.getDailyLoadAmountStatisticsFailure$.subscribe();

        tick();

        expect(snackbarManagerSpy.openErrorSnackbar).toHaveBeenCalledWith(['Failed to get daily load amount statistics: Error occurred']);
    }));

    describe('showCustomDetailsEvent$', () => {
        it('should call dialogManager.openCustomEventDetails', fakeAsync(() => {
            const action = showCustomDetailsEvent({ event: { id: '4', key: 'key4', creationDateUTC: new Date(), name: 'Detail Event', description: '', serializedMessage: '{}' } });
            actions$ = of(action);

            effects.showCustomDetailsEvent$.subscribe();

            tick();

            expect(dialogManagerSpy.openCustomEventDetails).toHaveBeenCalledWith(action.event.serializedMessage);
        }));
    });
});