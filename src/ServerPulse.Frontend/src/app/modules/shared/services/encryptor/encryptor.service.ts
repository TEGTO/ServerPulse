import { Injectable } from "@angular/core";
import * as CryptoJS from 'crypto-js';
import { environment } from "../../../../../environment/environment";

@Injectable({
    providedIn: 'root'
})
export class Encryptor {
    private readonly secretKey = environment.ecryptionSecretKey;

    encryptString(str: string): string {
        const encrypted = CryptoJS.AES.encrypt(str, this.secretKey).toString();
        return encrypted;
    }

    decryptString(str: string): string {
        const bytes = CryptoJS.AES.decrypt(str, this.secretKey);
        const decryptedString = bytes.toString(CryptoJS.enc.Utf8);
        return decryptedString;
    }
}
