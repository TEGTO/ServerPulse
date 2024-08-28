import { Injectable } from "@angular/core";

@Injectable({
    providedIn: 'root'
})
export abstract class JsonDownloader {
    abstract downloadInJson(objectToDownload: any, fileName: string): void;
}
