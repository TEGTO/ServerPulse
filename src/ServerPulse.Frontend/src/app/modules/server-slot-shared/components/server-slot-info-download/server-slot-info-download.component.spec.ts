import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ServerSlotInfoDownloadComponent } from './server-slot-info-download.component';

describe('ServerSlotInfoDownloadComponent', () => {
  let component: ServerSlotInfoDownloadComponent;
  let fixture: ComponentFixture<ServerSlotInfoDownloadComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ServerSlotInfoDownloadComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ServerSlotInfoDownloadComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
