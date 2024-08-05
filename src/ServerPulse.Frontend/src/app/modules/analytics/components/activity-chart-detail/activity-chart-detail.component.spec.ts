import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ActivityChartDetailComponent } from './activity-chart-detail.component';

describe('ActivityChartDetailComponent', () => {
  let component: ActivityChartDetailComponent;
  let fixture: ComponentFixture<ActivityChartDetailComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ActivityChartDetailComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ActivityChartDetailComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
