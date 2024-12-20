/* eslint-disable @typescript-eslint/no-explicit-any */
import { Injectable } from '@angular/core';
import { JsonDownloader } from './json-downloader';

@Injectable({
  providedIn: 'root'
})
export class JsonDownloaderService implements JsonDownloader {

  downloadInJson(objectToDownload: any, fileName: string): void {
    const blob = this.convertToJsonFile(objectToDownload);
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = fileName;
    a.click();
    window.URL.revokeObjectURL(url);
  }

  private convertToJsonFile(objectToDownload: any) {
    const jsonString = JSON.stringify(objectToDownload);
    const blob = new Blob([jsonString], { type: 'application/json' });
    return blob;
  }
}
