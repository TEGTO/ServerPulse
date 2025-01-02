import { CommonModule } from '@angular/common';
import { ComponentFixture, fakeAsync, TestBed, tick } from '@angular/core/testing';
import { MatButtonModule } from '@angular/material/button';
import { MAT_SNACK_BAR_DATA, MatSnackBarModule, MatSnackBarRef } from '@angular/material/snack-bar';
import { By } from '@angular/platform-browser';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { InfoCopyAnnotatedComponent } from './info-copy-annotated.component';

describe('InfoCopyAnnotatedComponent', () => {
  let fixture: ComponentFixture<InfoCopyAnnotatedComponent>;
  let mockSnackBarRef: MatSnackBarRef<InfoCopyAnnotatedComponent>;
  let clipboardSpy: jasmine.Spy;

  beforeEach(async () => {
    mockSnackBarRef = jasmine.createSpyObj('MatSnackBarRef', ['dismissWithAction']);

    clipboardSpy = spyOn(navigator.clipboard, 'writeText').and.returnValue(Promise.resolve());

    await TestBed.configureTestingModule({
      imports: [
        MatSnackBarModule,
        NoopAnimationsModule,
        CommonModule,
        MatButtonModule,
        InfoCopyAnnotatedComponent
      ],
      providers: [
        { provide: MAT_SNACK_BAR_DATA, useValue: { message: 'Info 1', copyMessage: 'Copied Info' } },
        { provide: MatSnackBarRef, useValue: mockSnackBarRef }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(InfoCopyAnnotatedComponent);
    fixture.detectChanges();
  });

  it('should display info message', () => {
    const messages = fixture.debugElement.queryAll(By.css('.text-base'));
    expect(messages.length).toBe(1);
    expect(messages[0].nativeElement.textContent).toContain('Info 1');
  });

  it('should copy message to clipboard on copy button click', fakeAsync(() => {
    const copyButton = fixture.debugElement.query(By.css('button'));
    copyButton.nativeElement.click();

    expect(clipboardSpy).toHaveBeenCalledWith('Copied Info');

    fixture.detectChanges()
    tick();

    expect(mockSnackBarRef.dismissWithAction).toHaveBeenCalled();
  }));

  it('should log an error if clipboard copy fails', fakeAsync(() => {
    const consoleSpy = spyOn(console, 'error');
    const error = new Error('Clipboard error');
    clipboardSpy.and.returnValue(Promise.reject(error));

    const copyButton = fixture.debugElement.query(By.css('button'));
    copyButton.nativeElement.click();

    fixture.detectChanges()
    tick();

    expect(consoleSpy).toHaveBeenCalledWith('Failed to copy text: ', error);
    expect(mockSnackBarRef.dismissWithAction).not.toHaveBeenCalled();
  }));
});
