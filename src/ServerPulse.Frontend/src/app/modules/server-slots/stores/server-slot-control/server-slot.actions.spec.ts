import { createServerSlot, createServerSlotFailure, createServerSlotSuccess, deleteServerSlot, deleteServerSlotFailure, deleteServerSlotSuccess, getServerSlots, getServerSlotsFailure, getServerSlotsSuccess, getServerSlotsWithString, getServerSlotsWithStringFailure, getServerSlotsWithStringSuccess, updateServerSlot, updateServerSlotFailure, updateServerSlotSuccess } from "../..";
import { CreateServerSlotRequest, ServerSlot, UpdateServerSlotRequest } from "../../../shared";

describe('Server Slot Actions', () => {
    const error = { message: 'An error occurred' };

    describe('Get Server Slots Actions', () => {
        it('should create getServerSlots action', () => {
            const action = getServerSlots();
            expect(action.type).toBe('[Server Slot] Get Server Slots Of The User');
        });
        it('should create getServerSlotsSuccess action', () => {
            const serverSlots: ServerSlot[] = [];
            const action = getServerSlotsSuccess({ serverSlots });
            expect(action.type).toBe('[Server Slot] Get Server Slots Of The User Success');
            expect(action.serverSlots).toEqual(serverSlots);
        });
        it('should create getServerSlotsFailure action', () => {
            const action = getServerSlotsFailure({ error });
            expect(action.type).toBe('[Server Slot] Get Server Slots Of The User Failure');
            expect(action.error).toEqual(error);
        });
    });

    describe('Get Server Slots With String Actions', () => {
        it('should create getServerSlotsWithString action', () => {
            const action = getServerSlotsWithString({ str: "str" });
            expect(action.type).toBe('[Server Slot] Get Server Slots That Contain The String');
            expect(action.str).toEqual("str");
        });
        it('should create getServerSlotsWithStringSuccess action', () => {
            const serverSlots: ServerSlot[] = [];
            const action = getServerSlotsWithStringSuccess({ serverSlots });
            expect(action.type).toBe('[Server Slot] Get Server Slots That Contain The String Success');
            expect(action.serverSlots).toEqual(serverSlots);
        });
        it('should create getServerSlotsWithStringFailure action', () => {
            const action = getServerSlotsWithStringFailure({ error });
            expect(action.type).toBe('[Server Slot] Get Server Slots That Contain The String Failure');
            expect(action.error).toEqual(error);
        });
    });

    describe('Create A New Server Slot Actions', () => {
        it('should create createServerSlot action', () => {
            const req: CreateServerSlotRequest =
            {
                name: "name"
            }
            const action = createServerSlot({ createRequest: req });
            expect(action.type).toBe('[Server Slot] Create A New Server Slot');
            expect(action.createRequest).toEqual(req);
        });
        it('should create createServerSlotSuccess action', () => {
            const serverSlot: ServerSlot = {
                id: "id",
                userEmail: "userEmail",
                name: "name",
                slotKey: "slotKey",
            };
            const action = createServerSlotSuccess({ serverSlot: serverSlot });
            expect(action.type).toBe('[Server Slot] Create A New Server Slot Success');
            expect(action.serverSlot).toEqual(serverSlot);
        });
        it('should create createServerSlotFailure action', () => {
            const action = createServerSlotFailure({ error });
            expect(action.type).toBe('[Server Slot] Create A New Server Slot Failure');
            expect(action.error).toEqual(error);
        });
    });

    describe('Update A Server Slot Actions', () => {
        it('should create updateServerSlot action', () => {
            const req: UpdateServerSlotRequest =
            {
                id: "id",
                name: "name"
            }
            const action = updateServerSlot({ updateRequest: req });
            expect(action.type).toBe('[Server Slot] Update A Server Slot');
            expect(action.updateRequest).toEqual(req);
        });
        it('should create updateServerSlotSuccess action', () => {
            const req: UpdateServerSlotRequest =
            {
                id: "id",
                name: "name"
            }
            const action = updateServerSlotSuccess({ updateRequest: req });
            expect(action.type).toBe('[Server Slot] Update A Server Slot Success');
            expect(action.updateRequest).toEqual(req);
        });
        it('should create updateServerSlotFailure action', () => {
            const action = updateServerSlotFailure({ error });
            expect(action.type).toBe('[Server Slot] Update A Server Slot Failure');
            expect(action.error).toEqual(error);
        });
    });

    describe('Delete A Server Slot Actions', () => {
        it('should create deleteServerSlot action', () => {
            const id = "id";
            const action = deleteServerSlot({ id: id });
            expect(action.type).toBe('[Server Slot] Delete A Server Slot');
            expect(action.id).toEqual(id);
        });
        it('should create deleteServerSlotSuccess action', () => {
            const id = "id";
            const action = deleteServerSlotSuccess({ id: id });
            expect(action.type).toBe('[Server Slot] Delete A Server Slot Success');
            expect(action.id).toEqual(id);
        });
        it('should create deleteServerSlotFailure action', () => {
            const action = deleteServerSlotFailure({ error });
            expect(action.type).toBe('[Server Slot] Delete A Server Slot Failure');
            expect(action.error).toEqual(error);
        });
    });
});