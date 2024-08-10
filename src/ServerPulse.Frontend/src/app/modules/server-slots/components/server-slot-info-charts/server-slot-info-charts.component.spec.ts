import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ServerSlotInfoChartsComponent } from './server-slot-info-charts.component';

describe('ServerSlotInfoChartsComponent', () => {
  let component: ServerSlotInfoChartsComponent;
  let fixture: ComponentFixture<ServerSlotInfoChartsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ServerSlotInfoChartsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ServerSlotInfoChartsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
