/* eslint-disable @typescript-eslint/no-explicit-any */
import { TestBed } from '@angular/core/testing';
import { JsonDownloaderService } from './json-downloader.service';

describe('JsonDownloaderService', () => {
  let service: JsonDownloaderService;
  let createObjectURLSpy: jasmine.Spy;
  let revokeObjectURLSpy: jasmine.Spy;
  let clickSpy: jasmine.Spy;
  let mockAnchorElement: HTMLAnchorElement;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [JsonDownloaderService],
    });
    service = TestBed.inject(JsonDownloaderService);

    mockAnchorElement = document.createElement('a');
    clickSpy = spyOn(mockAnchorElement, 'click');

    spyOn(document, 'createElement').and.callFake((tagName: string) => {
      if (tagName === 'a') {
        return mockAnchorElement;
      }
      return document.createElement(tagName);
    });

    createObjectURLSpy = spyOn(window.URL, 'createObjectURL').and.returnValue('blob:http://example.com/123456');
    revokeObjectURLSpy = spyOn(window.URL, 'revokeObjectURL');
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should create a JSON file and trigger download', () => {
    const testData = { key: 'value' };
    const testFileName = 'test.json';
    const mockBlob = new Blob([JSON.stringify(testData)], { type: 'application/json' });

    spyOn(service as any, 'convertToJsonFile').and.returnValue(mockBlob);

    service.downloadInJson(testData, testFileName);

    expect(service['convertToJsonFile']).toHaveBeenCalledWith(testData);
    expect(createObjectURLSpy).toHaveBeenCalledWith(mockBlob);
    expect(mockAnchorElement.href).toBe('blob:http://example.com/123456');
    expect(mockAnchorElement.download).toBe(testFileName);
    expect(clickSpy).toHaveBeenCalled();
    expect(revokeObjectURLSpy).toHaveBeenCalledWith('blob:http://example.com/123456');
  });
});