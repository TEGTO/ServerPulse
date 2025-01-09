import { selectCustomEvents, selectCustomReadFromDate, selectLoadAmountStatistics, selectLoadEvents, selectReadFromDate, selectSecondaryLoadAmountStatistics, selectSelectedDate, selectSlotInfoState, SlotInfoState } from "..";
import { TimeSpan } from "../../shared";

describe('SlotInfo Selectors', () => {
    const initialState: SlotInfoState = {
        selectedDate: new Date('2024-01-01T00:00:00.000Z'),
        readFromDate: new Date('2024-01-01T00:00:00.000Z'),
        customReadFromDate: new Date('2024-01-01T00:00:00.000Z'),
        loadStatisticsInterval: 0,
        secondaryLoadStatisticsInterval: 0,
        loadEvents: [],
        customEvents: [],
        loadAmountStatistics: [],
        secondaryLoadAmountStatistics: []
    };

    const populatedState: SlotInfoState = {
        ...initialState,
        loadEvents: [
            { id: '1', key: 'key1', creationDateUTC: new Date(), endpoint: '/api', method: 'GET', statusCode: 200, duration: new TimeSpan(0, 0, 0, 100), timestampUTC: new Date() },
            { id: '2', key: 'key2', creationDateUTC: new Date(), endpoint: '/api', method: 'POST', statusCode: 201, duration: new TimeSpan(0, 0, 0, 200), timestampUTC: new Date() }
        ],
        customEvents: [
            { id: '1', key: 'key1', creationDateUTC: new Date(), name: 'Custom Event 1', description: 'Desc 1', serializedMessage: '{}' },
            { id: '2', key: 'key2', creationDateUTC: new Date(), name: 'Custom Event 2', description: 'Desc 2', serializedMessage: '{}' }
        ],
        loadAmountStatistics: [
            { id: '1', amountOfEvents: 10, dateFrom: new Date(), dateTo: new Date(), collectedDateUTC: new Date() }
        ],
        secondaryLoadAmountStatistics: [
            { id: '2', amountOfEvents: 5, dateFrom: new Date(), dateTo: new Date(), collectedDateUTC: new Date() }
        ]
    };

    it('should select the slot info state', () => {
        const result = selectSlotInfoState.projector(initialState);
        expect(result).toEqual(initialState);
    });

    it('should select load events', () => {
        const result = selectLoadEvents.projector(populatedState);
        expect(result).toEqual(populatedState.loadEvents);
    });

    it('should select custom events', () => {
        const result = selectCustomEvents.projector(populatedState);
        expect(result).toEqual(populatedState.customEvents);
    });

    it('should select selected date', () => {
        const result = selectSelectedDate.projector(initialState);
        expect(result).toEqual(initialState.selectedDate);
    });

    it('should select read from date', () => {
        const result = selectReadFromDate.projector(initialState);
        expect(result).toEqual(initialState.readFromDate);
    });

    it('should select custom read from date', () => {
        const result = selectCustomReadFromDate.projector(initialState);
        expect(result).toEqual(initialState.customReadFromDate);
    });

    it('should select load amount statistics', () => {
        const result = selectLoadAmountStatistics.projector(populatedState);
        expect(result).toEqual(populatedState.loadAmountStatistics);
    });

    it('should select secondary load amount statistics', () => {
        const result = selectSecondaryLoadAmountStatistics.projector(populatedState);
        expect(result).toEqual(populatedState.secondaryLoadAmountStatistics);
    });
});