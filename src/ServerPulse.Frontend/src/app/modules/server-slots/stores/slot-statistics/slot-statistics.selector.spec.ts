import { SlotCustomStatisticsState, SlotLoadStatisticsState, SlotStatisticsState } from "../../index";
import { selectCurrentDate, selectLastCustomStatistics, selectLastLoadStatistics, selectLastStatistics, selectSlotCustomStatisticsState, selectSlotLoadStatisticsState, selectSlotStatisticsState } from "./slot-statistics.selector";

describe('Slot Statistics and Load Statistics Selectors', () => {

    const serverStatisticsResponse: ServerStatisticsResponse = {
        isAlive: true,
        dataExists: true,
        serverLastStartDateTimeUTC: new Date('2023-08-01T00:00:00Z'),
        serverUptime: new TimeSpan(24, 5, 30, 0),
        lastServerUptime: new TimeSpan(0, 20, 0, 0),
        lastPulseDateTimeUTC: new Date('2023-08-18T12:00:00Z'),
        collectedDateUTC: new Date('2023-08-18T12:05:00Z'),
        isInitial: false
    };

    const initialSlotStatisticsState: SlotStatisticsState = {
        lastStatistics: { key: 'slot1', statistics: serverStatisticsResponse },
        error: null
    };
    const serverLoadStatisticsResponse: ServerLoadStatisticsResponse = {
        amountOfEvents: 10,
        lastEvent: null,
        collectedDateUTC: new Date('2023-08-18T12:05:00Z'),
        isInitial: false,
        loadMethodStatistics: null
    };

    const initialSlotLoadStatisticsState: SlotLoadStatisticsState = {
        currentDate: new Date('2023-08-18'),
        lastLoadStatistics: { key: 'slot1', statistics: serverLoadStatisticsResponse },
        error: null
    };

    const errorSlotStatisticsState: SlotStatisticsState = {
        lastStatistics: null,
        error: 'An error occurred'
    };

    const errorSlotLoadStatisticsState: SlotLoadStatisticsState = {
        currentDate: new Date('2023-08-18'),
        lastLoadStatistics: null,
        error: 'An error occurred'
    };

    describe('Slot Statistics Selectors', () => {
        it('should select the slot statistics state', () => {
            const result = selectSlotStatisticsState.projector(initialSlotStatisticsState);
            expect(result).toEqual(initialSlotStatisticsState);
        });

        it('should select the last statistics', () => {
            const result = selectLastStatistics.projector(initialSlotStatisticsState);
            expect(result).toEqual(initialSlotStatisticsState.lastStatistics);
        });

        it('should handle error in slot statistics state', () => {
            const result = selectLastStatistics.projector(errorSlotStatisticsState);
            expect(result).toBeNull();
        });
    });

    describe('Slot Load Statistics Selectors', () => {
        it('should select the slot load statistics state', () => {
            const result = selectSlotLoadStatisticsState.projector(initialSlotLoadStatisticsState);
            expect(result).toEqual(initialSlotLoadStatisticsState);
        });

        it('should select the current date', () => {
            const result = selectCurrentDate.projector(initialSlotLoadStatisticsState);
            expect(result).toEqual(initialSlotLoadStatisticsState.currentDate);
        });

        it('should select the last load statistics', () => {
            const result = selectLastLoadStatistics.projector(initialSlotLoadStatisticsState);
            expect(result).toEqual(initialSlotLoadStatisticsState.lastLoadStatistics);
        });

        it('should handle error in slot load statistics state', () => {
            const result = selectLastLoadStatistics.projector(errorSlotLoadStatisticsState);
            expect(result).toBeNull();
        });
    });

    describe('Slot Custom Statistics Selectors', () => {
        const customEventResponse: CustomEventResponse = {
            id: '1',
            key: 'slot1',
            creationDateUTC: new Date('2023-08-18T12:00:00Z'),
            name: 'Custom Event',
            description: 'A custom event',
            serializedMessage: 'Message content'
        };

        const customEventStatisticsResponse: CustomEventStatisticsResponse = {
            collectedDateUTC: new Date('2023-08-18T12:05:00Z'),
            isInitial: false,
            lastEvent: customEventResponse
        };

        const initialSlotCustomStatisticsState: SlotCustomStatisticsState = {
            lastStatistics: { key: 'slot1', statistics: customEventStatisticsResponse },
            error: null
        };

        const errorSlotCustomStatisticsState: SlotCustomStatisticsState = {
            lastStatistics: null,
            error: 'An error occurred'
        };

        it('should select the custom statistics state', () => {
            const result = selectSlotCustomStatisticsState.projector(initialSlotCustomStatisticsState);
            expect(result).toEqual(initialSlotCustomStatisticsState);
        });

        it('should select the last custom statistics', () => {
            const result = selectLastCustomStatistics.projector(initialSlotCustomStatisticsState);
            expect(result).toEqual(initialSlotCustomStatisticsState.lastStatistics);
        });

        it('should handle error in custom statistics state', () => {
            const result = selectLastCustomStatistics.projector(errorSlotCustomStatisticsState);
            expect(result).toBeNull();
        });
    });
});