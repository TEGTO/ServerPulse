import { ComponentFixture, TestBed } from '@angular/core/testing';

import { InfoAnnotatedComponent } from './info-annotated.component';

describe('InfoAnnotatedComponent', () => {
  let component: InfoAnnotatedComponent;
  let fixture: ComponentFixture<InfoAnnotatedComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [InfoAnnotatedComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(InfoAnnotatedComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
