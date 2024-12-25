import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { Store } from '@ngrx/store';
import { downloadSlotStatistics } from '../../../analyzer';
import { ServerSlotInfoDownloadComponent } from './server-slot-info-download.component';

describe('ServerSlotInfoDownloadComponent', () => {
  let component: ServerSlotInfoDownloadComponent;
  let fixture: ComponentFixture<ServerSlotInfoDownloadComponent>;
  let mockStore: jasmine.SpyObj<Store>;

  beforeEach(() => {
    mockStore = jasmine.createSpyObj<Store>(['dispatch', 'select']);

    TestBed.configureTestingModule({
      declarations: [ServerSlotInfoDownloadComponent],
      providers: [
        { provide: Store, useValue: mockStore },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ServerSlotInfoDownloadComponent);
    component = fixture.componentInstance;
    component.slotKey = 'test-slot-key';
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should dispatch downloadSlotStatistics action when the button is clicked', () => {
    const button = fixture.debugElement.query(By.css('button'));
    button.triggerEventHandler('click', null);

    expect(mockStore.dispatch).toHaveBeenCalledWith(downloadSlotStatistics({ key: 'test-slot-key' }));
  });
});