import { NO_ERRORS_SCHEMA } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { By } from '@angular/platform-browser';
import { CustomEventDetailsComponent } from './custom-event-details.component';

describe('CustomEventDetailsComponent', () => {
  let component: CustomEventDetailsComponent;
  let fixture: ComponentFixture<CustomEventDetailsComponent>;

  const validJson = '{"name": "Test Event", "type": "Custom", "id": 123}';
  const invalidJson = '{"name": "Test Event", "type": "Custom", "id": 123';

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [CustomEventDetailsComponent],
      providers: [
        { provide: MAT_DIALOG_DATA, useValue: validJson }
      ],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(CustomEventDetailsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should parse JSON and populate jsonData array on init', () => {
    expect(component.jsonData.length).toBe(3);
    expect(component.jsonData).toEqual([
      { key: 'name', value: 'Test Event' },
      { key: 'type', value: 'Custom' },
      { key: 'id', value: 123 }
    ]);
  });

  it('should handle invalid JSON gracefully', () => {
    spyOn(console, 'error');

    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      declarations: [CustomEventDetailsComponent],
      providers: [
        { provide: MAT_DIALOG_DATA, useValue: invalidJson }
      ],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();

    fixture = TestBed.createComponent(CustomEventDetailsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    expect(component.jsonData.length).toBe(0);
    expect(console.error).toHaveBeenCalledWith('Invalid JSON format', jasmine.any(SyntaxError));
  });

  it('should render key-value pairs in the template', () => {
    const items = fixture.debugElement.queryAll(By.css('.details__item'));

    expect(items.length).toBe(3);
    expect(items[0].nativeElement.textContent).toContain('name:');
    expect(items[0].nativeElement.textContent).toContain('Test Event');
    expect(items[1].nativeElement.textContent).toContain('type:');
    expect(items[1].nativeElement.textContent).toContain('Custom');
    expect(items[2].nativeElement.textContent).toContain('id:');
    expect(items[2].nativeElement.textContent).toContain('123');
  });
});