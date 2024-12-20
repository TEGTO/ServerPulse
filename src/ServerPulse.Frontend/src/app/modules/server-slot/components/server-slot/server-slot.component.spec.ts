import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ServerSlotComponent } from './server-slot.component';

describe('ServerSlotComponent', () => {
  let component: ServerSlotComponent;
  let fixture: ComponentFixture<ServerSlotComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ServerSlotComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ServerSlotComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
