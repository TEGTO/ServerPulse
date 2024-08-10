import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ServerSlotInfoStatsComponent } from './server-slot-info-stats.component';

describe('ServerSlotInfoStatsComponent', () => {
  let component: ServerSlotInfoStatsComponent;
  let fixture: ComponentFixture<ServerSlotInfoStatsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ServerSlotInfoStatsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ServerSlotInfoStatsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
