import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SlotBoardComponent } from './slot-board.component';

describe('SlotBoardComponent', () => {
  let component: SlotBoardComponent;
  let fixture: ComponentFixture<SlotBoardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [SlotBoardComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SlotBoardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
