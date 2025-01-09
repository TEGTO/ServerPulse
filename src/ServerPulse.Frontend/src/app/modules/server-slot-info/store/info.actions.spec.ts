import { CustomEvent, getLoadAmountStatisticsInRange, getLoadAmountStatisticsInRangeFailure, GetLoadAmountStatisticsInRangeRequest, getLoadAmountStatisticsInRangeSuccess, GetSomeLoadEventsRequest, LoadAmountStatistics, LoadEvent } from "../../analyzer";
import { TimeSpan } from "../../shared";
import { addNewCustomEvent, addNewLoadEvent, getDailyLoadAmountStatistics, getDailyLoadAmountStatisticsFailure, getDailyLoadAmountStatisticsSuccess, getSomeCustomEvents, getSomeCustomEventsFailure, getSomeCustomEventsSuccess, getSomeLoadEvents, getSomeLoadEventsFailure, getSomeLoadEventsSuccess, setCustomReadFromDate, setLoadStatisticsInterval, setReadFromDate, setSecondaryLoadStatisticsInterval, setSelectedDate, showCustomDetailsEvent } from "./info.actions";

describe('Statistics Actions', () => {
    const testDate = new Date();
    const testDateReadFrom = new Date(testDate.getTime() - 1000 * 60 * 60 * 24); // 1 day before
    const testLoadEvent: LoadEvent = {
        id: '1',
        key: 'key1',
        creationDateUTC: testDate,
        endpoint: '/api/test',
        method: 'GET',
        statusCode: 200,
        duration: new TimeSpan(0, 0, 1, 200),
        timestampUTC: testDate,
    };
    const testCustomEvent: CustomEvent = {
        id: '2',
        key: 'key2',
        creationDateUTC: testDate,
        name: 'Test Event',
        description: 'This is a test event',
        serializedMessage: '{"data": "test"}',
    };
    const testStatistics: LoadAmountStatistics[] = [
        { id: '3', collectedDateUTC: testDate, amountOfEvents: 10, dateFrom: testDate, dateTo: testDate },
    ];
    const testRequest: GetSomeLoadEventsRequest = {
        key: 'key3',
        numberOfMessages: 5,
        startDate: testDate,
        readNew: true,
    };
    const testAmountRequest: GetLoadAmountStatisticsInRangeRequest = {
        key: 'key4',
        from: testDate,
        to: testDate,
        timeSpan: 'PT1H',
    };
    const testError = { message: 'An error occurred' };

    describe('Date Actions', () => {
        it('should create setSelectedDate action', () => {
            const action = setSelectedDate({ date: testDate, readFromDate: testDateReadFrom });
            expect(action.type).toBe('[Statistics] Set Selected Date ');
            expect(action.date).toBe(testDate);
            expect(action.readFromDate).toBe(testDateReadFrom);
        });

        it('should create setReadFromDate action', () => {
            const action = setReadFromDate({ date: testDate });
            expect(action.type).toBe('[Statistics] Set Read From Date ');
            expect(action.date).toBe(testDate);
        });

        it('should create setCustomReadFromDate action', () => {
            const action = setCustomReadFromDate({ date: testDate });
            expect(action.type).toBe('[Statistics] Set Custom Read From Date ');
            expect(action.date).toBe(testDate);
        });
    });

    describe('Load Events Actions', () => {
        it('should create getSomeLoadEvents action', () => {
            const action = getSomeLoadEvents({ req: testRequest });
            expect(action.type).toBe('[Statistics] Get Some Load Events');
            expect(action.req).toBe(testRequest);
        });

        it('should create getSomeLoadEventsSuccess action', () => {
            const action = getSomeLoadEventsSuccess({ events: [testLoadEvent] });
            expect(action.type).toBe('[Statistics] Get Some Load Events Success');
            expect(action.events).toEqual([testLoadEvent]);
        });

        it('should create getSomeLoadEventsFailure action', () => {
            const action = getSomeLoadEventsFailure({ error: testError });
            expect(action.type).toBe('[Statistics] Get Some Load Events Failure');
            expect(action.error).toEqual(testError);
        });

        it('should create addNewLoadEvent action', () => {
            const action = addNewLoadEvent({ event: testLoadEvent });
            expect(action.type).toBe('[Statistics] Add New Load Event');
            expect(action.event).toBe(testLoadEvent);
        });
    });

    describe('Custom Events Actions', () => {
        it('should create getSomeCustomEvents action', () => {
            const action = getSomeCustomEvents({ req: testRequest });
            expect(action.type).toBe('[Statistics] Get Some Custom Events');
            expect(action.req).toBe(testRequest);
        });

        it('should create getSomeCustomEventsSuccess action', () => {
            const action = getSomeCustomEventsSuccess({ events: [testCustomEvent] });
            expect(action.type).toBe('[Statistics] Get Some Custom Events Success');
            expect(action.events).toEqual([testCustomEvent]);
        });

        it('should create getSomeCustomEventsFailure action', () => {
            const action = getSomeCustomEventsFailure({ error: testError });
            expect(action.type).toBe('[Statistics] Get Some Custom Events Failure');
            expect(action.error).toEqual(testError);
        });

        it('should create addNewCustomEvent action', () => {
            const action = addNewCustomEvent({ event: testCustomEvent });
            expect(action.type).toBe('[Statistics] Add New Custom Event');
            expect(action.event).toBe(testCustomEvent);
        });

        it('should create showCustomDetailsEvent action', () => {
            const action = showCustomDetailsEvent({ event: testCustomEvent });
            expect(action.type).toBe('[Statistics] Show Custom Event Details');
            expect(action.event).toBe(testCustomEvent);
        });
    });

    describe('Load Amount Statistics Actions', () => {
        it('should create getDailyLoadAmountStatistics action', () => {
            const action = getDailyLoadAmountStatistics({ key: 'key1' });
            expect(action.type).toBe('[Statistics] Get Daily Load Amount Statistics');
            expect(action.key).toBe('key1');
        });

        it('should create getDailyLoadAmountStatisticsSuccess action', () => {
            const action = getDailyLoadAmountStatisticsSuccess({ statistics: testStatistics });
            expect(action.type).toBe('[Statistics] Get Daily Load Amount Statistics Success');
            expect(action.statistics).toEqual(testStatistics);
        });

        it('should create getDailyLoadAmountStatisticsFailure action', () => {
            const action = getDailyLoadAmountStatisticsFailure({ error: testError });
            expect(action.type).toBe('[Statistics] Get Daily Load Amount Statistics Failure');
            expect(action.error).toEqual(testError);
        });

        it('should create getLoadAmountStatisticsInRange action', () => {
            const action = getLoadAmountStatisticsInRange({ req: testAmountRequest });
            expect(action.type).toBe('[Statistics] Get Load Amount Statistics In Range');
            expect(action.req).toBe(testAmountRequest);
        });

        it('should create getLoadAmountStatisticsInRangeSuccess action', () => {
            const action = getLoadAmountStatisticsInRangeSuccess({ key: "key", statistics: testStatistics, timespan: new TimeSpan(0, 5, 0, 0) });
            expect(action.type).toBe('[Statistics] Get Load Amount Statistics In Range Success');
            expect(action.statistics).toEqual(testStatistics);
        });

        it('should create getLoadAmountStatisticsInRangeFailure action', () => {
            const action = getLoadAmountStatisticsInRangeFailure({ error: testError });
            expect(action.type).toBe('[Statistics] Get Load Amount Statistics In Range Failure');
            expect(action.error).toEqual(testError);
        });

        it('should create setLoadStatisticsInterval action', () => {
            const action = setLoadStatisticsInterval({ interval: 0 });
            expect(action.type).toBe('[Statistics] Set Load Statistics Interval');
            expect(action.interval).toBe(0);
        });

        it('should create setSecondaryLoadStatisticsInterval action', () => {
            const action = setSecondaryLoadStatisticsInterval({ interval: 0 });
            expect(action.type).toBe('[Statistics] Set Secondary Load Statistics Interval');
            expect(action.interval).toBe(0);
        });
    });
});
