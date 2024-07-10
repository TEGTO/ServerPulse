import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ErrorAnnotatedComponent } from './error-annotated.component';

describe('ErrorAnnotatedComponent', () => {
  let component: ErrorAnnotatedComponent;
  let fixture: ComponentFixture<ErrorAnnotatedComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ErrorAnnotatedComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ErrorAnnotatedComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
