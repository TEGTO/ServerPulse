import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NO_ERRORS_SCHEMA } from '@angular/core';
import { MainViewComponent } from './main-view.component';

describe('MainViewComponent', () => {
  let component: MainViewComponent;
  let fixture: ComponentFixture<MainViewComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [MainViewComponent],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();
    fixture = TestBed.createComponent(MainViewComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
