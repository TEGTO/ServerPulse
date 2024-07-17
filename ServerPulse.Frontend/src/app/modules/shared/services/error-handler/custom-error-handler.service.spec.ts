import { TestBed } from "@angular/core/testing";
import { CustomErrorHandler, getStatusCodeDescription } from "./custom-error-handler.service";

describe('CustomErrorHandler', () => {
    let service: CustomErrorHandler;

    beforeEach(() => {
        TestBed.configureTestingModule({});
        service = TestBed.inject(CustomErrorHandler);
    });

    it('should return joined error messages if error.error.messages exists', () => {
        const error = {
            error: {
                messages: ['Error 1', 'Error 2']
            }
        };
        const result = service.handleApiError(error);
        expect(result).toBe('Error 1\nError 2');
    });

    it('should return error.message if error.message exists', () => {
        const error = {
            message: 'An error occurred'
        };
        const result = service.handleApiError(error);
        expect(result).toBe('An error occurred');
    });

    it('should return status code description if no error messages are available', () => {
        const error = {
            status: 404
        };
        const result = service.handleApiError(error);
        expect(result).toBe('An unknown error occurred! (Not Found)');
    });

    it('should return "Unknown Status Code" for unknown status code', () => {
        const error = {
            status: 999
        };
        const result = service.handleApiError(error);
        expect(result).toBe('An unknown error occurred! (Unknown Status Code)');
    });

    it('should return default message for empty error', () => {
        const error = {};
        const result = service.handleApiError(error);
        expect(result).toBe('An unknown error occurred! (Unknown Status Code)');
    });
});

describe('getStatusCodeDescription', () => {
    it('should return correct description for known status code', () => {
        const result = getStatusCodeDescription(404);
        expect(result).toBe('Not Found');
    });

    it('should return "Unknown Status Code" for unknown status code', () => {
        const result = getStatusCodeDescription(999);
        expect(result).toBe('Unknown Status Code');
    });
});