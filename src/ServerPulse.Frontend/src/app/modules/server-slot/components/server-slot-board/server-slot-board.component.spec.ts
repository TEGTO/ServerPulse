import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ServerSlotBoardComponent } from './server-slot-board.component';

describe('ServerSlotBoardComponent', () => {
  let component: ServerSlotBoardComponent;
  let fixture: ComponentFixture<ServerSlotBoardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ServerSlotBoardComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ServerSlotBoardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
