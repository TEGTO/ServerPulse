import { CustomEventResponse, CustomEventStatisticsResponse, ServerLoadStatisticsResponse, ServerStatisticsResponse, TimeSpan, convertToCustomEventStatisticsResponse, convertToServerLoadStatisticsResponse, convertToServerStatisticsResponse } from "../../../shared";
import { selectDate, subscribeToCustomStatisticsFailure, subscribeToCustomStatisticsSuccess, subscribeToLoadStatisticsFailure, subscribeToLoadStatisticsSuccess, subscribeToSlotStatisticsFailure, subscribeToSlotStatisticsSuccess } from "../../index";
import { SlotCustomStatisticsState, SlotLoadStatisticsState, SlotStatisticsState, slotCustomStatisticsReducer, slotLoadStatisticsReducer, slotStatisticsReducer } from "./slot-statistics.reducer";

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
        expect(state.lastStatistics?.statistics.serverUptime).toEqual(convertToServerStatisticsResponse(mockStatisticsResponse).serverUptime);
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
        loadMethodStatistics: null,
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
        expect(state.lastLoadStatistics?.statistics.amountOfEvents).toEqual(convertToServerLoadStatisticsResponse(mockLoadStatisticsResponse).amountOfEvents);
        expect(state.error).toBeNull();
    });

    it('should handle subscribeToLoadStatisticsFailure', () => {
        const error = 'Error';
        const action = subscribeToLoadStatisticsFailure({ error });
        const state = slotLoadStatisticsReducer(initialSlotLoadStatisticsState, action);
        expect(state.error).toEqual(error);
    });
});

describe('SlotCustomStatistics Reducer', () => {
    const initialSlotCustomStatisticsState: SlotCustomStatisticsState = {
        lastStatistics: null,
        error: null
    };

    const mockCustomEventResponse: CustomEventResponse = {
        id: '1',
        key: 'slot1',
        name: 'Custom Event',
        description: 'A custom event',
        serializedMessage: 'Serialized message content',
        creationDateUTC: new Date('2023-08-18T12:00:00Z')
    };

    const mockCustomEventStatisticsResponse: CustomEventStatisticsResponse = {
        collectedDateUTC: new Date('2023-08-18T12:05:00Z'),
        isInitial: false,
        lastEvent: mockCustomEventResponse
    };

    const mockLastCustomStatistics = { key: 'slot1', data: JSON.stringify(mockCustomEventStatisticsResponse) };

    it('should return the initial state', () => {
        const action = { type: 'Unknown' } as any;
        const state = slotCustomStatisticsReducer(initialSlotCustomStatisticsState, action);
        expect(state).toBe(initialSlotCustomStatisticsState);
    });

    it('should handle subscribeToCustomStatisticsSuccess', () => {
        const action = subscribeToCustomStatisticsSuccess({ lastStatistics: mockLastCustomStatistics });
        const state = slotCustomStatisticsReducer(initialSlotCustomStatisticsState, action);
        expect(state.lastStatistics?.key).toBe(mockLastCustomStatistics.key);
        expect(state.lastStatistics?.statistics.lastEvent?.id).toEqual(convertToCustomEventStatisticsResponse(JSON.parse(mockLastCustomStatistics.data)).lastEvent?.id);
        expect(state.error).toBeNull();
    });

    it('should handle subscribeToCustomStatisticsFailure', () => {
        const error = 'Error';
        const action = subscribeToCustomStatisticsFailure({ error });
        const state = slotCustomStatisticsReducer(initialSlotCustomStatisticsState, action);
        expect(state.error).toEqual(error);
    });
});