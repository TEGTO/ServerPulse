/* eslint-disable @typescript-eslint/no-explicit-any */
import { addLoadEventToLoadAmountStatistics, downloadSlotStatistics, downLoadSlotStatisticsFailure, getLoadAmountStatisticsInRange, getLoadAmountStatisticsInRangeFailure, getLoadAmountStatisticsInRangeSuccess, receiveCustomStatisticsFailure, receiveCustomStatisticsSuccess, receiveLifecycleStatisticsFailure, receiveLifecycleStatisticsSuccess, receiveLoadStatisticsFailure, receiveLoadStatisticsSuccess, startCustomStatisticsReceiving, startLifecycleStatisticsReceiving, startLoadStatisticsReceiving, stopCustomStatisticsReceiving, stopLifecycleStatisticsReceiving, stopLoadKeyListening } from "..";

describe('Statistics Actions', () => {
    const error = { message: 'An error occurred' };

    describe('Lifecycle Statistics Actions', () => {
        it('should create startLifecycleStatisticsReceiving action', () => {
            const action = startLifecycleStatisticsReceiving({ key: 'testKey', getInitial: true });
            expect(action.type).toBe('[Statistics] Start Lifecycle Statistics Receiving ');
            expect(action.key).toBe('testKey');
            expect(action.getInitial).toBeTrue();
        });

        it('should create stopLifecycleStatisticsReceiving action', () => {
            const action = stopLifecycleStatisticsReceiving({ key: 'testKey' });
            expect(action.type).toBe('[Statistics] Stop Lifecycle Statistics Receiving');
            expect(action.key).toBe('testKey');
        });

        it('should create receiveLifecycleStatisticsSuccess action', () => {
            const statistics = { id: '123', isAlive: true } as any;
            const action = receiveLifecycleStatisticsSuccess({ key: 'testKey', statistics });
            expect(action.type).toBe('[Statistics] Receive Lifecycle Statistics Success');
            expect(action.key).toBe('testKey');
            expect(action.statistics).toBe(statistics);
        });

        it('should create receiveLifecycleStatisticsFailure action', () => {
            const action = receiveLifecycleStatisticsFailure({ error });
            expect(action.type).toBe('[Chat] Receive Lifecycle Statistics Failure');
            expect(action.error).toEqual(error);
        });
    });

    describe('Load Statistics Actions', () => {
        it('should create startLoadStatisticsReceiving action', () => {
            const action = startLoadStatisticsReceiving({ key: 'testKey', getInitial: false });
            expect(action.type).toBe('[Statistics] Start Load Statistics Receiving ');
            expect(action.key).toBe('testKey');
            expect(action.getInitial).toBeFalse();
        });

        it('should create stopLoadKeyListening action', () => {
            const action = stopLoadKeyListening({ key: 'testKey' });
            expect(action.type).toBe('[Statistics] Stop Load Key Listening');
            expect(action.key).toBe('testKey');
        });

        it('should create receiveLoadStatisticsSuccess action', () => {
            const statistics = { id: '123', amountOfEvents: 10 } as any;
            const action = receiveLoadStatisticsSuccess({ key: 'testKey', statistics });
            expect(action.type).toBe('[Statistics] Receive Load Statistics Success');
            expect(action.key).toBe('testKey');
            expect(action.statistics).toBe(statistics);
        });

        it('should create receiveLoadStatisticsFailure action', () => {
            const action = receiveLoadStatisticsFailure({ error });
            expect(action.type).toBe('[Chat] Receive Load Statistics Failure');
            expect(action.error).toEqual(error);
        });
    });

    describe('Custom Statistics Actions', () => {
        it('should create startCustomStatisticsReceiving action', () => {
            const action = startCustomStatisticsReceiving({ key: 'testKey', getInitial: true });
            expect(action.type).toBe('[Statistics] Start Custom Statistics Receiving ');
            expect(action.key).toBe('testKey');
            expect(action.getInitial).toBeTrue();
        });

        it('should create stopCustomStatisticsReceiving action', () => {
            const action = stopCustomStatisticsReceiving({ key: 'testKey' });
            expect(action.type).toBe('[Statistics] Stop Custom Statistics Receiving');
            expect(action.key).toBe('testKey');
        });

        it('should create receiveCustomStatisticsSuccess action', () => {
            const statistics = { id: '123', customField: 'value' } as any;
            const action = receiveCustomStatisticsSuccess({ key: 'testKey', statistics });
            expect(action.type).toBe('[Statistics] Receive Custom Statistics Success');
            expect(action.key).toBe('testKey');
            expect(action.statistics).toBe(statistics);
        });

        it('should create receiveCustomStatisticsFailure action', () => {
            const action = receiveCustomStatisticsFailure({ error });
            expect(action.type).toBe('[Chat] Receive Custom Statistics Failure');
            expect(action.error).toEqual(error);
        });
    });

    describe('Load Amount Statistics Actions', () => {
        it('should create getLoadAmountStatisticsInRange action', () => {
            const req = { key: 'testKey', from: new Date(), to: new Date(), timeSpan: '1:0:0' } as any;
            const action = getLoadAmountStatisticsInRange({ req });
            expect(action.type).toBe('[Statistics] Get Load Amount Statistics In Range');
            expect(action.req).toBe(req);
        });

        it('should create getLoadAmountStatisticsInRangeSuccess action', () => {
            const statistics = [{ id: '123', amountOfEvents: 10 }] as any;
            const timespan = { hours: 1, minutes: 0 } as any;
            const action = getLoadAmountStatisticsInRangeSuccess({ key: 'testKey', statistics, timespan });
            expect(action.type).toBe('[Statistics] Get Load Amount Statistics In Range Success');
            expect(action.key).toBe('testKey');
            expect(action.statistics).toBe(statistics);
            expect(action.timespan).toBe(timespan);
        });

        it('should create getLoadAmountStatisticsInRangeFailure action', () => {
            const action = getLoadAmountStatisticsInRangeFailure({ error });
            expect(action.type).toBe('[Statistics] Get Load Amount Statistics In Range Failure');
            expect(action.error).toEqual(error);
        });

        it('should create addLoadEventToLoadAmountStatistics action', () => {
            const event = { id: '123', endpoint: '/api', method: 'GET', statusCode: 200 } as any;
            const action = addLoadEventToLoadAmountStatistics({ key: 'testKey', event });
            expect(action.type).toBe('[Statistics] Add Load Event To Load Amount Statistics');
            expect(action.key).toBe('testKey');
            expect(action.event).toBe(event);
        });
    });

    describe('Slot Statistics Actions', () => {
        it('should create downloadSlotStatistics action', () => {
            const action = downloadSlotStatistics({ key: 'testKey' });
            expect(action.type).toBe('[Statistics] Download Slot Statistics');
            expect(action.key).toBe('testKey');
        });

        it('should create downLoadSlotStatisticsFailure action', () => {
            const action = downLoadSlotStatisticsFailure({ error });
            expect(action.type).toBe('[Statistics] Download Slot Statistics Failure');
            expect(action.error).toEqual(error);
        });
    });
});