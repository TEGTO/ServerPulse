import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { of, throwError } from 'rxjs';
import { ServerSlotDataCollector } from '../..';
import { JsonDownloader, SlotDataResponse } from '../../../shared';
import { SlotInfoDownloadButtonComponent } from './slot-info-download-button.component';

describe('SlotInfoDownloadButtonComponent', () => {
  let component: SlotInfoDownloadButtonComponent;
  let fixture: ComponentFixture<SlotInfoDownloadButtonComponent>;
  let mockDataCollector: jasmine.SpyObj<ServerSlotDataCollector>;
  let mockJsonDownloader: jasmine.SpyObj<JsonDownloader>;

  beforeEach(async () => {
    mockDataCollector = jasmine.createSpyObj('ServerSlotDataCollector', ['getServerSlotData']);
    mockJsonDownloader = jasmine.createSpyObj('JsonDownloader', ['downloadInJson']);

    await TestBed.configureTestingModule({
      declarations: [SlotInfoDownloadButtonComponent],
      providers: [
        { provide: ServerSlotDataCollector, useValue: mockDataCollector },
        { provide: JsonDownloader, useValue: mockJsonDownloader }
      ]
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(SlotInfoDownloadButtonComponent);
    component = fixture.componentInstance;
    component.slotKey = 'test-slot-key';
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have the correct slot data file name', () => {
    expect(component.slotDataFileName).toBe('slot-data-test-slot-key');
  });

  it('should call downloadData when the button is clicked', () => {
    spyOn(component, 'downloadData');

    const button = fixture.debugElement.query(By.css('button'));
    button.triggerEventHandler('click', null);

    expect(component.downloadData).toHaveBeenCalled();
  });

  it('should initiate data download and call JsonDownloader on success', () => {
    const mockData: SlotDataResponse = {
      collectedDateUTC: new Date(),
      generalStatistics: null,
      loadStatistics: null,
      customEventStatistics: null,
      lastLoadEvents: [],
      lastCustomEvents: []
    };
    mockDataCollector.getServerSlotData.and.returnValue(of(mockData));

    component.downloadData();

    expect(mockDataCollector.getServerSlotData).toHaveBeenCalledWith('test-slot-key');
    expect(mockJsonDownloader.downloadInJson).toHaveBeenCalledWith(mockData, 'slot-data-test-slot-key');
  });

  it('should handle errors during data download', () => {
    const consoleErrorSpy = spyOn(console, 'error');
    mockDataCollector.getServerSlotData.and.returnValue(throwError(() => new Error('Download error')));

    component.downloadData();

    expect(mockDataCollector.getServerSlotData).toHaveBeenCalledWith('test-slot-key');
    expect(consoleErrorSpy).toHaveBeenCalledWith('Error downloading data:', jasmine.any(Error));
  });

  it('should cancel the download and unsubscribe when ngOnDestroy is called', () => {
    spyOn(component as any, 'cancelDownload'); // Spy on the private cancelDownload method

    component.ngOnDestroy();

    expect(component['cancelDownload']).toHaveBeenCalled();
  });

  it('should not initiate another download if one is already in progress', () => {
    component['isRequestInProgress'] = true;

    component.downloadData();

    expect(mockDataCollector.getServerSlotData).not.toHaveBeenCalled();
  });
});