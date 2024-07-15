import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ServerSlotDeleteConfirmComponent } from './server-slot-delete-confirm.component';

describe('ServerSlotDeleteConfirmComponent', () => {
  let component: ServerSlotDeleteConfirmComponent;
  let fixture: ComponentFixture<ServerSlotDeleteConfirmComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ServerSlotDeleteConfirmComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ServerSlotDeleteConfirmComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
