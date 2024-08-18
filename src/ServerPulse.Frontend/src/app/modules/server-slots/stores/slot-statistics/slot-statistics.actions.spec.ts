import { selectDate, subscribeToLoadStatistics, subscribeToLoadStatisticsFailure, subscribeToLoadStatisticsSuccess, subscribeToSlotStatistics, subscribeToSlotStatisticsFailure, subscribeToSlotStatisticsSuccess } from "./slot-statistics.actions";

describe('Slot Statistics and Slot Load Statistics Actions', () => {
    const error = { message: 'An error occurred' };

    describe('Slot Statistics Actions', () => {
        it('should create subscribeToSlotStatistics action', () => {
            const slotKey = 'slotKey123';
            const action = subscribeToSlotStatistics({ slotKey });
            expect(action.type).toBe('[Slot Statistics] Subscribe To Slot Statistics');
            expect(action.slotKey).toEqual(slotKey);
        });

        it('should create subscribeToSlotStatisticsSuccess action', () => {
            const lastStatistics = { key: 'slotKey123', data: 'some data' };
            const action = subscribeToSlotStatisticsSuccess({ lastStatistics });
            expect(action.type).toBe('[Slot Statistics] Subscribe To Slot Statistics Success');
            expect(action.lastStatistics).toEqual(lastStatistics);
        });

        it('should create subscribeToSlotStatisticsFailure action', () => {
            const action = subscribeToSlotStatisticsFailure({ error });
            expect(action.type).toBe('[Slot Statistics] Subscribe To Slot Statistics Failure');
            expect(action.error).toEqual(error);
        });
    });

    describe('Slot Load Statistics Actions', () => {
        it('should create selectDate action', () => {
            const date = new Date();
            const action = selectDate({ date });
            expect(action.type).toBe('[Slot Load Statistics] Select Date');
            expect(action.date).toEqual(date);
        });

        it('should create subscribeToLoadStatistics action', () => {
            const slotKey = 'slotKey123';
            const action = subscribeToLoadStatistics({ slotKey });
            expect(action.type).toBe('[Slot Load Statistics] Subscribe To Load Statistics');
            expect(action.slotKey).toEqual(slotKey);
        });

        it('should create subscribeToLoadStatisticsSuccess action', () => {
            const lastLoadStatistics = { key: 'slotKey123', data: 'some data' };
            const action = subscribeToLoadStatisticsSuccess({ lastLoadStatistics });
            expect(action.type).toBe('[Slot Load Statistics] Subscribe To Load Statistics Success');
            expect(action.lastLoadStatistics).toEqual(lastLoadStatistics);
        });

        it('should create subscribeToLoadStatisticsFailure action', () => {
            const action = subscribeToLoadStatisticsFailure({ error });
            expect(action.type).toBe('[Slot Load Statistics] Subscribe To Load Statistics Failure');
            expect(action.error).toEqual(error);
        });
    });
});