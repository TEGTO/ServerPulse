/* eslint-disable @typescript-eslint/no-explicit-any */
import { addLoadEventToLoadAmountStatistics, getDefaultLoadAmountStatistics, getDefaultLoadEvent, getDefaultServerCustomStatistics, getDefaultServerLifecycleStatistics, getDefaultServerLoadStatistics, getLoadAmountStatisticsInRangeSuccess, LoadAmountStatistics, LoadAmountStatisticsState, receiveCustomStatisticsSuccess, receiveLifecycleStatisticsSuccess, receiveLoadStatisticsSuccess, ServerCustomStatistics, serverCustomStatisticsReducer, ServerCustomStatisticsState, ServerLifecycleStatistics, serverLifecycleStatisticsReducer, ServerLifecycleStatisticsState, serverLoadAmountStatisticsReducer, ServerLoadStatistics, serverLoadStatisticsReducer, ServerLoadStatisticsState } from "..";
import { TimeSpan } from "../../shared";

//#region Lifecycle Statistics

describe('ServerLifecycleStatisticsReducer', () => {
    const initialState: ServerLifecycleStatisticsState = {
        statisticsMap: new Map<string, ServerLifecycleStatistics[]>()
    };

    it('should return the initial state by default', () => {
        const action = { type: 'UNKNOWN' } as any;
        const state = serverLifecycleStatisticsReducer(initialState, action);

        expect(state).toEqual(initialState);
    });

    it('should add new statistics on receiveLifecycleStatisticsSuccess', () => {
        const key = 'test-key';
        const statistics = getDefaultServerLifecycleStatistics();
        const action = receiveLifecycleStatisticsSuccess({ key, statistics });

        const state = serverLifecycleStatisticsReducer(initialState, action);

        expect(state.statisticsMap.get(key)?.length).toBe(1);
        expect(state.statisticsMap.get(key)).toEqual([statistics]);
    });

    it('should not add duplicate statistics on receiveLifecycleStatisticsSuccess', () => {
        const key = 'test-key';
        const statistics = getDefaultServerLifecycleStatistics();
        const preloadedState = {
            ...initialState,
            statisticsMap: new Map<string, ServerLifecycleStatistics[]>([[key, [statistics]]])
        };
        const action = receiveLifecycleStatisticsSuccess({ key, statistics });

        const state = serverLifecycleStatisticsReducer(preloadedState, action);

        expect(state.statisticsMap.get(key)?.length).toBe(1);
    });
});

//#endregion

//#region Load Statistics

describe('ServerLoadStatisticsReducer', () => {
    const initialState: ServerLoadStatisticsState = {
        statisticsMap: new Map<string, ServerLoadStatistics[]>()
    };

    it('should return the initial state by default', () => {
        const action = { type: 'UNKNOWN' } as any;
        const state = serverLoadStatisticsReducer(initialState, action);

        expect(state).toEqual(initialState);
    });

    it('should add new load statistics on receiveLoadStatisticsSuccess', () => {
        const key = 'test-key';
        const statistics = getDefaultServerLoadStatistics();
        const action = receiveLoadStatisticsSuccess({ key, statistics });

        const state = serverLoadStatisticsReducer(initialState, action);

        expect(state.statisticsMap.get(key)).toEqual([statistics]);
    });

    it('should not add duplicate load statistics', () => {
        const key = 'test-key';
        const statistics = getDefaultServerLoadStatistics();
        const preloadedState = {
            ...initialState,
            statisticsMap: new Map<string, ServerLoadStatistics[]>([[key, [statistics]]])
        };
        const action = receiveLoadStatisticsSuccess({ key, statistics });

        const state = serverLoadStatisticsReducer(preloadedState, action);

        expect(state.statisticsMap.get(key)?.length).toBe(1);
    });
});

//#endregion

//#region Custom Statistics

describe('ServerCustomStatisticsReducer', () => {
    const initialState: ServerCustomStatisticsState = {
        statisticsMap: new Map<string, ServerCustomStatistics[]>()
    };

    it('should return the initial state by default', () => {
        const action = { type: 'UNKNOWN' } as any;
        const state = serverCustomStatisticsReducer(initialState, action);

        expect(state).toEqual(initialState);
    });

    it('should add new custom statistics on receiveCustomStatisticsSuccess', () => {
        const key = 'test-key';
        const statistics = getDefaultServerCustomStatistics();
        const action = receiveCustomStatisticsSuccess({ key, statistics });

        const state = serverCustomStatisticsReducer(initialState, action);

        expect(state.statisticsMap.get(key)).toEqual([statistics]);
    });

    it('should not add duplicate custom statistics', () => {
        const key = 'test-key';
        const statistics = getDefaultServerCustomStatistics();
        const preloadedState = {
            ...initialState,
            statisticsMap: new Map<string, ServerCustomStatistics[]>([[key, [statistics]]])
        };
        const action = receiveCustomStatisticsSuccess({ key, statistics });

        const state = serverCustomStatisticsReducer(preloadedState, action);

        expect(state.statisticsMap.get(key)?.length).toBe(1);
    });
});

//#endregion

//#region Load Amount Statistics

describe('ServerLoadAmountStatisticsReducer', () => {
    const initialState: LoadAmountStatisticsState = {
        statisticsMap: new Map<string, LoadAmountStatistics[]>(),
        timespanMap: new Map<string, TimeSpan>(),
        addedEvents: new Map<string, string>()
    };

    it('should return the initial state by default', () => {
        const action = { type: 'UNKNOWN' } as any;
        const state = serverLoadAmountStatisticsReducer(initialState, action);

        expect(state).toEqual(initialState);
    });

    it('should update statistics and timespan on getLoadAmountStatisticsInRangeSuccess', () => {
        const key = 'test-key';
        const statistics: LoadAmountStatistics[] = [getDefaultLoadAmountStatistics()];
        const timespan = new TimeSpan(0, 5, 0);
        const action = getLoadAmountStatisticsInRangeSuccess({ key, statistics, timespan });

        const state = serverLoadAmountStatisticsReducer(initialState, action);

        expect(state.statisticsMap.get(key)).toEqual(statistics);
        expect(state.timespanMap.get(key)).toEqual(timespan);
    });

    it('should add events to statistics on addLoadEventToLoadAmountStatistics', () => {
        const key = 'test-key-1';
        const event = getDefaultLoadEvent();
        const action = addLoadEventToLoadAmountStatistics({ key, event });

        const timespanMap = new Map<string, TimeSpan>();
        timespanMap.set(key, new TimeSpan(24, 24, 24, 24));
        const initialStateWithTimespan = { ...initialState, timespanMap: timespanMap }

        const state = serverLoadAmountStatisticsReducer(initialStateWithTimespan, action);

        expect(state.addedEvents.get(key)).toEqual(event.id);
        expect(state.statisticsMap.get(key)).toBeTruthy();
        expect(state.statisticsMap.get(key)!.length).toEqual(1);
        expect(state.statisticsMap.get(key)![0].amountOfEvents).toEqual(1);
    });

    it('should add more events to statistics on addLoadEventToLoadAmountStatistics', () => {
        const key = 'test-key-2';
        const event = getDefaultLoadEvent();
        const event2 = { ...getDefaultLoadEvent(), id: "newId" };
        const action = addLoadEventToLoadAmountStatistics({ key, event });
        const action2 = addLoadEventToLoadAmountStatistics({ key, event: event2 });

        const timespanMap = new Map<string, TimeSpan>();
        timespanMap.set(key, new TimeSpan(24, 24, 24, 24));
        const initialStateWithTimespan = { ...initialState, timespanMap: timespanMap }

        const state = serverLoadAmountStatisticsReducer(initialStateWithTimespan, action);
        const state2 = serverLoadAmountStatisticsReducer(state, action2);

        expect(state2.addedEvents.get(key)).toEqual(event2.id);
        expect(state.statisticsMap.get(key)).toBeTruthy();
        expect(state.statisticsMap.get(key)!.length).toEqual(1);
        expect(state.statisticsMap.get(key)![0].amountOfEvents).toEqual(2);
    });

    it('should not add events to statistics on addLoadEventToLoadAmountStatistics, because there are no timespans', () => {
        const key = 'test-key-3';
        const event = getDefaultLoadEvent();
        const action = addLoadEventToLoadAmountStatistics({ key, event });

        const state = serverLoadAmountStatisticsReducer(initialState, action);

        expect(state.addedEvents.get(key)).toEqual(event.id);
        expect(state.statisticsMap.get(key)).toBeTruthy();
        expect(state.statisticsMap.get(key)!.length).toEqual(0);
    });
});

//#endregion