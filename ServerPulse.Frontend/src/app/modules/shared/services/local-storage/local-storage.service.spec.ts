import { TestBed } from '@angular/core/testing';

import { LocalStorageService } from './local-storage.service';

describe('LocalStorageService', () => {
  let service: LocalStorageService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(LocalStorageService);
  });

  afterEach(() => {
    localStorage.clear();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should set item in localStorage', () => {
    service.setItem('testKey', 'testValue');
    expect(localStorage.getItem('testKey')).toEqual('testValue');
  });

  it('should get item from localStorage', () => {
    localStorage.setItem('testKey', 'testValue');
    const item = service.getItem('testKey');
    expect(item).toEqual('testValue');
  });

  it('should return null for non-existing item', () => {
    const item = service.getItem('nonExistingKey');
    expect(item).toBeNull();
  });

  it('should remove item from localStorage', () => {
    localStorage.setItem('testKey', 'testValue');
    service.removeItem('testKey');
    expect(localStorage.getItem('testKey')).toBeNull();
  });

  it('should clear localStorage', () => {
    localStorage.setItem('testKey1', 'testValue1');
    localStorage.setItem('testKey2', 'testValue2');
    service.clear();
    expect(localStorage.length).toBe(0);
  });
});
