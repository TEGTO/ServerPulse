import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ServerSlotEditComponent } from './server-slot-edit.component';

describe('ServerSlotEditComponent', () => {
  let component: ServerSlotEditComponent;
  let fixture: ComponentFixture<ServerSlotEditComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ServerSlotEditComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ServerSlotEditComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
