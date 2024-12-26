import { TestBed } from "@angular/core/testing";
import { Encryptor } from "./encryptor.service";

describe('LocalStorageService', () => {
    let service: Encryptor;

    beforeEach(() => {
        TestBed.configureTestingModule({});
        service = TestBed.inject(Encryptor);
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    it('should encrypt string', () => {
        const str = "some string";

        const ecryptedString = service.encryptString(str);

        expect(ecryptedString).not.toBe(str);
    });

    it('should deencrypt string', () => {
        const str = "some string";

        const ecryptedString = service.encryptString(str);

        const decryptedString = service.decryptString(ecryptedString);

        expect(decryptedString).toBe(str);
    });

});