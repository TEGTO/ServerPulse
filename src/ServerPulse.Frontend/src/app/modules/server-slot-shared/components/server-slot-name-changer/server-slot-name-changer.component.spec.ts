import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ServerSlotNameChangerComponent } from './server-slot-name-changer.component';

describe('ServerSlotNameChangerComponent', () => {
  let component: ServerSlotNameChangerComponent;
  let fixture: ComponentFixture<ServerSlotNameChangerComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ServerSlotNameChangerComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ServerSlotNameChangerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
