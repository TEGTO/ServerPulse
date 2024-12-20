import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ServerSlotActivityPreviewComponent } from './server-slot-activity-preview.component';

describe('ServerSlotActivityPreviewComponent', () => {
  let component: ServerSlotActivityPreviewComponent;
  let fixture: ComponentFixture<ServerSlotActivityPreviewComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ServerSlotActivityPreviewComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ServerSlotActivityPreviewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
