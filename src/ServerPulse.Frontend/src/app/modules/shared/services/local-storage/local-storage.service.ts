import { Injectable } from '@angular/core';
import { Encryptor } from '../..';

@Injectable({
  providedIn: 'root'
})
export class LocalStorageService {

  constructor(
    private readonly encryptor: Encryptor
  ) { }

  setItem(key: string, value: string, ecryptValue = true): void {
    if (ecryptValue) {
      value = this.encryptor.encryptString(value);
    }
    localStorage.setItem(key, value);
  }

  getItem(key: string, decryptValue = true): string | null {
    let value = localStorage.getItem(key);
    if (decryptValue && value) {
      value = this.encryptor.decryptString(value);
    }
    return value;
  }

  removeItem(key: string): void {
    localStorage.removeItem(key);
  }

  clear(): void {
    localStorage.clear();
  }
}