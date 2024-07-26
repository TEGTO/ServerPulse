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
        let str = "some string";
        let ecryptedString = service.encryptString(str);
        expect(ecryptedString).not.toBe(str);
    });

    it('should deencrypt string', () => {
        let str = "some string";
        let ecryptedString = service.encryptString(str);
        let decryptedString = service.decryptString(ecryptedString);
        expect(decryptedString).toBe(str);
    });

});