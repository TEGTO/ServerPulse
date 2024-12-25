/* eslint-disable @typescript-eslint/no-explicit-any */
import { addNewCustomEvent, addNewLoadEvent, getDailyLoadAmountStatisticsSuccess, getLoadAmountStatisticsInRangeSuccess, getSomeCustomEventsSuccess, getSomeLoadEventsSuccess, setCustomReadFromDate, setReadFromDate, setSelectedDate } from "..";
import { getDefaultCustomEvent, getDefaultLoadAmountStatistics, getDefaultLoadEvent } from "../../analyzer";
import { slotInfoStateReducer } from "./info.reducer";

describe('Slot Info Reducer', () => {
    const initialState = {
        selectedDate: new Date(),
        readFromDate: new Date(),
        customReadFromDate: new Date(),
        loadEvents: [],
        customEvents: [],
        loadAmountStatistics: [],
        secondaryLoadAmountStatistics: [],
    };

    it('should return the initial state by default', () => {
        const action = { type: 'UNKNOWN' } as any;
        const state = slotInfoStateReducer(initialState, action);
        expect(state).toEqual(initialState);
    });

    describe('Date Actions', () => {
        it('should set selectedDate and readFromDate on setSelectedDate', () => {
            const selectedDate = new Date(2024, 11, 25);
            const readFromDate = new Date(2024, 11, 24);
            const action = setSelectedDate({ date: selectedDate, readFromDate });
            const state = slotInfoStateReducer(initialState, action);

            expect(state.selectedDate).toEqual(selectedDate);
            expect(state.readFromDate).toEqual(readFromDate);
            expect(state.loadEvents).toEqual([]);
        });

        it('should set readFromDate on setReadFromDate', () => {
            const readFromDate = new Date(2024, 11, 24);
            const action = setReadFromDate({ date: readFromDate });
            const state = slotInfoStateReducer(initialState, action);

            expect(state.readFromDate).toEqual(readFromDate);
        });

        it('should set customReadFromDate on setCustomReadFromDate', () => {
            const customReadFromDate = new Date(2024, 11, 24);
            const action = setCustomReadFromDate({ date: customReadFromDate });
            const state = slotInfoStateReducer(initialState, action);

            expect(state.customReadFromDate).toEqual(customReadFromDate);
        });
    });

    describe('Load Events Actions', () => {
        it('should add unique load events on getSomeLoadEventsSuccess', () => {
            const loadEvents = [
                { ...getDefaultLoadEvent(), id: "1" },
                { ...getDefaultLoadEvent(), id: "2" },
            ];
            const action = getSomeLoadEventsSuccess({ events: loadEvents });
            const state = slotInfoStateReducer(initialState, action);

            expect(state.loadEvents).toEqual(loadEvents);
        });

        it('should filter out duplicate load events on getSomeLoadEventsSuccess', () => {
            const initialLoadEvents = [
                { ...getDefaultLoadEvent(), id: "1" },
            ];
            const newLoadEvents = [
                { ...getDefaultLoadEvent(), id: "2" },
                { ...getDefaultLoadEvent(), id: "2" },
            ];
            const initialStateWithEvents = { ...initialState, loadEvents: initialLoadEvents };
            const action = getSomeLoadEventsSuccess({ events: newLoadEvents });
            const state = slotInfoStateReducer(initialStateWithEvents, action);

            expect(state.loadEvents.length).toBe(2);
            expect(state.loadEvents[1].id).toBe('2');
        });

        it('should add a new load event on addNewLoadEvent', () => {
            const loadEvent = { ...getDefaultLoadEvent(), id: "2" };
            const action = addNewLoadEvent({ event: loadEvent });
            const state = slotInfoStateReducer(initialState, action);

            expect(state.loadEvents[0]).toEqual(loadEvent);
        });

        it('should not add duplicate load event on addNewLoadEvent', () => {
            const initialLoadEvents = [
                { ...getDefaultLoadEvent(), id: "1" }
            ];
            const initialStateWithEvents = { ...initialState, loadEvents: initialLoadEvents };
            const loadEvent = { ...getDefaultLoadEvent(), id: "1" };
            const action = addNewLoadEvent({ event: loadEvent });
            const state = slotInfoStateReducer(initialStateWithEvents, action);

            expect(state.loadEvents.length).toBe(1);
        });
    });

    describe('Custom Events Actions', () => {
        it('should add unique custom events on getSomeCustomEventsSuccess', () => {
            const customEvents = [
                { ...getDefaultCustomEvent(), id: "1" },
                { ...getDefaultCustomEvent(), id: "2" },
            ];
            const action = getSomeCustomEventsSuccess({ events: customEvents });
            const state = slotInfoStateReducer(initialState, action);

            expect(state.customEvents).toEqual(customEvents);
        });

        it('should add a new custom event on addNewCustomEvent', () => {
            const customEvent = { ...getDefaultCustomEvent(), id: "3" };
            const action = addNewCustomEvent({ event: customEvent });
            const state = slotInfoStateReducer(initialState, action);

            expect(state.customEvents[0]).toEqual(customEvent);
        });
    });

    describe('Load Amount Statistics Actions', () => {
        it('should set loadAmountStatistics on getDailyLoadAmountStatisticsSuccess', () => {
            const statistics = [
                { ...getDefaultLoadAmountStatistics(), id: "1" }
            ];
            const action = getDailyLoadAmountStatisticsSuccess({ statistics });
            const state = slotInfoStateReducer(initialState, action);

            expect(state.loadAmountStatistics).toEqual(statistics);
        });

        it('should set secondaryLoadAmountStatistics on getLoadAmountStatisticsInRangeSuccess', () => {
            const statistics = [
                { ...getDefaultLoadAmountStatistics(), id: "1" }
            ];
            const action = getLoadAmountStatisticsInRangeSuccess({ statistics });
            const state = slotInfoStateReducer(initialState, action);

            expect(state.secondaryLoadAmountStatistics).toEqual(statistics);
        });
    });
});