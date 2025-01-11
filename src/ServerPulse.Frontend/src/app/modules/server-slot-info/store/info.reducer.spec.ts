/* eslint-disable @typescript-eslint/no-explicit-any */
import { addNewCustomEvent, addNewLoadEvent, getDailyLoadAmountStatisticsSuccess, getLoadAmountStatisticsInRangeSuccess, getSomeCustomEventsSuccess, setCustomReadFromDate, setLoadStatisticsInterval, setReadFromDate, setSecondaryLoadStatisticsInterval, setSelectedDate } from "..";
import { getDefaultCustomEvent, getDefaultLoadAmountStatistics, getDefaultLoadEvent, LoadEvent } from "../../analyzer";
import { SlotInfoState, slotInfoStateReducer } from "./info.reducer";

describe('Slot Info Reducer', () => {
    const initialState: SlotInfoState = {
        selectedDate: new Date(),
        readFromDate: new Date(),
        loadStatisticsInterval: 0,
        secondaryLoadStatisticsInterval: 0,
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
        it('should add a new LoadEvent when addNewLoadEvent is dispatched', () => {
            const newLoadEvent: LoadEvent = {
                ...getDefaultLoadEvent(),
                timestampUTC: new Date(),
            };

            const interval = 24 * 60 * 60 * 1000; // 1 day in milliseconds
            const initialStateWithInterval: SlotInfoState = {
                ...initialState,
                loadStatisticsInterval: interval,
                secondaryLoadStatisticsInterval: interval,
            };

            const action = addNewLoadEvent({ event: newLoadEvent });

            const updatedState = slotInfoStateReducer(initialStateWithInterval, action);

            // Check loadEvents
            expect(updatedState.loadEvents.length).toEqual(1);
            expect(updatedState.loadEvents[0]).toEqual(newLoadEvent);

            // Check loadAmountStatistics
            expect(updatedState.loadAmountStatistics.length).toEqual(1);
            expect(updatedState.loadAmountStatistics[0].amountOfEvents).toBe(1);

            // Check secondaryLoadAmountStatistics
            expect(updatedState.secondaryLoadAmountStatistics.length).toEqual(1);
            expect(updatedState.secondaryLoadAmountStatistics[0].amountOfEvents).toBe(1);
        });

        it('should not add duplicate LoadEvent when addNewLoadEvent is dispatched', () => {
            const existingLoadEvent: LoadEvent = {
                ...getDefaultLoadEvent(),
                timestampUTC: new Date(),
            };

            const interval = 24 * 60 * 60 * 1000; // 1 day in milliseconds
            const stateWithExistingEvent: SlotInfoState = {
                ...initialState,
                loadStatisticsInterval: interval,
                secondaryLoadStatisticsInterval: interval,
                loadEvents: [existingLoadEvent],
            };

            const action = addNewLoadEvent({ event: existingLoadEvent });

            const updatedState = slotInfoStateReducer(stateWithExistingEvent, action);

            // Check that loadEvents remain unchanged
            expect(updatedState.loadEvents.length).toEqual(1);
            expect(updatedState.loadEvents[0]).toEqual(existingLoadEvent);

            // Check that statistics are unchanged
            expect(updatedState.loadAmountStatistics.length).toEqual(0);
            expect(updatedState.secondaryLoadAmountStatistics.length).toEqual(0);
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
        it('should update loadStatisticsInterval when setLoadStatisticsInterval is dispatched', () => {
            const interval = 3600000; // 1 hour in milliseconds
            const action = setLoadStatisticsInterval({ interval });

            const updatedState = slotInfoStateReducer(initialState, action);

            expect(updatedState.loadStatisticsInterval).toBe(interval);
        });

        it('should update secondaryLoadStatisticsInterval when setSecondaryLoadStatisticsInterval is dispatched', () => {
            const interval = 7200000; // 2 hours in milliseconds
            const action = setSecondaryLoadStatisticsInterval({ interval });

            const updatedState = slotInfoStateReducer(initialState, action);

            expect(updatedState.secondaryLoadStatisticsInterval).toBe(interval);
        });

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