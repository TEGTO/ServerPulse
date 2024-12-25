import { getDefaultCustomEvent, getDefaultLoadAmountStatistics, getDefaultLoadEvent, getDefaultServerCustomStatistics, getDefaultServerLifecycleStatistics, getDefaultServerLoadStatistics, selectCustomStatisticsByKey, selectLastCustomEventByKey, selectLastLifecycleStatisticsByKey, selectLastLoadEventByKey, selectLastLoadStatisticsByKey, selectLifecycleStatisticsByKey, selectLoadAmountStatisticsByKey, selectLoadStatisticsByKey } from "..";
import { LoadAmountStatisticsState, ServerCustomStatisticsState, ServerLifecycleStatisticsState, ServerLoadStatisticsState } from "./analyzer.reducer";

//#region Lifecycle Statistics

describe('Lifecycle Statistics Selectors', () => {
    const initialState: ServerLifecycleStatisticsState = {
        statisticsMap: new Map([
            ['key1', [{ ...getDefaultServerLifecycleStatistics(), id: '1', }]],
            ['key2', [{ ...getDefaultServerLifecycleStatistics(), id: '2', }, { ...getDefaultServerLifecycleStatistics(), id: '3', }]],
        ]),
    };

    it('should select lifecycle statistics by key', () => {
        const result = selectLifecycleStatisticsByKey('key1').projector(initialState);
        expect(result).toEqual([jasmine.objectContaining({ id: '1', })]);
    });

    it('should select last lifecycle statistics by key', () => {
        const result = selectLastLifecycleStatisticsByKey('key2').projector(initialState);
        expect(result).toEqual({ ...result!, id: '3', });
    });

    it('should return empty array for non-existing key in selectLifecycleStatisticsByKey', () => {
        const result = selectLifecycleStatisticsByKey('non-existing-key').projector(initialState);
        expect(result).toEqual([]);
    });

    it('should return null for non-existing key in selectLastLifecycleStatisticsByKey', () => {
        const result = selectLastLifecycleStatisticsByKey('non-existing-key').projector(initialState);
        expect(result).toBeNull();
    });
});

//#endregion

//#region Load Statistics

describe('Load Statistics Selectors', () => {
    const initialState: ServerLoadStatisticsState = {
        statisticsMap: new Map([
            ['key1', [{ ...getDefaultServerLoadStatistics(), id: '1', }]],
            ['key2', [{ ...getDefaultServerLoadStatistics(), id: '2', lastEvent: { ...getDefaultLoadEvent(), id: 'event1' } }]],
        ]),
    };

    it('should select load statistics by key', () => {
        const result = selectLoadStatisticsByKey('key1').projector(initialState);
        expect(result).toEqual([jasmine.objectContaining({ id: '1' })]);

    });

    it('should select last load statistics by key', () => {
        const result = selectLastLoadStatisticsByKey('key2').projector(initialState);
        expect(result).toEqual(jasmine.objectContaining({ id: '2' }));
    });

    it('should select last load event by key', () => {
        const result = selectLastLoadEventByKey('key2').projector(initialState);
        expect(result).toEqual(jasmine.objectContaining({ id: 'event1' }));
    });

    it('should return empty array for non-existing key in selectLoadStatisticsByKey', () => {
        const result = selectLoadStatisticsByKey('non-existing-key').projector(initialState);
        expect(result).toEqual([]);
    });

    it('should return null for non-existing key in selectLastLoadStatisticsByKey', () => {
        const result = selectLastLoadStatisticsByKey('non-existing-key').projector(initialState);
        expect(result).toBeNull();
    });

    it('should return null for non-existing key in selectLastLoadEventByKey', () => {
        const result = selectLastLoadEventByKey('non-existing-key').projector(initialState);
        expect(result).toBeNull();
    });
});

//#endregion

//#region Custom Statistics

describe('Custom Statistics Selectors', () => {
    const initialState: ServerCustomStatisticsState = {
        statisticsMap: new Map([
            ['key1', [{ ...getDefaultServerCustomStatistics(), id: '1', }]],
            ['key2', [{ ...getDefaultServerCustomStatistics(), id: '2', lastEvent: { ...getDefaultCustomEvent(), id: 'event1' } }]],
        ]),
    };

    it('should select custom statistics by key', () => {
        const result = selectCustomStatisticsByKey('key1').projector(initialState);
        expect(result).toEqual([jasmine.objectContaining({ id: '1' })]);
    });

    it('should select last custom event by key', () => {
        const result = selectLastCustomEventByKey('key2').projector(initialState);
        expect(result).toEqual(jasmine.objectContaining({ id: 'event1' }));
    });

    it('should return empty array for non-existing key in selectCustomStatisticsByKey', () => {
        const result = selectCustomStatisticsByKey('non-existing-key').projector(initialState);
        expect(result).toEqual([]);
    });

    it('should return null for non-existing key in selectLastCustomEventByKey', () => {
        const result = selectLastCustomEventByKey('non-existing-key').projector(initialState);
        expect(result).toBeNull();
    });
});

//#endregion

//#region Load Amount Statistics

describe('Load Amount Statistics Selectors', () => {
    const initialState: LoadAmountStatisticsState = {
        statisticsMap: new Map([
            ['key1', [{ ...getDefaultLoadAmountStatistics(), id: '1', }]],
        ]),
        timespanMap: new Map(),
        addedEvents: new Map(),
    };

    it('should select load amount statistics by key', () => {
        const result = selectLoadAmountStatisticsByKey('key1').projector(initialState);
        expect(result).toEqual([jasmine.objectContaining({ id: '1' })]);
    });

    it('should return empty array for non-existing key', () => {
        const result = selectLoadAmountStatisticsByKey('non-existing-key').projector(initialState);
        expect(result).toEqual([]);
    });
});

//#endregion