import { ServerLoadStatisticsResponse, ServerStatisticsResponse, TimeSpan, convertToServerLoadStatisticsResponse, convertToServerStatisticsResponse } from "../../../shared";
import { selectDate, subscribeToLoadStatisticsFailure, subscribeToLoadStatisticsSuccess, subscribeToSlotStatisticsFailure, subscribeToSlotStatisticsSuccess } from "../../index";
import { SlotLoadStatisticsState, SlotStatisticsState, slotLoadStatisticsReducer, slotStatisticsReducer } from "./slot-statistics.reducer";

describe('SlotStatistics Reducer', () => {
    const initialSlotStatisticsState: SlotStatisticsState = {
        lastStatistics: null,
        error: null
    };

    const mockStatisticsResponse: ServerStatisticsResponse = {
        isAlive: true,
        dataExists: true,
        serverLastStartDateTimeUTC: new Date('2023-08-01T00:00:00Z'),
        serverUptime: new TimeSpan(24, 5, 30, 0),
        lastServerUptime: new TimeSpan(0, 20, 0, 0),
        lastPulseDateTimeUTC: new Date('2023-08-18T12:00:00Z'),
        collectedDateUTC: new Date('2023-08-18T12:05:00Z'),
        isInitial: false
    };

    const mockLastStatistics = { key: 'slot1', data: JSON.stringify(mockStatisticsResponse) };

    it('should return the initial state', () => {
        const action = { type: 'Unknown' } as any;
        const state = slotStatisticsReducer(initialSlotStatisticsState, action);
        expect(state).toBe(initialSlotStatisticsState);
    });

    it('should handle subscribeToSlotStatisticsSuccess', () => {
        const action = subscribeToSlotStatisticsSuccess({ lastStatistics: mockLastStatistics });
        const state = slotStatisticsReducer(initialSlotStatisticsState, action);
        expect(state.lastStatistics?.key).toBe(mockLastStatistics.key);
        expect(state.lastStatistics?.statistics).toEqual(convertToServerStatisticsResponse(mockStatisticsResponse));
        expect(state.error).toBeNull();
    });

    it('should handle subscribeToSlotStatisticsFailure', () => {
        const error = 'Error';
        const action = subscribeToSlotStatisticsFailure({ error });
        const state = slotStatisticsReducer(initialSlotStatisticsState, action);
        expect(state.error).toEqual(error);
    });
});

describe('SlotLoadStatistics Reducer', () => {
    const initialSlotLoadStatisticsState: SlotLoadStatisticsState = {
        currentDate: new Date('2023-08-18'),
        lastLoadStatistics: null,
        error: null
    };

    const mockLoadStatisticsResponse: ServerLoadStatisticsResponse = {
        amountOfEvents: 10,
        lastEvent: {
            id: "id",
            key: "key",
            creationDateUTC: new Date(),
            endpoint: "endpoint",
            method: "method",
            statusCode: 200,
            duration: new TimeSpan(0, 20, 0, 0),
            timestampUTC: new Date()
        },
        collectedDateUTC: new Date('2023-08-18T12:05:00Z'),
        isInitial: false
    };

    const mockLastLoadStatistics = { key: 'slot1', data: JSON.stringify(mockLoadStatisticsResponse) };

    it('should return the initial state', () => {
        const action = { type: 'Unknown' } as any;
        const state = slotLoadStatisticsReducer(initialSlotLoadStatisticsState, action);
        expect(state).toBe(initialSlotLoadStatisticsState);
    });

    it('should handle selectDate', () => {
        const newDate = new Date('2023-09-01');
        const action = selectDate({ date: newDate });
        const state = slotLoadStatisticsReducer(initialSlotLoadStatisticsState, action);
        expect(state.currentDate).toEqual(newDate);
    });

    it('should handle subscribeToLoadStatisticsSuccess', () => {
        const action = subscribeToLoadStatisticsSuccess({ lastLoadStatistics: mockLastLoadStatistics });
        const state = slotLoadStatisticsReducer(initialSlotLoadStatisticsState, action);
        expect(state.lastLoadStatistics?.key).toBe(mockLastLoadStatistics.key);
        expect(state.lastLoadStatistics?.statistics).toEqual(convertToServerLoadStatisticsResponse(mockLoadStatisticsResponse));
        expect(state.error).toBeNull();
    });

    it('should handle subscribeToLoadStatisticsFailure', () => {
        const error = 'Error';
        const action = subscribeToLoadStatisticsFailure({ error });
        const state = slotLoadStatisticsReducer(initialSlotLoadStatisticsState, action);
        expect(state.error).toEqual(error);
    });
});