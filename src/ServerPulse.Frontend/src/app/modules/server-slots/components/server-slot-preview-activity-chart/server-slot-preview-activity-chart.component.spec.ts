import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ServerSlotDailyChartComponent } from './server-slot-preview-activity-chart.component';

describe('ServerSlotDailyChartComponent', () => {
  let component: ServerSlotDailyChartComponent;
  let fixture: ComponentFixture<ServerSlotDailyChartComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ServerSlotDailyChartComponent]
    })
      .compileComponents();

    fixture = TestBed.createComponent(ServerSlotDailyChartComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
