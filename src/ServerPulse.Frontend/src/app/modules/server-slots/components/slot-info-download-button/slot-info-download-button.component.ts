import { Component, Input, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';
import { ServerSlotDataCollector } from '../..';
import { JsonDownloader } from '../../../shared';

@Component({
  selector: 'slot-info-download-button',
  templateUrl: './slot-info-download-button.component.html',
  styleUrl: './slot-info-download-button.component.scss'
})
export class SlotInfoDownloadButtonComponent implements OnDestroy {
  @Input({ required: true }) slotKey!: string;

  private currentRequestSubscription?: Subscription;
  private isRequestInProgress: boolean = false;

  get slotDataFileName() { return `slot-data-${this.slotKey}`; }

  constructor(
    private readonly dataCollector: ServerSlotDataCollector,
    private readonly jsonDownloader: JsonDownloader
  ) { }

  ngOnDestroy() {
    this.cancelDownload();
  }

  downloadData() {
    if (this.isRequestInProgress) {
      return;
    }

    this.isRequestInProgress = true;

    this.currentRequestSubscription = this.dataCollector.getServerSlotData(this.slotKey).subscribe({
      next: (data) => {
        this.jsonDownloader.downloadInJson(data, this.slotDataFileName);
      },
      error: (err) => {
        console.error('Error downloading data:', err);
      },
      complete: () => {
        this.cancelDownload();
      }
    });
  }

  cancelDownload() {
    if (this.currentRequestSubscription) {
      this.currentRequestSubscription.unsubscribe();
      this.isRequestInProgress = false;
      this.currentRequestSubscription = undefined;
    }
  }
}