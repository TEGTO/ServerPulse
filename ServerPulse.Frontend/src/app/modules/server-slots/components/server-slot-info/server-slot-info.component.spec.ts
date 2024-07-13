import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ServerSlotInfoComponent } from './server-slot-info.component';

describe('ServerSlotInfoComponent', () => {
  let component: ServerSlotInfoComponent;
  let fixture: ComponentFixture<ServerSlotInfoComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ServerSlotInfoComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ServerSlotInfoComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
