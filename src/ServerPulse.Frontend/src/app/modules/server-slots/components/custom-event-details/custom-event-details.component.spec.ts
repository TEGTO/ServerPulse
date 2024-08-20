import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CustomEventDetailsComponent } from './custom-event-details.component';

describe('CustomEventDetailsComponent', () => {
  let component: CustomEventDetailsComponent;
  let fixture: ComponentFixture<CustomEventDetailsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [CustomEventDetailsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CustomEventDetailsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
