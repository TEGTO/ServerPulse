import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ServerSlotAdditionalInfromationComponent } from './server-slot-additional-infromation.component';

describe('ServerSlotAdditionalInfromationComponent', () => {
  let component: ServerSlotAdditionalInfromationComponent;
  let fixture: ComponentFixture<ServerSlotAdditionalInfromationComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ServerSlotAdditionalInfromationComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ServerSlotAdditionalInfromationComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
