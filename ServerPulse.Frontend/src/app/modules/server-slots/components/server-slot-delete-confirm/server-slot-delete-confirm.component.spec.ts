import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { By } from '@angular/platform-browser';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { ServerSlotDeleteConfirmComponent } from './server-slot-delete-confirm.component';

describe('ServerSlotDeleteConfirmComponent', () => {
  let component: ServerSlotDeleteConfirmComponent;
  let fixture: ComponentFixture<ServerSlotDeleteConfirmComponent>;
  let mockDialogRef: jasmine.SpyObj<MatDialogRef<ServerSlotDeleteConfirmComponent>>;

  beforeEach(async () => {
    mockDialogRef = jasmine.createSpyObj('MatDialogRef', ['close']);

    await TestBed.configureTestingModule({
      imports: [
        MatDialogModule,
        NoopAnimationsModule
      ],
      declarations: [ServerSlotDeleteConfirmComponent],
      providers: [
        { provide: MatDialogRef, useValue: mockDialogRef }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ServerSlotDeleteConfirmComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render the title and content correctly', () => {
    const compiled = fixture.nativeElement as HTMLElement;
    const title = compiled.querySelector('h2');
    const content = compiled.querySelector('mat-dialog-content');
    expect(title?.textContent).toContain('Delete slot');
    expect(content?.textContent).toContain('All collected data will also be deleted. Do you want to delete a slot?');
  });

  it('should close the dialog when the close button is clicked', () => {
    const closeButton = fixture.debugElement.query(By.css('.close-button')).nativeElement;
    closeButton.click();
    fixture.detectChanges();
    expect(mockDialogRef.close).toHaveBeenCalled();
  });

  it('should close the dialog with false when the No button is clicked', () => {
    const noButton = fixture.debugElement.query(By.css('button[color="primary"]')).nativeElement;
    noButton.click();
    fixture.detectChanges();
    expect(mockDialogRef.close).toHaveBeenCalledWith("");
  });

  it('should close the dialog with true when the Delete button is clicked', () => {
    const deleteButton = fixture.debugElement.query(By.css('button[color="warn"]')).nativeElement;
    deleteButton.click();
    fixture.detectChanges();
    expect(mockDialogRef.close).toHaveBeenCalledWith(true);
  });
});
