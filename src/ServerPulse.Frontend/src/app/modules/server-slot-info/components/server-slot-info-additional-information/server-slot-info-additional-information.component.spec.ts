import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ServerSlotInfoAdditionalInformationComponent } from './server-slot-info-additional-information.component';

describe('ServerSlotInfoAdditionalInformationComponent', () => {
  let component: ServerSlotInfoAdditionalInformationComponent;
  let fixture: ComponentFixture<ServerSlotInfoAdditionalInformationComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ServerSlotInfoAdditionalInformationComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ServerSlotInfoAdditionalInformationComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
