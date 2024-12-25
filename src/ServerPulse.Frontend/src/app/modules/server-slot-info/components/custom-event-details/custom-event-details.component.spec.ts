import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { By } from '@angular/platform-browser';
import { CustomEventDetailsComponent } from './custom-event-details.component';

describe('CustomEventDetailsComponent', () => {
  let component: CustomEventDetailsComponent;
  let fixture: ComponentFixture<CustomEventDetailsComponent>;

  const validSerializedEvent = JSON.stringify({
    key1: 'value1',
    key2: 123,
    key3: true
  });

  const invalidSerializedEvent = "Invalid JSON String";

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [CustomEventDetailsComponent],
      imports: [MatDialogModule],
      providers: [
        { provide: MAT_DIALOG_DATA, useValue: validSerializedEvent },
      ]
    }).compileComponents();
  });

  it('should create the component', () => {
    fixture = TestBed.createComponent(CustomEventDetailsComponent);
    component = fixture.componentInstance;

    expect(component).toBeTruthy();
  });

  describe('ngOnInit', () => {
    it('should parse valid JSON and populate jsonData', () => {
      fixture = TestBed.createComponent(CustomEventDetailsComponent);
      component = fixture.componentInstance;

      component.ngOnInit();
      expect(component.jsonData).toEqual([
        { key: 'key1', value: 'value1' },
        { key: 'key2', value: 123 },
        { key: 'key3', value: true },
      ]);
    });

    it('should log error for invalid JSON', () => {
      const consoleErrorSpy = spyOn(console, 'error');

      TestBed.overrideProvider(MAT_DIALOG_DATA, { useValue: invalidSerializedEvent });
      fixture = TestBed.createComponent(CustomEventDetailsComponent);
      component = fixture.componentInstance;

      component.ngOnInit();
      expect(consoleErrorSpy).toHaveBeenCalledWith('Invalid JSON format', jasmine.any(Error));
    });
  });

  describe('HTML Template', () => {
    beforeEach(() => {
      fixture = TestBed.createComponent(CustomEventDetailsComponent);
      component = fixture.componentInstance;

      component.ngOnInit();
      fixture.detectChanges();
    });

    it('should display a close button', () => {
      const closeButton = fixture.debugElement.query(By.css('button.close-button'));
      expect(closeButton).toBeTruthy();
    });

    it('should display the event details title', () => {
      const dialogTitle = fixture.debugElement.query(By.css('h2[mat-dialog-title]')).nativeElement;
      expect(dialogTitle.textContent).toContain('Event Details');
    });

    it('should render JSON data as a list', () => {
      const listItems = fixture.debugElement.queryAll(By.css('.details__item'));
      expect(listItems.length).toBe(3);

      expect(listItems[0].nativeElement.textContent.trim()).toContain('key1:');
      expect(listItems[0].nativeElement.textContent.trim()).toContain('value1');

      expect(listItems[1].nativeElement.textContent.trim()).toContain('key2:');
      expect(listItems[1].nativeElement.textContent.trim()).toContain('123');

      expect(listItems[2].nativeElement.textContent.trim()).toContain('key3:');
      expect(listItems[2].nativeElement.textContent.trim()).toContain('true');
    });
  });
});